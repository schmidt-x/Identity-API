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
	/// <param name="emailRequest"></param>
	/// <response code="200">New session is made and the session id is retured in cookie</response>
	/// <response code="400">Validation failed or email address is already in use</response>
	[HttpPost("session")]
	[ProducesResponseType(typeof(SessionSuccessResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	public async Task<IActionResult> CreateUserSession(EmailRequest emailRequest)
	{
		var sessionResult = await _authService.CreateSessionAsync(emailRequest.Email);
		
		if (!sessionResult.Success)
			return BadRequest(new FailResponse { Errors = sessionResult.Errors });
		
		Response.Cookies.Append(
			"session_id",
			sessionResult.Id,
			new()
			{
				Secure = true,
				HttpOnly = true,
				Expires = DateTimeOffset.UtcNow.AddMinutes(5)
			}
		);
		
		return Ok(new SessionSuccessResponse { Message = "Verification code is sent to email" });
	}
	
	
	/// <summary>
	/// Verifies an email
	/// </summary>
	/// <param name="verificationRequest"></param>
	/// <response code="200">Email address is successfully verified</response>
	/// <response code="400">Validation or session verification failed</response>
	[HttpPost("verification")]
	[ProducesResponseType(typeof(SessionSuccessResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	[ServiceFilter(typeof(SessionCookieActionFilter))]
	public IActionResult VerifyEmail(VerificationRequest verificationRequest)
	{
		var id = (string) HttpContext.Items["sessionId"]!;
		
		var sessionResult = _authService.VerifyEmail(id, verificationRequest.Code);
		
		if (!sessionResult.Success)
		{
			return BadRequest(new FailResponse { Errors = sessionResult.Errors});
		}
		
		return Ok(new SessionSuccessResponse
		{
			Message = "Email address has successfully been verified"
		});
	}
	
	// /// <summary>
	// /// Registers the user
	// /// </summary>
	// /// <param name="userRegister"></param>
	// /// <returns></returns>
	// [HttpPost("registration")] 
	// [ProducesResponseType(typeof(AuthSuccessResponse), 200)]
	// [ProducesResponseType(typeof(AuthFailedResponse), 400)]
	// public async Task<IActionResult> Register(UserRegister userRegister)
	// {
	// 	var authResult = await _authService.RegisterAsync(userRegister);
	// 	
	// 	if (!authResult.Success)
	// 		return BadRequest(new AuthFailedResponse { Errors = authResult.Errors });
	// 	
	// 	var tokenGenerationResult = await _authService.GenerateTokensAsync(authResult.UserId);
	// 	
	// 	_authService.SetHttpSecureCookie(
	// 		"jwt_refresh_token",
	// 		tokenGenerationResult.RefreshToken.ToString(),
	// 		DateTimeOffset.UtcNow.AddMonths(6));
	// 	
	// 	return Ok(new AuthSuccessResponse
	// 	{
	// 		Message = "Registration is successfully completed",
	// 		AccessToken = tokenGenerationResult.AccessToken
	// 	});
	// }
	
// 	[HttpPost("login")]
// 	public async Task<IActionResult> Login(UserLogin userLogin)
// 	{
// 		var authResult = await _authService.AuthenticateAsync(userLogin);
// 		
// 		if (!authResult.Success)
// 			return Unauthorized(new AuthFailedResponse { Errors = authResult.Errors });
// 		
// 		var tokenGenerationResult = await _authService.GenerateTokensAsync(authResult.UserClaims);
// 		
// 		_authService.SetRefreshTokenCookie(tokenGenerationResult.RefreshToken, Response);
// 		
// 		return Ok(new AuthSuccessResponse { AccessToken = tokenGenerationResult.AccessToken });
// 	}
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