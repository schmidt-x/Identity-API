using System;
using System.Security.Claims;
using System.Security.Principal;
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
		ThrowIfNotAuthenticated();
		
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
	
	public string GetEmail()
	{
		ThrowIfNotAuthenticated();
		
		var email = _ctx.User.FindFirstValue(JwtRegisteredClaimNames.Email);
		
		if (email is null)
		{
			throw new SecurityTokenException("User claim 'email' is not present");
		}
		
		return email;
	}
	
	public string GetToken()
	{
		ThrowIfNotAuthenticated();
		
		var authScheme = _ctx.User.Identity!.AuthenticationType;
		
		var token = authScheme switch
		{
			"Bearer" => _ctx.Request.Headers.Authorization.ToString()["Bearer ".Length..],
			_ => throw new NotImplementedException(),
		};
		
		return token;
	}
	
	public Guid GetJti()
	{
		ThrowIfNotAuthenticated();
		
		var rawJti = _ctx.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
		
		if (string.IsNullOrEmpty(rawJti))
		{
			throw new SecurityTokenException("User claim 'jti' is not present");
		}
		
		if (!Guid.TryParse(rawJti, out var jti))
		{
			throw new SecurityTokenException("User claim 'jti' is not valid");
		}
		
		return jti;
	}
	
	private void ThrowIfNotAuthenticated()
	{
		if (!IsAuthenticated(_ctx.User.Identity))
			throw new Exception("User is not authenticated");
	}
	
	private static bool IsAuthenticated(IIdentity? identity) => identity is { IsAuthenticated: true };
}