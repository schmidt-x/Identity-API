using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using IdentityApi.Domain.Constants;
using IdentityApi.Domain.Models;
using IdentityApi.Options;
using IdentityApi.Extensions;
using IdentityApi.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityApi.Services;

public class JwtService : IJwtService
{
	private readonly JwtOptions _jwtOptions;
	private readonly TokenValidationParameters _tokenValidationParameters;
	
	public JwtService(
		IOptions<JwtOptions> jwtOptions,
		TokenValidationParameters tokenValidationParameters)
	{
		_tokenValidationParameters = tokenValidationParameters;
		_jwtOptions = jwtOptions.Value;
	}


	public TimeSpan TotalExpirationTime => 
		_jwtOptions.AccessTokenLifeTime + _jwtOptions.ClockSkew;
	
	public Tokens GenerateTokens(UserClaims user)
	{
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		var handler = new JwtSecurityTokenHandler();
		
		var jti = Guid.NewGuid();
		var identity = new ClaimsIdentity(new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, jti.ToString()),
			new Claim(JwtRegisteredClaimNames.Email, user.Email)
		}, JwtBearerDefaults.AuthenticationScheme);
		
		var timeNow = DateTime.UtcNow;
		
		var descriptor = new SecurityTokenDescriptor
		{
			Subject = identity,
			Audience = _jwtOptions.Audience,
			Issuer = _jwtOptions.Issuer,
			Expires = timeNow.Add(_jwtOptions.AccessTokenLifeTime),
			SigningCredentials = credentials
		};
		
		var securityToken = handler.CreateToken(descriptor);
		
		var refreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			Jti = jti,
			CreatedAt = timeNow.GetTotalSeconds(),
			ExpiresAt = timeNow.Add(_jwtOptions.RefreshTokenLifeTime).GetTotalSeconds(),
			UserId = user.Id,
			Invalidated = false,
			Used = false
		};
		
		return new Tokens
		{
			AccessToken = handler.WriteToken(securityToken),
			RefreshToken = refreshToken
		};
	}
	
	public ClaimsPrincipal? ValidateTokenExceptLifetime(string token, out JwtSecurityToken? jwtSecurityToken)
	{
		var handler = new JwtSecurityTokenHandler();
		jwtSecurityToken = default;
		
		try
		{
			// do I have to say hello to race-condition since it's singleton?
			_tokenValidationParameters.ValidateLifetime = false;
			
			var claimsPrincipal = handler.ValidateToken(token, _tokenValidationParameters, out var securityToken);
			
			if (securityToken is not JwtSecurityToken _jwtSecurityToken || 
				!_jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
			{
				return null;
			}
			
			jwtSecurityToken = _jwtSecurityToken;
			return claimsPrincipal;
		}
		catch
		{
			return null;
		}
		finally
		{
			_tokenValidationParameters.ValidateLifetime = true;
		}
	}
	
	public ValidationResult ValidateRefreshToken(RefreshToken refreshToken, Guid jti)
	{
		if (refreshToken.Invalidated)
		{
			return ValidationResult.Fail(ErrorKey.RefreshToken, ErrorMessage.RefreshTokenInvalidated);
		}
		
		if (refreshToken.Used)
		{
			return ValidationResult.Fail(ErrorKey.RefreshToken, ErrorMessage.RefreshTokenUsed);
		}
		
		if (refreshToken.ExpiresAt < DateTime.UtcNow.GetTotalSeconds())
		{
			return ValidationResult.Fail(ErrorKey.RefreshToken, ErrorMessage.RefreshTokenExpired);
		}
		
		if (!refreshToken.Jti.Equals(jti))
		{
			return ValidationResult.Fail(ErrorKey.AccessToken, ErrorMessage.TokensNotMatch);
		}
		
		return ValidationResult.Success();
	}

	public string UpdateToken(string jwtToken, Guid newJti, string? newEmail = null)
	{
		var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
		var payload = jwtSecurityToken.Payload;
		
		payload[JwtRegisteredClaimNames.Jti] = newJti.ToString();
		
		if (newEmail is not null)
			payload[JwtRegisteredClaimNames.Email] = newEmail;
		
		object secondsNow = DateTime.UtcNow.GetTotalSeconds();

		payload[JwtRegisteredClaimNames.Iat] = secondsNow;
		payload[JwtRegisteredClaimNames.Nbf] = secondsNow;
		
		// I don't know how to name the part that consists of Header and Payload
		var newSomething = $"{jwtSecurityToken.EncodedHeader}.{jwtSecurityToken.EncodedPayload}";
		
		var newToken = newSomething + "." + jwtSecurityToken.SignatureAlgorithm switch
		{
			"HS256" => ComputeHashHS256(_jwtOptions.SecretKey, newSomething),
			_ => throw new NotImplementedException()
		};
		
		return newToken;
	}
	
	public long GetSecondsLeft(long exp)
	{
		var secondsLeft = CalculateSecondsLeft(exp);
		
		return secondsLeft > 0 ? secondsLeft : 0;
	}
	
	public bool IsExpired(long exp, out long secondsLeft)
	{
		var _secondsLeft = CalculateSecondsLeft(exp);
		
		if (_secondsLeft > 0)
		{
			secondsLeft = _secondsLeft;
			return false;
		}
		
		secondsLeft = 0;
		return true;
	}
	
	
	private long CalculateSecondsLeft(long exp)
	{
		var totalExpirationTime = exp + (long)_jwtOptions.ClockSkew.TotalSeconds;
		var secondsNow = DateTime.UtcNow.GetTotalSeconds();
		
		return  totalExpirationTime - secondsNow;
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