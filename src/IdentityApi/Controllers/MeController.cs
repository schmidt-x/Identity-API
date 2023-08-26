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
	private readonly IMeService _meService;
	private readonly IEmailSender _emailSender;
	private readonly IAuthService _authService;
	
	public MeController(IMeService meService, IEmailSender emailSender, IAuthService authService)
	{
		_meService = meService;
		_emailSender = emailSender;
		_authService = authService;
	}
	
	
	/// <summary>
	/// Returns Me
	/// </summary>
	/// <response code="200">Me is returned</response>
	[ProducesResponseType(typeof(Me), (int)HttpStatusCode.OK)]
	[HttpGet]
	public async Task<IActionResult> GetMe(CancellationToken ct)
	{
		var me = await _meService.GetAsync(ct);
		
		return Ok(me);
	}
	
	/// <summary>
	/// Updates username
	/// </summary>
	/// <response code="200">Username is successfully updated</response>
	/// <response code="400">Username is already taken or validation failed</response>
	[ProducesResponseType(typeof(Me), (int)HttpStatusCode.OK)]
	[ProducesResponseType(typeof(FailResponse), (int)HttpStatusCode.BadRequest)]
	[HttpPatch("username-update")]
	public async Task<IActionResult> UpdateUsername(UsernameUpdateRequest usernameRequest, CancellationToken ct)
	{
		var result = await _meService.UpdateUsernameAsync(usernameRequest.Username, usernameRequest.Password, ct);
		
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
	public IActionResult StartEmailUpdatingProcess()
	{
		var verificationCode = _meService.CreateEmailUpdateSession();
		
		var oldEmail = HttpContext.User.FindEmail()!;
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
		// Note: I'm not sure is it's appropriate to use another service's method here,
		// but the logic is completely the same 
		var result = _authService.VerifyEmail(HttpContext.User.FindId()!, codeRequest.Code);
		
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
		var result = await _meService.CacheNewEmailAsync(emailRequest.Email, ct);
		
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
		var result = await _meService.UpdateEmailAsync(code.Code, ct);
		
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
		var result = await _meService.UpdatePasswordAsync(passwordRequest.Password, passwordRequest.NewPassword, ct);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		return Ok(result.Value);
	}
}