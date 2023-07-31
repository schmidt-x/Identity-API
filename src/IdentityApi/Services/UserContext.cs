using System;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

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
		if (!IsAuthenticated(_ctx.User.Identity))
		{
			throw new Exception("User is not authenticated");
		}
		
		var rawId = _ctx.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
		
		if (rawId == null)
		{
			throw new SecurityException("User claim 'sub' is not present");
		}
		
		if (!Guid.TryParse(rawId, out var userId))
		{
			throw new SecurityException("User claim 'sub' is not valid");
		}
		
		return userId;
	}
	
	public string GetEmail()
	{
		if (!IsAuthenticated(_ctx.User.Identity))
		{
			throw new Exception("User is not authenticated");
		}
		
		var email = _ctx.User.FindFirstValue(JwtRegisteredClaimNames.Email);
		
		if (email is null)
		{
			throw new SecurityException("User claim 'email' is not present");
		}
		
		return email;
	}
	
	public string GetToken()
	{
		var identity = _ctx.User.Identity;
		
		if (!IsAuthenticated(identity))
		{
			throw new Exception("User is not authenticated");
		}
		
		var authScheme = identity!.AuthenticationType;
		
		var token = authScheme switch
		{
			"Bearer" => _ctx.Request.Headers.Authorization.ToString()["Bearer ".Length..],
			_ => throw new NotImplementedException(),
		};
		
		return token;
	}
	
	private static bool IsAuthenticated(IIdentity? identity) => identity is { IsAuthenticated: true } ;
}