namespace IdentityLibrary.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddIdentityLibrary(this IServiceCollection services)
	{
		return services
			.AddScoped<IUserRepository, UserRepository>()
			.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>()
			.AddScoped<IAuthService, AuthService>();
	}
}