using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Extensions;
using IdentityApi.Contracts.Responses;
using IdentityApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityApi.Contracts.Requests;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Controllers;

[Authorize(Policy.UserPolicy)]
[Produces("application/json")]
[ApiController, Route("api/[controller]")]
[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.Unauthorized)]
public class MeController : ControllerBase
{
	private readonly IUserService _userService;
	private readonly IEmailSender _emailSender;
	private readonly ISessionService _sessionService;
	
	
	public MeController(
		IUserService userService,
		IEmailSender emailSender,
		ISessionService sessionService)
	{
		_userService = userService;
		_emailSender = emailSender;
		_sessionService = sessionService;
	}
	
	
	/// <summary>
	/// Returns Me
	/// </summary>
	/// <response code="200">Me is returned</response>
	[HttpGet]
	[ProducesResponseType(typeof(Me), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> GetMe(CancellationToken ct)
	{
		var me = await _userService.GetAsync(ct);
		
		return Ok(me);
	}
	
	/// <summary>
	/// Updates username
	/// </summary>
	/// <response code="200">Username is successfully updated</response>
	/// <response code="400">Username is already taken or validation failed</response>
	[HttpPatch("username-update")]
	[ProducesResponseType(typeof(Me), (int)HttpStatusCode.OK)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.BadRequest)]
	public async Task<IActionResult> UpdateUsername(UsernameUpdateRequest usernameRequest, CancellationToken ct)
	{
		var result = await _userService.UpdateUsernameAsync(usernameRequest.Username, usernameRequest.Password, ct);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		return Ok(result.Value);
	}
	
	/// <summary>
	/// Sends verification code to old email address
	/// </summary>
	/// <response code="200">Verification code is sent</response>
	[HttpPost("email-update")]
	[ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.OK)]
	public IActionResult EmailUpdateSession()
	{
		var verificationCode = _userService.CreateEmailUpdateSession();
		
		var oldEmail = User.FindEmail()!;
		var _ = _emailSender.SendAsync(oldEmail, verificationCode);
		
		return Ok(new MessageResponse { Message = $"Verification code is sent to '{oldEmail}' email address" });
	}
	
	/// <summary>
	/// Verifies old email address
	/// </summary>
	/// <response code="200">Old email address is successfully verified</response>
	/// <response code="400">Verification failed</response>
	[HttpPatch("email-update/verify-old-email")]
	[ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.OK)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.BadRequest)]
	public IActionResult VerifyEmail(CodeVerificationRequest codeRequest)
	{
		var result = _sessionService.VerifySession(User.FindId()!, codeRequest.Code);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		return Ok(new MessageResponse { Message = "Old email address is successfully verified" });
	}
	
	/// <summary>
	/// Sends verification code to new email address
	/// </summary>
	/// <response code="200">Verification code is sent</response> 
	/// <response code="400">Validation failed</response> 
	[HttpPatch("email-update/register-new-email")]
	[ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.OK)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.BadRequest)]
	public async Task<IActionResult> RegisterNewEmail(EmailRequest emailRequest, CancellationToken ct)
	{
		var result = await _userService.CacheNewEmailAsync(emailRequest.Email, ct);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		var _ = _emailSender.SendAsync(emailRequest.Email, result.Value);
		
		return Ok(new MessageResponse { Message = $"Verification code sent to '{emailRequest.Email}' email address" });
	}
	
	/// <summary>
	/// Updates email address
	/// </summary>
	/// <response code="200">Email address is successfully updated</response> 
	/// <response code="400">Verification failed</response>
	[HttpPatch("email-update/verify-new-email")]
	[ProducesResponseType(typeof(Me), (int)HttpStatusCode.OK)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.BadRequest)]
	public async Task<IActionResult> UpdateEmail(CodeVerificationRequest code, CancellationToken ct)
	{
		var result = await _userService.UpdateEmailAsync(code.Code, ct);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		return Ok(result.Value);
	}
	
	/// <summary>
	/// Updates password
	/// </summary>
	/// <response code="200">Password is successfully updated</response> 
	/// <response code="400">Validation failed</response>
	[HttpPatch("password-update")]
	[ProducesResponseType(typeof(Me), (int)HttpStatusCode.OK)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.BadRequest)]
	public async Task<IActionResult> UpdatePassword(PasswordUpdateRequest passwordRequest, CancellationToken ct)
	{
		var result = await _userService.UpdatePasswordAsync(passwordRequest.Password, passwordRequest.NewPassword, ct);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		return Ok(result.Value);
	}
	
	/// <summary>
	/// Logs out a user
	/// </summary>
	/// <response code="204">User is logged out</response>
	[HttpPost("log-out")]
	[ProducesResponseType((int)HttpStatusCode.NoContent)]
	public async Task<IActionResult> LogOut(CancellationToken ct)
	{
		await _userService.LogOutAsync(ct);
		
		return NoContent();
	}
}