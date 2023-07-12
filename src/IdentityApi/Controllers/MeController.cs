using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Responses;
using IdentityApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers;

[Authorize("user")]
[Produces("application/json")]
[ApiController, Route("api/[controller]")]
public class MeController : ControllerBase
{
	private readonly IMeService _meService;
	
	public MeController(IMeService meService)
	{
		_meService = meService;
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
	[HttpPut("update-username")]
	public async Task<IActionResult> UpdateUsername(UsernameUpdate user, CancellationToken ct)
	{
		var result = await _meService.UpdateUsernameAsync(user, ct);
		
		if (!result.Succeeded)
		{
			return BadRequest(new FailResponse { Errors = result.Errors });
		}
		
		return Ok(result.Value);
	}
}