using IdentityApi.Data.DataAccess;
using IdentityApi.Data.Repositories;
using IdentityApi.Filters;
using IdentityApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityApi.Extensions;

public static class ServiceCollectionExtentions
{
	public static IServiceCollection AddAuthorizationWithPolicies(this IServiceCollection services)
	{
		services.AddAuthorization(o =>
		{
			o.FallbackPolicy = new AuthorizationPolicyBuilder()
				.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
				.RequireAuthenticatedUser()
				.Build();
				
			o.AddPolicy("user", b =>
			{
				b.RequireClaim("role", "user");
			});
		});
		
		return services;
	}
	
	public static IServiceCollection AddDataAccess(this IServiceCollection services)
	{
		Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
		
		return services
			.AddScoped<ISqlDataAccess, SqlDataAccess>();
	}
	
	public static IServiceCollection AddRepositories(this IServiceCollection services)
	{
		return services
			.AddScoped<IUserRepository, UserRepository>()
			.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
	}
	
	public static IServiceCollection AddServices(this IServiceCollection services)
	{
		return services
			.AddScoped<IAuthService, AuthService>()
			.AddScoped<IEmailService, EmailService>()
			.AddScoped<IPasswordService, PasswordService>()
			.AddScoped<IUserContext, UserContext>()
			.AddScoped<IMeService, MeService>();
	}
	
	public static IServiceCollection AddFilters(this IServiceCollection services)
	{
		return services
			.AddScoped<SessionCookieActionFilter>()
			.AddScoped<ModerlStateErrorsHandlerActionFilter>();
	}
}