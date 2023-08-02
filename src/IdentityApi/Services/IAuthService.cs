using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Models;
using IdentityApi.Results;

namespace IdentityApi.Services;

public interface IAuthService
{
	Task<SessionResult> CreateSessionAsync(string email, CancellationToken ct);
	ResultEmpty VerifyEmail(string sessionId, string verificationCode);
	Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegistration userRegistration, CancellationToken ct);
	Task<AuthenticationResult> AuthenticateAsync(UserLogin userLogin, CancellationToken ct);
	Task<TokenGenerationResult> GenerateTokensAsync(UserClaims user, CancellationToken ct);
	Task<AuthenticationResult> ValidateTokensAsync(TokenRefreshing tokens, CancellationToken ct);
	
	/// <summary>
	/// Updates 'email' claim in Jwt access token and recomputes the signature
	/// </summary>
	/// <param name="oldToken">Jwt access token to update</param>
	/// <param name="newEmail">New email address to replace</param>
	/// <returns>New Jwt access token with updated 'email' claim</returns>
	public string UpdateAccessTokenEmail(string oldToken, string newEmail);
}