﻿namespace Uno.Extensions;

public static class HostBuilderExtensions
{
	public static IHostBuilder UseHttp(
		this IHostBuilder hostBuilder,
		Action<IServiceCollection> configure)
	{
		return hostBuilder.UseHttp((context, builder) => configure.Invoke(builder));
	}

	public static IHostBuilder UseHttp(
		this IHostBuilder builder,
		Action<HostBuilderContext, IServiceCollection>? configure = default)
	{
		return builder
			.ConfigureServices((ctx, services) =>
		{
			_ = services
				.AddNativeHandler()
				.AddTransient<DelegatingHandler, DiagnosticHandler>();

			configure?.Invoke(ctx, services);
		});
	}
}
