namespace IdentityLibrary.Services;

public interface IAuthService
{
	Task<SessionResult> CreateSessionAsync(string email);
	SessionResult VerifyEmail(string sessionId, string verificationCode);
	Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegistration userRegistration);
	Task<TokenGenerationResult> GenerateTokensAsync(Guid userId);
	Task<AuthenticationResult> AuthenticateAsync(UserLogin userLogin);
// 	Task<AuthenticationResult> ValidateTokensAsync(string accessTokenRequest, string refreshTokenRequest);
}