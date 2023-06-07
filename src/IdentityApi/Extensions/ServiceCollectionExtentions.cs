using FluentValidation;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Data.DataAccess;
using IdentityApi.Data.Repositories;
using IdentityApi.Filters;
using IdentityApi.Services;
using IdentityApi.Validation.DTOValidation;
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
	
	public static IServiceCollection AddValidators(this IServiceCollection services)
	{
		return services
			.AddScoped<IValidator<UserRegistration>, UserRegisterValidator>()
			.AddScoped<IValidator<UserLogin>, UserLoginValidator>()
			.AddScoped<IValidator<EmailRegistration>, EmailRequestValidator>()
			.AddScoped<IValidator<CodeVerification>, CodeVerificaitonValidator>()
			.AddScoped<IValidator<TokenRefreshing>, TokenRefreshingValidator>()
			.AddScoped<IValidator<UsernameUpdate>, UsernameUpdateValidator>();
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
			.AddScoped<IPasswordService, PasswordService>();
	}
	
	public static IServiceCollection AddFilters(this IServiceCollection services)
	{
		return services
			.AddScoped<SessionCookieActionFilter>()
			.AddScoped<ModerlStateErrorsHandlerActionFilter>();
	}
}