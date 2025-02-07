﻿namespace Uno.Extensions.Authentication.Handlers;

internal class HeaderHandler : BaseAuthorizationHandler
{
	public HeaderHandler(
		ILogger<BaseAuthorizationHandler> logger,
		IAuthenticationService authenticationService,
		ITokenCache tokens,
		HandlerSettings settings
	) : base(logger,
		authenticationService,
		tokens, settings)
	{
	}
	public override bool ShouldIncludeToken(HttpRequestMessage request) => true;

	protected override async Task<bool> ApplyTokensToRequest(HttpRequestMessage request, CancellationToken ct)
	{
		var accessToken = await _tokens.AccessTokenAsync();
		if (!string.IsNullOrWhiteSpace(accessToken) &&
			!string.IsNullOrWhiteSpace(_settings.AuthorizationHeaderScheme))
		{
			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_settings.AuthorizationHeaderScheme, accessToken);
			return true;
		}

		return false;
	}
}
