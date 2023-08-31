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
		
		if (rawId is null)
		{
			throw new SecurityTokenException("User claim 'sub' is not present");
		}
		
		if (!Guid.TryParse(rawId, out var userId))
		{
			throw new SecurityTokenException($"User claim 'sub' is not valid Guid. (sub: {rawId})");
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
		
		if (rawJti is null)
		{
			throw new SecurityTokenException("User claim 'jti' is not present");
		}
		
		if (!Guid.TryParse(rawJti, out var jti))
		{
			throw new SecurityTokenException($"User claim 'jti' is not valid Guid. (jti: {rawJti})");
		}
		
		return jti;
	}
	
	public long GetExp()
	{
		ThrowIfNotAuthenticated();
		
		var rawExp = _ctx.User.FindFirstValue(JwtRegisteredClaimNames.Exp);
		
		if (rawExp is null)
		{
			throw new SecurityTokenException("User claim 'exp' is not present");
		}
		
		if (!long.TryParse(rawExp, out var exp))
		{
			throw new SecurityTokenException($"User claim 'exp' is not valid Long. (exp: {rawExp})");
		}
		
		return exp;
	}
	
	
	private void ThrowIfNotAuthenticated()
	{
		if (!IsAuthenticated(_ctx.User.Identity))
			throw new Exception("User is not authenticated");
	}
	
	private static bool IsAuthenticated(IIdentity? identity) => identity is { IsAuthenticated: true };
}