using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IdentityApi.Services;

public class UserContext : IUserContext
{
	private readonly HttpContext _ctx;
	 
	public UserContext(IHttpContextAccessor httpContextAccessor)
	{
		_ctx = httpContextAccessor.HttpContext!;
	}
	
	
	public Guid GetId()
	{
		var rawId = _ctx.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
		
		if (rawId == null)
		{
			throw new SecurityTokenException("User claim 'sub' is not present");
		}
		
		if (!Guid.TryParse(rawId, out var userId))
		{
			throw new SecurityTokenException("User claim 'sub' is not valid");
		}
		
		return userId;
	}
}