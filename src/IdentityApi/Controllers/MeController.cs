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

	public MeController(IMeService meService, IEmailService emailService)
	{
		_meService = meService;
		_emailService = emailService;
	}
	
	/// <summary>
	/// Gets profile
	/// </summary>
	/// <response code="200">User profile is returned</response>
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
	/// Sends verification code to old email address
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
	/// Registers new email address
	/// </summary>
	/// <response code="200">Email is registered</response> 
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
	/// Verifies and updates new email address
	/// </summary>
	/// <response code="200">Email is updated and profile is returned</response> 
	/// <response code="400">Verification failed</response>
	[HttpPut("email-update/verify-new-email")]
	[ProducesResponseType(typeof(UserProfile), 200)]
	[ProducesResponseType(typeof(FailResponse), 400)]
	public async Task<IActionResult> VerifyAndUpdateNewEmail(CodeVerification code, CancellationToken ct)
	{
		var verificatoinResult = await _meService.VerifyAndUpdateNewEmailAsync(code.Code, ct);
		
		if (!verificatoinResult.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = verificatoinResult.Errors });
		}
		
		return Ok(verificatoinResult.Value);
	}
	
}