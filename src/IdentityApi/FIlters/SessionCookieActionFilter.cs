using System;
using System.Security;
using System.Threading.Tasks;
using IdentityApi.Contracts.Responses;
using IdentityApi.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityApi.Filters;

public class SessionCookieActionFilter : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var ctx = context.HttpContext;
		
		if (!ctx.Request.Cookies.TryGetValue(Key.CookieSessionId, out var sessionId))
		{
			context.Result = new BadRequestObjectResult(new FailResponse { Errors = new()
			{
				{ ErrorKey.Session, new[] { ErrorMessage.SessionNotFound } }
			}});
			
			return;
		} 
		
		if (!Guid.TryParse(sessionId, out var _))
		{
			throw new SecurityException($"Session ID (Guid) is not valid: {sessionId}", typeof(Guid));
		}
		
		ctx.Items.Add(Key.SessionId, sessionId);
		await next();
	}
}