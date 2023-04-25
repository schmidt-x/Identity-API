﻿namespace IdentityApi.Extensions;

public static class ServiceCollectionExtentions
{
	public static IServiceCollection AddRequestValidation(this IServiceCollection services)
	{
		return services
			.AddScoped<IValidator<UserRegistration>, UserRegisterValidator>()
			.AddScoped<IValidator<UserLogin>, UserLoginValidator>()
			.AddScoped<IValidator<EmailRegistration>, EmailRequestValidator>()
			.AddScoped<IValidator<CodeVerification>, CodeVerificaitonValidator>();
	}
	
	public static IServiceCollection SetIdentityConfiguration(this IServiceCollection services, IConfiguration config)
	{
		services.AddSingleton(new DbConfig
		{
			ConnectionString = config.GetConnectionString("SqlServer")!
		});
		
		services.AddSingleton(new JwtConfig
		{
			SecretKey = config["Jwt:SecretKey"]!,
			Audience = config["Jwt:Audience"]!,
			Issuer = config["Jwt:Issuer"]!,
		});
		
		services.AddSingleton(new EmailConfig
		{
			Address = config["Email:Address"]!,
			Password = config["Email:Password"]!
		});
		
		return services;
	}
	
	public static IServiceCollection AddFilters(this IServiceCollection services)
	{
		return services
			.AddScoped<SessionCookieActionFilter>()
			.AddScoped<ValidationActionFilter>();
	}
}