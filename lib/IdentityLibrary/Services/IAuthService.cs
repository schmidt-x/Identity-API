namespace IdentityLibrary.Services;

public interface IAuthService
{
	Task<SessionResult> CreateSessionAsync(string email);
	SessionResult VerifyEmail(string sessionId, string verificationCode);
	Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegister userRegister);
	Task<TokenGenerationResult> GenerateTokensAsync(Guid userId);
// 	void SetHttpSecureCookie(string key, string value, DateTimeOffset expires);
// 	SessionResult VerifyCode(string verificaitonCode);
// 	Task<AuthenticationResult> RegisterAsync(UserRegister userRegister);
// 	Task<AuthenticationResult> AuthenticateAsync(UserLogin userRegister);
// 	Task<TokenGenerationResult> GenerateTokensAsync(Guid userId);
// 	Task<AuthenticationResult> ValidateTokensAsync(string accessTokenRequest, string refreshTokenRequest);
// 	TokenExtractionResult ExtractTokens(HttpRequest request);
}