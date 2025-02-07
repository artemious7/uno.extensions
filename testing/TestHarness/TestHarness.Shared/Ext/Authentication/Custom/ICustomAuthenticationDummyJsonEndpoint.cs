﻿namespace TestHarness.Ext.Authentication.Custom;

public static class DummyJsonEndpointConstants
{
	// See https://dummyjson.com/docs/auth for username/password values
	public const string ValidUserName = "kminchelle";
	public const string ValidPassword = "0lelplR";
}

[Headers("Content-Type: application/json")]
public interface ICustomAuthenticationDummyJsonEndpoint
{
	[Post("/auth/login")]
	Task<CustomAuthenticationAuthResponse> Login(CustomAuthenticationCredentials credentials, CancellationToken ct);

	[Get("/products")]
	Task<CustomAuthenticationProductsResponse> Products(CancellationToken ct);

}

public class CustomAuthenticationCredentials
{
	[JsonPropertyName("username")]
	public string? Username { get; init; }
	[JsonPropertyName("password")]
	public string? Password { get; init; }
}

public class CustomAuthenticationAuthResponse
{
	[JsonPropertyName("token")]
	public string? Token { get; set; }
}

public class CustomAuthenticationProductsResponse : BaseCustomAuthenticationResponse
{
	[JsonPropertyName("products")]
	public CustomAuthenticationProduct[]? Products { get; set; }

}

public class BaseCustomAuthenticationResponse
{
	[JsonPropertyName("total")]
	public int Total { get; set; }
	[JsonPropertyName("skip")]
	public int Skip { get; set; }
	[JsonPropertyName("limit")]
	public int Limit { get; set; }
}

public class CustomAuthenticationProduct
{
	[JsonPropertyName("title")]
	public string? Title { get; set; }
}
