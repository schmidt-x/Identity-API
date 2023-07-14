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
}