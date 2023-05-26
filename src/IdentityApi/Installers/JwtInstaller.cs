using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using IdentityApi.Contracts.Options;
using IdentityApi.Data.Repositories;
using IdentityApi.Validation.OptionsValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IdentityApi.Installers;

public static class JwtInstaller
{
	public static WebApplicationBuilder AddJwtAuthentication(this WebApplicationBuilder builder)
	{
		var jwtSection = builder.Configuration.GetRequiredSection(JwtOptions.Jwt);
		
		builder.Services
			.AddOptions<JwtOptions>()
			.Bind(jwtSection)
			.ValidateOnStart();
		
		builder.Services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

		var jwt = jwtSection.Get<JwtOptions>()!;
		
		var validationParameters = new TokenValidationParameters
		{
			ValidIssuer = jwt.Issuer,
			ValidAudience = jwt.Audience,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateIssuerSigningKey = true,
			ValidateLifetime = true
		};
		
		builder.Services.AddSingleton(validationParameters);
		
		
		return builder;
	}
}