using System;

namespace IdentityApi.Results;

public class TokenGenerationResult
{
	public string AccessToken { get; set; } = default!;
	public Guid RefreshToken { get; set; }
}