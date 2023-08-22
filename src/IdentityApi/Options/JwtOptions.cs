using System;

namespace IdentityApi.Options;

public class JwtOptions
{
	public const string Jwt = "Jwt";
	
	public string SecretKey { get; set; } = String.Empty;
	public string Issuer { get; set; } = String.Empty;
	public string Audience { get; set; } = String.Empty;
	public TimeSpan AccessTokenLifeTime { get; set; }
	public TimeSpan RefreshTokenLifeTime { get; set; }
}