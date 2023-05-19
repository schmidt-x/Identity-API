using IdentityApi.Middleware;
using Microsoft.AspNetCore.Builder;

namespace IdentityApi.Extensions;

public static class ApplicationBuilderExtensions
{
	public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder app)
	{
		return app.UseMiddleware<ExceptionHandlerMiddleware>();
	}
}