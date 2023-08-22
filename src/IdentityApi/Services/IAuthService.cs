using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.Requests;
using IdentityApi.Models;
using IdentityApi.Results;

namespace IdentityApi.Services;

public interface IAuthService
{
	Task<SessionResult> CreateSessionAsync(string email, CancellationToken ct);
	ResultEmpty VerifyEmail(string sessionId, string verificationCode);
	Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegistrationRequest registrationRequest, CancellationToken ct);
	Task<AuthenticationResult> AuthenticateAsync(UserLoginRequest loginRequest, CancellationToken ct);
	Task<TokenGenerationResult> GenerateTokensAsync(UserClaims user, CancellationToken ct);
	Task<AuthenticationResult> ValidateTokensAsync(TokenRefreshingRequest tokens, CancellationToken ct);
}