using IdentityLibrary.Repositories;
using IdentityLibrary.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityLibrary.Extensions;

public static class IdentityApiExtensions
{
	public static IServiceCollection AddIdentityLibrary(this IServiceCollection services)
	{
		services.AddSingleton<IUserRepository, UserRepository>();
		services.AddSingleton<IRefreshTokenRepository, RefreshTokenRepository>();
		services.AddSingleton<IAuthService, AuthService>();
		
		return services;
	}
}