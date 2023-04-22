using IdentityApi.Middleware;

namespace IdentityApi.Extensions;

public static class ApplicationBuilderExtensions
{
	public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder app)
	{
		return app.UseMiddleware<ExceptionHandlerMiddleware>();
	}
}