﻿namespace Uno.Extensions.Navigation;

public static class FrameworkElementExtensions
{
	public static bool DisableLoadWaiting { get; set; }

	/// <summary>
	/// Attaches the specified IServiceProvider instance to the <paramref name="element"/> using the
	/// Region.ServiceProvider attached property. Any child element can access the IServiceProvider
	/// instance by traversing up the ancestor hierarchy
	/// </summary>
	/// <param name="element">The UIElement to attach the IServiceProvider instance to</param>
	/// <param name="services">The IServiceProvider instance</param>
	/// <returns>The attached IServiceProvider instance - scoped for use in this visual hierarchy</returns>
	public static IServiceProvider AttachServiceProvider(this UIElement element, IServiceProvider services)
	{
		var scopedServices = services.CreateScope().ServiceProvider;
		element.SetServiceProvider(scopedServices);
		return scopedServices;
	}

	public static async Task HostAsync(this FrameworkElement root, string? initialRoute = "", Type? initialView = null, Type? initialViewModel = null)
	{
		// Make sure the element has loaded and is attached to the visual tree
		await root.EnsureLoaded();

		Host(root, initialRoute, initialView, initialViewModel);
	}

	public static void Host(this FrameworkElement root, string? initialRoute = "", Type? initialView = null, Type? initialViewModel = null)
	{

		var sp = root.FindServiceProvider();
		var services = sp?.CreateNavigationScope();

		// Create the Root region
		var elementRegion = new NavigationRegion(root, services);

		var nav = elementRegion.Navigator();
		if (nav is not null)
		{
			var start = () => Task.CompletedTask;
			if (initialView is not null)
			{
				start = () => nav.NavigateViewAsync(root, initialView);
			}
			else if (initialViewModel is not null)
			{
				start = () => nav.NavigateViewModelAsync(root, initialViewModel);
			}
			else
			{
				start = () => nav.NavigateRouteAsync(root, initialRoute ?? string.Empty);
			}
			elementRegion.Services?.Startup(start);
		}
	}

	public static async Task Startup(this IServiceProvider services, Func<Task> afterStartup)
	{
		var startupServices = services
								.GetServices<IHostedService>()
									.Select(x => x as IStartupService)
									.Where(x => x is not null)
								.Union(services.GetServices<IStartupService>()).ToArray();

		var startServices = startupServices.Select(x => x?.StartupComplete() ?? Task.CompletedTask).ToArray();
		if (startServices?.Any() ?? false)
		{
			await Task.WhenAll(startServices);
		}
		await afterStartup();
	}

	public static async Task EnsureLoaded(this IRegion region)
	{
		if (DisableLoadWaiting)
		{
			return;
		}

		if (region.Services is not null)
		{
			return;
		}

		if (region?.View is null)
		{
			return;
		}

		await region.View.EnsureLoaded();

		if (region.Parent is null)
		{
			return;
		}

		await region.Parent.EnsureLoaded();
	}

	private static DispatcherQueue? GetDispatcher(this FrameworkElement? element) =>
#if WINUI
		element?.DispatcherQueue;
#else
		Windows.ApplicationModel.Core.CoreApplication.MainView.DispatcherQueue;
#endif

	public static async Task<bool> EnsureLoaded(this FrameworkElement? element)
	{
		if (element is null)
		{
			return false;
		}

		var dispatcher = element.GetDispatcher();
		var success = true;
		if (dispatcher is not null)
		{
			var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
			success = await dispatcher.ExecuteAsync(async () =>
			{
				try
				{
					await EnsureElementLoaded(element, timeoutToken);
					return true;
				}
				catch
				{
					if (timeoutToken.IsCancellationRequested)
					{
						return false;
					}
					else
					{
						throw;
					}
				}
			});
		}


#if __ANDROID__
		// EnsureLoaded can return from LayoutUpdated causing the remaining task to continue from the measure pass.
		// This is problematic as modifying the visual tree during that moment
		// could potentially leave the visual tree in a broken state.
		// By yielding here, we avoid such situation from happening.
		await Task.Yield();
#endif

		return success;
	}
	private static Task EnsureElementLoaded(this FrameworkElement? element, CancellationToken? timeoutToken = null)
	{
		if (element == null)
		{
			return Task.CompletedTask;
		}

		var completion = new TaskCompletionSource<object>();

		// Note: We're attaching to three different events to
		// a) always detect when element is loaded (sometimes Loaded is never fired)
		// b) detect as soon as IsLoaded is true (Loading and Loaded not always in right order)

		RoutedEventHandler? loaded = null;
		EventHandler<object>? layoutChanged = null;
		TypedEventHandler<FrameworkElement, object>? loading = null;

		CancellationTokenRegistration? rego = null;
		Action timeoutAction = () =>
		{
			rego?.Dispose();

			if (timeoutToken is not null)
			{
				completion.TrySetCanceled(timeoutToken.Value);
			}
		};

		rego = timeoutToken?.Register(timeoutAction);

		Action<bool> loadedAction = (overrideLoaded) =>
		{
			if (element.IsLoaded ||
				(element.ActualHeight > 0 && element.ActualWidth > 0))
			{
				rego?.Dispose();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
				completion.TrySetResult(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

				element.Loaded -= loaded;
				element.Loading -= loading;
				element.LayoutUpdated -= layoutChanged;
			}
		};

		loaded = (s, e) => loadedAction(false);
		loading = (s, e) => loadedAction(false);
		layoutChanged = (s, e) => loadedAction(true);

		element.Loaded += loaded;
		element.Loading += loading;
		element.LayoutUpdated += layoutChanged;

		if (element.IsLoaded ||
			(element.ActualHeight > 0 && element.ActualWidth > 0))
		{
			loadedAction(false);
		}

		return completion.Task;
	}

	public static void InjectServicesAndSetDataContext(
		this FrameworkElement view,
		IServiceProvider services,
		INavigator navigation,
		object? viewModel)
	{
		if (view is not null)
		{
			if (viewModel is not null &&
				view.DataContext != viewModel)
			{
				view.DataContext = viewModel;
			}
		}

		if (view is IInjectable<INavigator> navAware)
		{
			navAware.Inject(navigation);
		}

		if (view is IInjectable<IServiceProvider> spAware)
		{
			spAware.Inject(services);
		}
	}
}
