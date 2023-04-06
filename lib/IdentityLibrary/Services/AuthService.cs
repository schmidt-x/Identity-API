using IdentityLibrary.Repositories;

namespace IdentityLibrary.Services;

public class AuthService : IAuthService
{
	private readonly IUserRepository _userRepo;
	public AuthService(IConfiguration config, IUserRepository userRepo)
	{
		_userRepo = userRepo;
	}
	
	
	public Task<AuthenticationResult> RegisterAsync(UserRegister userRegister)
	{
		throw new System.NotImplementedException();
	}

	public Task<AuthenticationResult> AuthenticateAsync(UserLogin userRegister)
	{
		throw new System.NotImplementedException();
	}

	public Task<AuthenticationResult> ValidateTokensAsync(string accessTokenRequest, string RefreshTokenRequest)
	{
		throw new System.NotImplementedException();
	}

	public void SetRefreshToken(string refreshToken, HttpResponse response)
	{
		throw new System.NotImplementedException();
	}

	public Task<TokenGenerationResult> GenerateTokensAsync(UserClaims claims)
	{
		throw new System.NotImplementedException();
	}

	public TokenExtractionResult ExtractTokens(HttpRequest request)
	{
		throw new System.NotImplementedException();
	}
}