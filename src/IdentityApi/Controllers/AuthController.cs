namespace IdentityApi.Controllers;

[ApiController, Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;

	public AuthController(IAuthService authService)
	{
		_authService = authService;
	}
	
	
	[HttpPost("register")]
	public async Task<IActionResult> Register(UserRegister userRegister)
	{
		var authResult = await _authService.RegisterAsync(userRegister);
		
		if (!authResult.Success)
			return BadRequest(new AuthFailedResponse { Errors = authResult.Errors });
		
		var tokenGenerationResult = await _authService.GenerateTokensAsync(authResult.UserClaims);
		
		_authService.SetRefreshToken(tokenGenerationResult.RefreshToken, Response);
		
		return Ok(new AuthSuccessResponse { AccessToken = tokenGenerationResult.AccessToken });
	}
	
	[HttpPost("login")]
	public IActionResult Login(UserLogin userLogin)
	{
		
		
		
		throw new NotImplementedException();
	}
}

