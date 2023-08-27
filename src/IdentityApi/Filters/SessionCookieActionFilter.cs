using System;
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
				{ ErrorKey.Session, new[] { ErrorMessage.SessionIdNotFound } }
			}});
			
			return;
		}
		
		if (!Guid.TryParse(sessionId, out _))
		{
			context.Result = new BadRequestObjectResult(new FailResponse { Errors = new()
			{
				{ ErrorKey.Session, new[] { ErrorMessage.InvalidSessionId } }
			}});
			
			return;
		}
		
		ctx.Items.Add(Key.SessionId, sessionId);
		await next();
	}
}