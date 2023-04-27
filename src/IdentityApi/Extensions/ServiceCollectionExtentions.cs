using System.Text;

namespace IdentityApi.Extensions;

public static class ServiceCollectionExtentions
{
	public static IServiceCollection AddRequestValidation(this IServiceCollection services)
	{
		return services
			.AddScoped<IValidator<UserRegistration>, UserRegisterValidator>()
			.AddScoped<IValidator<UserLogin>, UserLoginValidator>()
			.AddScoped<IValidator<EmailRegistration>, EmailRequestValidator>()
			.AddScoped<IValidator<CodeVerification>, CodeVerificaitonValidator>()
			.AddScoped<IValidator<RefreshTokenRequest>, RefreshTokenRequestValidation>();
	}
	
	public static IServiceCollection SetIdentityConfiguration(this IServiceCollection services, IConfiguration config)
	{
		services.AddSingleton(new DbConfig
		{
			ConnectionString = config.GetConnectionString("SqlServer")!
		});
		
		var secretKey = config["Jwt:SecretKey"]!;
		var audience = config["Jwt:Audience"]!;
		var issuer = config["Jwt:Issuer"]!;
		
		services.AddSingleton(new JwtConfig
		{
			SecretKey = secretKey,
			Audience = audience,
			Issuer = issuer,
			Parameters = new()
			{
				ValidIssuer = issuer,
				ValidAudience = audience,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateIssuerSigningKey = true,
				ValidateLifetime = false
			}
		});
		
		services.AddSingleton(new EmailConfig
		{
			Address = config["Email:Address"]!,
			Password = config["Email:Password"]!,
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