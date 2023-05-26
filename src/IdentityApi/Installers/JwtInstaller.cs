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
		
		builder.Services
			.AddAuthentication(o =>
			{
				o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(b =>
			{
				b.TokenValidationParameters = validationParameters;
				b.MapInboundClaims = false;
				b.Events = new()
				{
					// Since the 'role' claim is not stored inside the Jwt token,
					// we need to retrieve it from the db and add it to the request claims.
					// This additional security measure ensures that the 'role' claim is retrieved dynamically
					
					OnTokenValidated = async tvc =>
					{
						var ctx = tvc.HttpContext;
						var user = tvc.Principal!;
						
						var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
						var userRepo = ctx.RequestServices.GetRequiredService<IUserRepository>();
						
						var userRole = await userRepo.GetRoleAsync(userId);
						
						var claims = user.Claims.Append(new Claim("role", userRole));
						var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
						tvc.Principal = new ClaimsPrincipal(identity);
					}
				};
			});
		
		return builder;
	}
}