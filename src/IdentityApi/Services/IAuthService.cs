using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.Requests;
using IdentityApi.Domain.Models;
using IdentityApi.Results;

namespace IdentityApi.Services;

public interface IAuthService
{
	Task<SessionResult> CreateRegistrationSessionAsync(string email, CancellationToken ct);
	Task<AuthResult> RegisterAsync(string sessionId, UserRegistrationRequest registrationRequest, CancellationToken ct);
	Task<AuthResult> AuthenticateAsync(UserLoginRequest loginRequest, CancellationToken ct);
	Task<TokensResult> GenerateTokensAsync(UserClaims user, CancellationToken ct);
	Task<AuthResult> ValidateTokensAsync(TokenRefreshingRequest tokens, CancellationToken ct);
	Task<SessionResult> CreateForgotPasswordSessionAsync(string email, CancellationToken ct);
	Task<AuthResult> RestorePasswordAsync(string sessionId, string newPassword, CancellationToken ct);	
}