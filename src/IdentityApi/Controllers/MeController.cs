using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Extensions;
using IdentityApi.Responses;
using IdentityApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityApi.Contracts.Requests;

namespace IdentityApi.Controllers;

[Authorize("user")]
[Produces("application/json")]
[ApiController, Route("api/[controller]")]
[ProducesResponseType(typeof(FailResponse), 401)]
public class MeController : ControllerBase
{
	private readonly IMeService _meService;
	private readonly IEmailService _emailService;

	public MeController(IMeService meService, IEmailService emailService)
	{
		_meService = meService;
		_emailService = emailService;
	}
	
	/// <summary>
	/// Returns Me
	/// </summary>
	/// <response code="200">Me is returned</response>
	[ProducesResponseType(typeof(Me), 200)]
	[HttpGet]
	public async Task<IActionResult> GetMe(CancellationToken ct)
	{
		var me = await _meService.GetAsync(ct);
		
		return Ok(me);
	}
	
	/// <summary>
	/// Updates username
	/// </summary>
	/// <response code="200">Username is updated</response>
	/// <response code="400">Username is already taken or validation failed</response>
	[ProducesResponseType(typeof(Me), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
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
	/// Sends verification code to the old email address
	/// </summary>
	/// <response code="200">Verification code is sent</response>
	[HttpPost("email-update")]
	[ProducesResponseType(typeof(MessageResponse), 200)]
	public IActionResult StartEmailUpdatingProcess()
	{
		var verificationCode = _meService.CreateEmailUpdateSession();
		
		var _ = _emailService.SendAsync(HttpContext.User.FindEmail()!, verificationCode);
		
		return Ok(new MessageResponse { Message = "Verification code is sent to your old email" });
	}
	
	/// <summary>
	/// Verifies old email address
	/// </summary>
	/// <response code="200">Old email address is verified</response>
	/// <response code="400">Verification failed</response>
	[HttpPatch("email-update/verify-old-email")]
	[ProducesResponseType(typeof(MessageResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	public IActionResult VerifyOldEmail(CodeVerificationRequest codeRequest)
	{
		var result = _meService.VerifyOldEmail(codeRequest.Code);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		return Ok(new MessageResponse { Message = "Old email has successfully been verified"});
	}
	
	/// <summary>
	/// Registers new email address and send verification code
	/// </summary>
	/// <response code="200">Email is registered and verification code is sent</response> 
	/// <response code="400">Validation failed</response> 
	[HttpPatch("email-update/register-new-email")]
	[ProducesResponseType(typeof(MessageResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	public async Task<IActionResult> RegisterNewEmail(EmailUpdateRequest emailRequest, CancellationToken ct)
	{
		var result = await _meService.CacheNewEmailAsync(emailRequest.Email, ct);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		var _ = _emailService.SendAsync(emailRequest.Email, result.Value);
		
		return Ok(new MessageResponse { Message = "Verification code is sent to your new email" });
	}
	
	/// <summary>
	/// Updates email address
	/// </summary>
	/// <response code="200">Email address is updated</response> 
	/// <response code="400">Verification failed</response>
	[HttpPatch("email-update/verify-new-email")]
	[ProducesResponseType(typeof(Me), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
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
	/// <response code="200">Password is updated</response> 
	/// <response code="400">Verification failed</response>
	[HttpPatch("password-change")]
	[ProducesResponseType(typeof(Me), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
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