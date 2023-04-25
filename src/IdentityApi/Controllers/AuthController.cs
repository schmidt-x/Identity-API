namespace IdentityApi.Controllers;

[Produces("application/json")]
[ServiceFilter(typeof(ValidationActionFilter))]
[ApiController, Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;

	public AuthController(IAuthService authService)
	{
		_authService = authService;
	}
	
	
	/// <summary>
	/// Makes a registration session
	/// </summary>
	/// <param name="emailRegistration"></param>
	/// <response code="200">New session is made and the session id is retured in cookie</response>
	/// <response code="400">Email address is already in use or validation failed</response>
	[HttpPost("session")]
	[ProducesResponseType(typeof(SessionSuccessResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	public async Task<IActionResult> CreateUserSession(EmailRegistration emailRegistration)
	{
		var sessionResult = await _authService.CreateSessionAsync(emailRegistration.Email);
		
		if (!sessionResult.Succeeded)
			return BadRequest(new FailResponse { Errors = sessionResult.Errors });
		
		Response.Cookies.Append(
			"session_id",
			sessionResult.Id, // should I convert it into Base64?
			new()
			{
				Secure = true,
				HttpOnly = true,
				Expires = DateTimeOffset.UtcNow.AddMinutes(5)
			}
		);
		
		return Ok(new SessionSuccessResponse
		{
			Message = "Verification code is sent to your email"
		});
	}
	
	
	/// <summary>
	/// Verifies an email
	/// </summary>
	/// <param name="codeVerification"></param>
	/// <response code="200">Email address is successfully verified</response>
	/// <response code="400">Email verification or validation failed</response>
	[HttpPost("verification")]
	[ProducesResponseType(typeof(SessionSuccessResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	[ServiceFilter(typeof(SessionCookieActionFilter))]
	public IActionResult VerifyEmail(CodeVerification codeVerification)
	{
		var id = (string) HttpContext.Items["sessionId"]!;
		
		var sessionResult = _authService.VerifyEmail(id, codeVerification.Code);
		
		if (!sessionResult.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = sessionResult.Errors});
		}
		
		return Ok(new SessionSuccessResponse
		{
			Message = "Email address has successfully been verified"
		});
	}
	
	/// <summary>
	/// Registers a user
	/// </summary>
	/// <param name="userRegistration"></param>
	/// <response code="200">User is successfully registered</response>
	/// <response code="400">User already exists or validation failed</response>
	[HttpPost("registration")]
	[ServiceFilter(typeof(SessionCookieActionFilter))]
	[ProducesResponseType(typeof(AuthSuccessResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	public async Task<IActionResult> Register(UserRegistration userRegistration)
	{
		var id = (string) HttpContext.Items["sessionId"]!;
		
		var authenticationResult = await _authService.RegisterAsync(id, userRegistration);
		
		if (!authenticationResult.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = authenticationResult.Errors });
		}
		
		var tokensResult = await _authService.GenerateTokensAsync(authenticationResult.UserId);
		
		Response.Cookies.Delete("session_id"); // should I?
		
		Response.Cookies.Append(
			"jwt_refresh_token",
			tokensResult.RefreshToken.ToString(), // should I convert it into Base64 ?
			new()
			{
				HttpOnly = true,
				Secure = true,
				Expires = DateTimeOffset.UtcNow.AddMonths(6)
			});
		
		return Ok(new AuthSuccessResponse
		{
			Message = "You have successfully registered",
			AccessToken = tokensResult.AccessToken
		});
	}
	
	/// <summary>
	/// Logs in a user
	/// </summary>
	/// <param name="userLogin"></param>
	/// <response code="200">User logged in</response>
	/// <response code="400">Validation failed</response>
	/// <response code="401">User not found</response>
	[HttpPost("login")]
	[ProducesResponseType(typeof(AuthSuccessResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	[ProducesResponseType(typeof(FailResponse), 401)]
	public async Task<IActionResult> Login(UserLogin userLogin)
	{
		var authenticationResult = await _authService.AuthenticateAsync(userLogin);
		
		if (!authenticationResult.Succeeded)
		{
			return Unauthorized(new FailResponse { Errors = authenticationResult.Errors });
		}
		
		var tokensResult = await _authService.GenerateTokensAsync(authenticationResult.UserId);
		
		Response.Cookies.Delete("session_id");
		
		Response.Cookies.Append(
			"jwt_refresh_token",
			tokensResult.RefreshToken.ToString(),
			new()
			{
				HttpOnly = true,
				Secure = true,
				Expires = DateTimeOffset.UtcNow.AddMonths(6)
			}
		);
		
		return Ok(new AuthSuccessResponse
		{
			Message = "You have successfully logged in",
			AccessToken = tokensResult.AccessToken,
		});
	} 
	
	
	
// 	
// 	[HttpGet("refresh_token")]
// 	public async Task<IActionResult> Refresh()
// 	{
// 		var tokensResult = _authService.ExtractTokens(Request);
// 		
// 		if (!tokensResult.Success)
// 			return BadRequest(new AuthFailedResponse { Errors = tokensResult.Errors });
// 		
// 		var authResult = await _authService.ValidateTokensAsync(tokensResult.AccessToken, tokensResult.RefreshToken);
// 		
// 		if (!authResult.Success)
// 			return BadRequest(new AuthFailedResponse { Errors = authResult.Errors });
// 		
// 		var tokenGenerationResult = await _authService.GenerateTokensAsync(authResult.UserClaims);
// 		
// 		_authService.SetRefreshTokenCookie(tokenGenerationResult.RefreshToken, Response);
// 		
// 		return Ok(new AuthSuccessResponse { AccessToken = tokenGenerationResult.AccessToken });
// 	}
// 	
}