namespace IdentityLibrary.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddIdentityLibrary(this IServiceCollection services)
	{
		services
			.AddSingleton<IUserRepository, UserRepository>()
			.AddSingleton<IRefreshTokenRepository, RefreshTokenRepository>()
			.AddSingleton<IAuthService, AuthService>();
		
		return services;
	}
}