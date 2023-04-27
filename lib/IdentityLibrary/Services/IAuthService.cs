namespace IdentityLibrary.Services;

public interface IAuthService
{
	Task<SessionResult> CreateSessionAsync(string email);
	SessionResult VerifyEmail(string sessionId, string verificationCode);
	Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegistration userRegistration);
	Task<AuthenticationResult> AuthenticateAsync(UserLogin userLogin);
	Task<TokenGenerationResult> GenerateTokensAsync(UserClaims user);
	Task<AuthenticationResult> ValidateTokensAsync(RefreshTokenRequest tokens);
// 	Task<AuthenticationResult> ValidateTokensAsync(string accessTokenRequest, string refreshTokenRequest);
}