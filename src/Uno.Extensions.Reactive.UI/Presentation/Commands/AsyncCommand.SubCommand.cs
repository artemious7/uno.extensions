using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uno.Extensions.Reactive.Core;

namespace Uno.Extensions.Reactive;

partial class AsyncCommand
{
	private sealed class SubCommand
	{
		private readonly CommandConfig _config;
		private readonly AsyncCommand _command;

		private (object? value, bool isValid)? _externalParameter;

		public SubCommand(CommandConfig config, AsyncCommand command)
		{
			_config = config;
			_command = command;
		}

		public async Task SubscribeToParameter(SourceContext context, CancellationToken ct)
		{
			// This method is **hopefully** run on the UI thread  so we use .ConfigureAwait(true)
			// in order to reduce threading issues for _externalParameter and thread changes for UpdateCanExecute().

			if (_config.Parameter is null)
			{
				return;
			}

			_externalParameter = (null, false);
			_command.UpdateCanExecute();

			var parameters = _config
				.Parameter(context)
				.Where(message => message.Changes.Contains(MessageAxis.Data))
				.WithCancellation(ct)
				.ConfigureAwait(true);
			await foreach (var parameter in parameters)
			{
				bool isValid, wasValid = _externalParameter is { isValid: true };
				try
				{
					isValid = parameter.Current.Data.IsSome(out var value)
						&& (_config.CanExecute?.Invoke(value) ?? true);

					_externalParameter = (value, isValid);
				}
				catch (Exception error)
				{
					_command.ReportError(error, when: "validating can execute of external parameter");
					_externalParameter = (null, isValid = false);
				}

				if (wasValid != isValid)
				{
					_command.UpdateCanExecute();
				}
			}
		}

		public bool CanExecute(object? parameter)
		{
			if (_externalParameter is { } externalParameter)
			{
				if (!externalParameter.isValid)
				{
					return false;
				}

				parameter = externalParameter.value;
			}

			return !_command.IsExecutingFor(parameter) && (_config.CanExecute?.Invoke(parameter) ?? true);
		}

		public bool TryExecute(object? parameter, SourceContext context, CancellationToken ct)
		{
			if (_externalParameter is { } externalParameter)
			{
				if (!externalParameter.isValid)
				{
					return false;
				}

				parameter = externalParameter.value;
			}

			if (!(_config.CanExecute?.Invoke(parameter) ?? true))
			{
				return false;
			}

			_command.ReportExecutionStarting(parameter);

			Task.Run(
					async () =>
					{
						try
						{
							using var _ = context.AsCurrent();
							await _config.Execute(parameter, _command._ct.Token);
						}
						catch (Exception error)
						{
							_command.ReportError(error, when: $"executing command with '{parameter ?? "-null-"}'");
						}
					},
					_command._ct.Token)
				.ContinueWith((_, state) =>
					{
						try
						{
							var (command, arg) = ((AsyncCommand, object?))state!;
							command.ReportExecutionEnded(arg);
						}
						catch (Exception) { } // Almost impossible, but an error here would crash the app
					},
					(_command, parameter),
					TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously);

			return true;
		}
	}
}
