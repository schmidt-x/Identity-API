using System;
using System.Security;
using System.Threading.Tasks;
using IdentityApi.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityApi.Filters;

public class SessionCookieActionFilter : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var ctx = context.HttpContext;
		
		if (!ctx.Request.Cookies.TryGetValue("session_id", out var sessionId))
		{
			context.Result = new BadRequestObjectResult(new FailResponse { Errors = new()
			{
				{ "sessionID", new[] { "Session ID is required" } }
			}});
			
			return;
		} 
		
		if (!Guid.TryParse(sessionId, out var _))
		{
			throw new SecurityException("Session ID (Guid) is not valid");
		}
		
		ctx.Items.Add("sessionID", sessionId);
		await next();
	}
}