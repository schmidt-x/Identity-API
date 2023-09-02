using IdentityApi.Data.DataAccess;
using IdentityApi.Data.Repositories;
using IdentityApi.Factories;
using IdentityApi.Filters;
using IdentityApi.Services;
using System.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityApi.Extensions;

public static class ServiceCollectionExtentions
{
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
			.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()
			.AddScoped<IUnitOfWork, UnitOfWork>();
	}
	
	public static IServiceCollection AddTransactionFactory(this IServiceCollection services)
	{
		services.AddScoped<TransactionFactory>(sp => 
			new TransactionFactory(sp.GetRequiredService<SqlConnection>()));
		
		return services;
	}
	
	public static IServiceCollection AddServices(this IServiceCollection services)
	{
		return services
			.AddScoped<IAuthService, AuthService>()
			.AddScoped<IEmailSender, EmailSender>()
			.AddScoped<IPasswordHasher, PasswordHasher>()
			.AddScoped<IUserContext, UserContext>()
			.AddScoped<ICodeGenerator, CodeGenerator>()
			.AddScoped<IUserService, UserService>()
			.AddScoped<IJwtService, JwtService>()
			
			.AddSingleton<ISessionService, SessionService>()
			.AddSingleton<ITokenBlacklist, TokenBlacklist>();
	}
	
	public static IServiceCollection AddFilters(this IServiceCollection services)
	{
		return services
			.AddScoped<SessionCookieActionFilter>()
			.AddScoped<ModerlStateErrorsHandlerActionFilter>();
	}
}