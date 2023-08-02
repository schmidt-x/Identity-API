using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Responses;
using IdentityApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IdentityApi.Controllers;

[Authorize("user")]
[Produces("application/json")]
[ApiController, Route("api/[controller]")]
[ProducesResponseType(typeof(FailResponse), 401)]
public class MeController : ControllerBase
{
	private readonly IMeService _meService;
	private readonly IEmailService _emailService;
	private readonly IAuthService _authService;

	public MeController(IMeService meService, IEmailService emailService, IAuthService authService)
	{
		_meService = meService;
		_emailService = emailService;
		_authService = authService;
	}
	
	/// <summary>
	/// Gets Me
	/// </summary>
	/// <response code="200">Me is returned</response>
	[ProducesResponseType(typeof(UserProfile), 200)]
	[HttpGet("profile")]
	public async Task<IActionResult> GetProfile(CancellationToken ct)
	{
		var me = await _meService.GetAsync(ct);
		
		return Ok(me);
	}
	
	/// <summary>
	/// Updates username
	/// </summary>
	/// <response code="200">Username is updated</response>
	/// <response code="400">Username is already taken or validation failed</response>
	[ProducesResponseType(typeof(UserProfile), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	[HttpPut("username-update")]
	public async Task<IActionResult> UpdateUsername(UsernameUpdate update, CancellationToken ct)
	{
		var result = await _meService.UpdateUsernameAsync(update, ct);
		
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
	public IActionResult CreateEmailUpdateSession()
	{
		var verificationCode = _meService.CreateEmailUpdateSession();
		
		var userEmail = HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Email)!;
		
		var _ = _emailService.SendAsync(userEmail, verificationCode);
		
		return Ok(new MessageResponse { Message = "Verification code is sent to your old email" });
	}
	
	/// <summary>
	/// Verifies old email address
	/// </summary>
	/// <response code="200">Old email address is verified</response>
	/// <response code="400">Verification failed</response>
	[HttpPost("email-update/verify-old-email")]
	[ProducesResponseType(typeof(MessageResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	public IActionResult VerifyOldEmail(CodeVerification code)
	{
		var verificationResult = _meService.VerifyOldEmail(code.Code);
		
		if (!verificationResult.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = verificationResult.Errors });
		}
		
		return Ok(new MessageResponse { Message = "Old email has successfully been verified"});
	}
	
	/// <summary>
	/// Registers new email address and send verification code
	/// </summary>
	/// <response code="200">Email is registered and verification code is sent</response> 
	/// <response code="400">Validation failed</response> 
	[HttpPost("email-update/register-new-email")]
	[ProducesResponseType(typeof(MessageResponse), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	public async Task<IActionResult> RegisterNewEmail(EmailAddress email, CancellationToken ct)
	{
		var validationResult = await _meService.CacheNewEmailAsync(email.Email, ct);
		
		if (!validationResult.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = validationResult.Errors });
		}
		
		var userEmail = HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Email)!;
		
		var _ = _emailService.SendAsync(userEmail, validationResult.Value);
		
		return Ok(new MessageResponse { Message = "Verification code is sent to your new email" });
	}
	
	/// <summary>
	/// Updates email address
	/// </summary>
	/// <response code="200">Email address is updated</response> 
	/// <response code="400">Verification failed</response>
	[HttpPut("email-update/verify-new-email")]
	[ProducesResponseType(typeof(UserProfile), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	public async Task<IActionResult> UpdateEmail(CodeVerification code, CancellationToken ct)
	{
		var result = await _meService.UpdateEmailAsync(code.Code, ct);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		var me = result.Value;
		me.Token = _authService.UpdateAccessTokenEmail(me.Token, me.Email);
		
		return Ok(me);
	}
	
}