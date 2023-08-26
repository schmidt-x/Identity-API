using System;

namespace IdentityApi.Results;

public class TokensResult
{
	public string AccessToken { get; set; } = default!;
	public Guid RefreshToken { get; set; }
}