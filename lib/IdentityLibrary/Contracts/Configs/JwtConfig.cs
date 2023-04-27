﻿namespace IdentityLibrary.Contracts.Configs;

public class JwtConfig
{
	public string SecretKey { get; set; }
	public string Issuer { get; set; }
	public string Audience { get; set; }
	public TokenValidationParameters Parameters { get; set; }
}