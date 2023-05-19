using System;

namespace IdentityApi.Contracts.Options;

public class JwtOptions
{
	public const string Jwt = "Jwt";

	public string SecretKey { get; set; } = String.Empty;
	public string Issuer { get; set; } = String.Empty;
	public string Audience { get; set; } = String.Empty;
}