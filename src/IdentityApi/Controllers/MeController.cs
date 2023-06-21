using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers;

[Authorize("user")]
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
	[Produces("application/json")]
	[HttpGet("profile")]
	public async Task<IActionResult> GetProfile(CancellationToken ct)
	{
		var me = await _meService.GetAsync(ct);
		
		return Ok(me);
	}
	
}