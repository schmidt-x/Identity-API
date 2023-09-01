using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.Requests;
using IdentityApi.Domain.Models;
using IdentityApi.Results;

namespace IdentityApi.Services;

public interface IAuthService
{
	Task<SessionResult> CreateRegistrationSessionAsync(string email, CancellationToken ct);
	Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegistrationRequest registrationRequest, CancellationToken ct);
	Task<AuthenticationResult> AuthenticateAsync(UserLoginRequest loginRequest, CancellationToken ct);
	Task<TokensResult> GenerateTokensAsync(UserClaims user, CancellationToken ct);
	Task<AuthenticationResult> ValidateTokensAsync(TokenRefreshingRequest tokens, CancellationToken ct);
	
	// Task<SessionResult> CreateForgotPasswordSessionAsync(string email, CancellationToken ct);
}