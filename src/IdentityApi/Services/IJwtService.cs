using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IdentityApi.Domain.Models;
using IdentityApi.Results;

namespace IdentityApi.Services;

public interface IJwtService
{
	/// <summary>
	/// Generated Access and Refresh tokens
	/// </summary>
	/// <param name="claims"><see cref="UserClaims"/> that are to be included into payload of Jwt access token</param>
	/// <returns>Generated <see cref="Tokens"/></returns>
	Tokens GenerateTokens(UserClaims claims);
	
	/// <summary>
	/// Validates Jwt access token
	/// </summary>
	/// <param name="token">Access token to validate</param>
	/// <param name="jwtSecurityToken">If succeeded, <see cref="JwtSecurityToken"/> of the validated token is stored. Otherwise, null </param>
	/// <returns>If succeeded, <see cref="ClaimsPrincipal"/> of the validated token. Otherwise, null</returns>
	ClaimsPrincipal? ValidateTokenExceptLifetime(string token, out JwtSecurityToken? jwtSecurityToken);
	
	/// <summary>
	/// Validates refresh token
	/// </summary>
	/// <param name="refreshToken"><see cref="RefreshToken"/> to validate</param>
	/// <param name="jti">The unique identifier (jti) of the associated access token</param>
	/// <returns><see cref="ValidationResult"/></returns>
	ValidationResult ValidateRefreshToken(RefreshToken refreshToken, Guid jti);
	
	/// <summary>
	/// Gets the total duration of a Jwt access token
	/// (clock skew is also considered)
	/// </summary>
	TimeSpan TotalExpirationTime { get; }
	
	/// <summary>
	/// Updates payload and reissues new Jwt access token
	/// </summary>
	/// <param name="jwtToken">Jwt token to update</param>
	/// <param name="newJti">New Jti to replace</param>
	/// <param name="newEmail">New email address to replace (optional)</param>
	/// <returns>Updated Jwt token</returns>
	string UpdateToken(string jwtToken, Guid newJti, string? newEmail = null);
	
	/// <summary>
	/// Calculates the remaining seconds of validity for a Jwt access token
	/// based on its expiration time in total seconds
	/// </summary>
	/// <param name="exp">The expiration time of the Jwt access token</param>
	/// <returns>The number of seconds left until the access token expires.
	/// If the token is already expired, 0 is returned</returns>
	long GetSecondsLeft(long exp);
	
	/// <summary>
	/// Indicates if a Jwt access token is expired based on its expiration time in total seconds
	/// </summary>
	/// <param name="exp">The expiration time of the Jwt access token</param>
	/// <param name="secondsLeft">Variable to store the number of seconds left until the access token expires.
	/// If it's already expired, 0 is stored</param>
	/// <returns>True if token is expired</returns>
	public bool IsExpired(long exp, out long secondsLeft);
}