using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Filters;
using IdentityApi.Responses;
using IdentityApi.Services;
using Microsoft.AspNetCore.Mvc;

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
	/// <response code="200">New session is made and the session id is retured in cookie</response>
	/// <response code="400">Email address is already in use or validation failed</response>
	[HttpPost("session")]
	[ProducesResponseType(typeof(SessionSuccessResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	public async Task<IActionResult> CreateUserSession(EmailRegistration emailRegistration, CancellationToken ct)
	{
		var sessionResult = await _authService.CreateSessionAsync(emailRegistration.Email, ct);
		
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
	/// <response code="200">Email address is successfully verified</response>
	/// <response code="400">Email verification or validation failed</response>
	[HttpPost("verification")]
	[ServiceFilter(typeof(SessionCookieActionFilter))]
	[ProducesResponseType(typeof(SessionSuccessResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
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
	/// <response code="200">User is successfully registered</response>
	/// <response code="400">User already exists or validation failed</response>
	[HttpPost("registration")]
	[ServiceFilter(typeof(SessionCookieActionFilter))]
	[ProducesResponseType(typeof(AuthSuccessResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	public async Task<IActionResult> Register(UserRegistration userRegistration, CancellationToken ct)
	{
		var id = (string) HttpContext.Items["sessionId"]!;
		
		var authenticationResult = await _authService.RegisterAsync(id, userRegistration, ct);
		
		if (!authenticationResult.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = authenticationResult.Errors });
		}
		
		var tokensResult = await _authService.GenerateTokensAsync(authenticationResult.User, ct);
		
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
	/// <response code="200">User logged in</response>
	/// <response code="400">Validation failed</response>
	/// <response code="401">User not found</response>
	[HttpPost("login")]
	[ProducesResponseType(typeof(AuthSuccessResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	[ProducesResponseType(typeof(FailResponse), 401)]
	public async Task<IActionResult> Login(UserLogin userLogin, CancellationToken ct)
	{
		var authenticationResult = await _authService.AuthenticateAsync(userLogin, ct);
		
		if (!authenticationResult.Succeeded)
		{
			return Unauthorized(new FailResponse { Errors = authenticationResult.Errors });
		}
		
		var tokensResult = await _authService.GenerateTokensAsync(authenticationResult.User, ct);
		
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
	
	
	[HttpPost("refresh")]
	public async Task<IActionResult> RefreshToken(TokenRefreshing tokens, CancellationToken ct)
	{
		var validationResult = await _authService.ValidateTokensAsync(tokens, ct);
		
		if (!validationResult.Succeeded)
		{
			return Unauthorized(new FailResponse { Errors = validationResult.Errors });
		}
		
		var tokensResult = await _authService.GenerateTokensAsync(validationResult.User, ct);
		
		Response.Cookies.Append(
			"jwt_refresh_token",
			tokensResult.RefreshToken.ToString(),
			new()
			{
				HttpOnly = true,
				Secure = true,
				Expires = DateTimeOffset.UtcNow.AddMonths(6)
			});
		
		return Ok(new AuthSuccessResponse
		{
			Message = "You have successfully refreshed tokens",
			AccessToken = tokensResult.AccessToken
		});
	}
}