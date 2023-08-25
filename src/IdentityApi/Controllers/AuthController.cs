using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.Requests;
using IdentityApi.Filters;
using IdentityApi.Contracts.Responses;
using IdentityApi.Domain.Constants;
using IdentityApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers;

[Consumes("application/json")]
[Produces("application/json")]
[ApiController, Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
	private readonly IAuthService _authService;
	private readonly IEmailSender _emailSender;

	public AuthController(IAuthService authService, IEmailSender emailSender)
	{
		_authService = authService;
		_emailSender = emailSender;
	}
	
	
	/// <summary>
	/// Sends verification code to email address
	/// </summary>
	/// <response code="200">Verification code is sent</response>
	/// <response code="400">Email address is already taken or invalid</response>
	[HttpPost("registration")]
	[ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.OK)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.BadRequest)]
	public async Task<IActionResult> CreateSession(EmailRequest emailRequest, CancellationToken ct)
	{
		var result = await _authService.CreateSessionAsync(emailRequest.Email, ct);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		var _ = _emailSender.SendAsync(emailRequest.Email, result.VerificationCode);
		
		Response.Cookies.Append(
			Key.CookieSessionId,
			result.Id,
			new()
			{
				Secure = true,
				HttpOnly = true,
				Expires = DateTimeOffset.UtcNow.AddMinutes(5)
			}
		);
		
		return Ok(new MessageResponse { Message = $"Verification code is sent to '{emailRequest.Email}' email address" });
	}
	
	/// <summary>
	/// Verifies email address
	/// </summary>
	/// <response code="200">Email address is successfully verified</response>
	/// <response code="400">Vefirication code is wrong</response>
	[HttpPost("registration/verify-email")]
	[ServiceFilter(typeof(SessionCookieActionFilter))]
	[ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.OK)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.BadRequest)]
	public IActionResult VerifyEmail(CodeVerificationRequest codeVerificationRequest)
	{
		var id = (string) HttpContext.Items[Key.SessionId]!;
		
		var sessionResult = _authService.VerifyEmail(id, codeVerificationRequest.Code);
		
		if (!sessionResult.Succeeded)
			return BadRequest(new FailResponse { Errors = sessionResult.Errors});
		
		// refresh cookie life-time
		Response.Cookies.Append(
			Key.CookieSessionId,
			id,
			new()
			{
				Secure = true,
				HttpOnly = true,
				Expires = DateTimeOffset.UtcNow.AddMinutes(5)
			}
		);
		
		return Ok(new MessageResponse { Message = "Email address successfully verified" });
	}
	
	/// <summary>
	/// Registers user
	/// </summary>
	/// <response code="200">Registration is successfuly completed</response>
	/// <response code="400">Username is already taken or validation failed</response>
	[HttpPost("registration/register")]
	[ServiceFilter(typeof(SessionCookieActionFilter))]
	[ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.OK)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.BadRequest)]
	public async Task<IActionResult> Register(UserRegistrationRequest userRegistrationRequest, CancellationToken ct)
	{
		var id = (string) HttpContext.Items[Key.SessionId]!;
		
		var result = await _authService.RegisterAsync(id, userRegistrationRequest, ct);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		var tokens = await _authService.GenerateTokensAsync(result.Claims, ct);
		
		Response.Cookies.Delete(Key.CookieSessionId);
		
		return Ok(new TokenResponse
		{
			AccessToken = tokens.AccessToken,
			RefreshToken = tokens.RefreshToken
		});
	}
	
	/// <summary>
	/// Logs in user
	/// </summary>
	/// <response code="200">User is logged in</response>
	/// <response code="400">Validation failed</response>
	/// <response code="401">Login/password are wrong</response>
	[HttpPost("login")]
	[ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.OK)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.BadRequest)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.Unauthorized)]
	public async Task<IActionResult> Login(UserLoginRequest userLoginRequest, CancellationToken ct)
	{
		var result = await _authService.AuthenticateAsync(userLoginRequest, ct);
		
		if (!result.Succeeded)
		{
			return Unauthorized(new FailResponse { Errors = result.Errors });
		}
		
		var tokens = await _authService.GenerateTokensAsync(result.Claims, ct);
		
		return Ok(new TokenResponse
		{
			AccessToken = tokens.AccessToken,
			RefreshToken = tokens.RefreshToken
		});
	} 
	
	/// <summary>
	/// Refreshes tokens
	/// </summary>
	/// <response code="200">Tokens are successfully refreshed</response>
	/// <response code="400">Tokens are missing</response>
	/// <response code="401">Tokens are invalid</response>
	[HttpPost("refresh")]
	[ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.OK)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.BadRequest)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.Unauthorized)]
	public async Task<IActionResult> RefreshToken(TokenRefreshingRequest tokensRequest, CancellationToken ct)
	{
		var result = await _authService.ValidateTokensAsync(tokensRequest, ct);
		
		if (!result.Succeeded)
		{
			return Unauthorized(new FailResponse { Errors = result.Errors });
		}
		
		var tokens = await _authService.GenerateTokensAsync(result.Claims, ct);
		
		return Ok(new TokenResponse
		{
			AccessToken = tokens.AccessToken,
			RefreshToken = tokens.RefreshToken
		});
	}
}