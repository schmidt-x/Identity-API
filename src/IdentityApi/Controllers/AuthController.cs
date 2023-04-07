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
	public IActionResult Register(UserRegister userRegister)
	{
		// var authenticationResult = await _authService.RegisterAsync(userRegister);
		//
		// if (!authenticationResult.Success)
		// 	return BadRequest(new AuthFailedResponse { Errors = authenticationResult.Errors });
		//
		// var tokenGenerationResult = await _authService.GenerateTokensAsync(authenticationResult.UserClaims);
		//
		// _authService.SetRefreshToken(tokenGenerationResult.RefreshToken, Response);
		//
		// return Ok(new AuthSuccessResponse { AccessToken = tokenGenerationResult.AccessToken });
		
		throw new NotImplementedException();
	}
	
	[HttpPost("login")]
	public IActionResult Login(UserLogin userLogin)
	{
		
		
		
		throw new NotImplementedException();
	}
}

