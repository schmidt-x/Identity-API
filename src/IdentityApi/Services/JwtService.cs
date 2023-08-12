using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using IdentityApi.Contracts.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityApi.Services;

public class JwtService : IJwtService
{
	private readonly JwtOptions _jwt;
	
	public JwtService(IOptions<JwtOptions> jwtOptions)
	{
		_jwt = jwtOptions.Value;
	}


	// Note: Only the HS256 hashing algorithm is supported now
	// In other cases, NotImplementedException is thrown
	public string UpdateToken(string jwtToken, Guid newJti, string? newEmail = null)
	{
		var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
		var payload = jwtSecurityToken.Payload;
		
		payload[JwtRegisteredClaimNames.Jti] = newJti.ToString();
		
		if (newEmail is not null)
			payload[JwtRegisteredClaimNames.Email] = newEmail;
		
		object secondsNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

		payload[JwtRegisteredClaimNames.Iat] = secondsNow;
		payload[JwtRegisteredClaimNames.Nbf] = secondsNow;
		
		// I don't know how to name the part that consists of Header and Payload
		var newSomething = $"{jwtSecurityToken.EncodedHeader}.{jwtSecurityToken.EncodedPayload}";
		
		var newToken = newSomething + "." + jwtSecurityToken.SignatureAlgorithm switch
		{
			"HS256" => ComputeHashHS256(_jwt.SecretKey, newSomething),
			_ => throw new NotImplementedException()
		};
		
		return newToken;
	}
	
	
	/// <summary>
	/// Computes the HMAC-SHA256 hash of the given value using the provided secret key
	/// </summary>
	/// <param name="secretKey">Secret key used for hashing algorithm</param>
	/// <param name="value">Value to hash</param>
	/// <returns>Hashed value encoded to Base64Url string</returns>
	private static string ComputeHashHS256(string secretKey, string value)
	{
		var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
		var newSignature = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
		
		return Base64UrlEncoder.Encode(newSignature);
	}
}