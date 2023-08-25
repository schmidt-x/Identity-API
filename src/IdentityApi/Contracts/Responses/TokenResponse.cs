using System;

namespace IdentityApi.Contracts.Responses;

public class TokenResponse
{
	public string AccessToken { get; set; } = default!;
	public Guid RefreshToken { get; set; }
}