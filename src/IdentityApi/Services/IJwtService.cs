using System;

namespace IdentityApi.Services;

public interface IJwtService
{
	/// <summary>
	/// Updates payload and reissues new Jwt access token
	/// </summary>
	/// <param name="jwtToken">Jwt token to update</param>
	/// <param name="newJti">New Jti to update</param>
	/// <param name="newEmail">New email address to update (optional)</param>
	/// <returns>Jwt access token</returns>
	string UpdateToken(string jwtToken, Guid newJti, string? newEmail = null);
	
	
}