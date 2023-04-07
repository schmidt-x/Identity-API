using IdentityApi.Validators;

namespace IdentityApi.Extensions;

public static class ServiceCollectionExtentions
{
	public static IServiceCollection AddUserValidation(this IServiceCollection services)
	{
		services
			.AddScoped<IValidator<UserRegister>, UserRegisterValidator>()
			.AddScoped<IValidator<UserLogin>, UserLoginValidator>();
		
		return services;
	}
}