using System.Security;

namespace IdentityApi.Filters;

public class SessionCookieActionFilter : IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var ctx = context.HttpContext;
		
		if (!ctx.Request.Cookies.TryGetValue("session_id", out var rawSessionId))
		{
			var request = ctx.Request;
			ctx.Response.Headers.Location = $"{request.Scheme}://{request.Host}/api/auth/session";
			
			context.Result = new BadRequestObjectResult(new FailResponse { Errors = new()
			{
				{ "SessionID", new[] { "Session ID is required" } }
			}});
			
			return;
		} 
		
		if (!Guid.TryParse(rawSessionId, out var _))
		{
			throw new SecurityException("On validating session ID");
		}
		
		ctx.Items.Add("sessionId", rawSessionId);
		await next();
	}
}