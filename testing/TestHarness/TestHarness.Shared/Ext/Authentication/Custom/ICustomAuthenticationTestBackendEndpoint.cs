﻿

namespace TestHarness.Ext.Authentication.Custom;

[Headers("Content-Type: application/json")]
public interface ICustomAuthenticationTestBackendEndpoint
{
	[Get("/customauth/login")]
	Task<CustomAuthenticationCustomAuthResponse> Login([Query] string username, [Query] string password, CancellationToken ct);

	[Post("/customauth/logincookie")]
	Task LoginCookie([Query] string username, [Query] string password, CancellationToken ct);

	[Post("/customauth/refreshcookie")]
	Task RefreshCookie(CancellationToken ct);


	[Get("/customauth/getdataauthorizationheader")]
	Task<IEnumerable<string>?> GetDataAuthorizationHeader(CancellationToken ct);

	[Get("/customauth/getdatacookie")]
	Task<IEnumerable<string>?> GetDataCookie(CancellationToken ct);
}

public class CustomAuthenticationCustomAuthResponse
{
	public string? AccessToken { get; init; }
}
