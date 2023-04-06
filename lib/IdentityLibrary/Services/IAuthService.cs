namespace IdentityLibrary.Services;

public interface IAuthService
{
	Task<AuthenticationResult> RegisterAsync(UserRegister userRegister);
	Task<AuthenticationResult> AuthenticateAsync(UserLogin userRegister);
	Task<AuthenticationResult> ValidateTokensAsync(string accessTokenRequest, string RefreshTokenRequest);
	void SetRefreshToken(string refreshToken, HttpResponse response);
	Task<TokenGenerationResult> GenerateTokensAsync(UserClaims claims);
	TokenExtractionResult ExtractTokens(HttpRequest request); 
}