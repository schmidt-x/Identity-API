using System;

namespace IdentityApi.Contracts.Responses;

public class AuthResponse
{
	public string Message { get; set; } = default!;
	public string AccessToken { get; set; } = default!;
	public Guid RefreshToken { get; set; }
}