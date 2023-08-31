using System;

namespace IdentityApi.Services;

public interface IJwtService
{
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
	/// <returns>The number of seconds left until the access token expires</returns>
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