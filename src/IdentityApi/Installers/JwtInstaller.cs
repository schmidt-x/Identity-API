using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityApi.Options;
using IdentityApi.Data.Repositories;
using IdentityApi.Contracts.Responses;
using IdentityApi.Domain.Constants;
using IdentityApi.Extensions;
using IdentityApi.Services;
using IdentityApi.Validation.OptionsValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
			ValidateLifetime = true,
			ClockSkew = jwt.ClockSkew
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
					OnTokenValidated = ValidatedTokenHandler,
					
					OnChallenge = ChallengeHandler
				};
			});
		
		return builder;
		
		
		async Task ValidatedTokenHandler(TokenValidatedContext tvc)
		{
			var services = tvc.HttpContext.RequestServices;
			var principal = tvc.Principal!;
			
			// check if the token is black-listed
			
			var tokenBlacklist = services.GetRequiredService<ITokenBlacklist>();
			
			if (tokenBlacklist.Contains(principal.FindJti()!))
			{
				tvc.Fail("Token is blocked");
				return;
			}
			
			// Since the 'role' claim is not stored inside the Jwt token,
			// we need to retrieve it from the database and add it to the request claims.
			// This additional security measure ensures that the 'role' claim is retrieved dynamically
			
			var userId = Guid.Parse(principal.FindId()!);
			var userRepo = services.GetRequiredService<IUserRepository>();
			var userRole = await userRepo.GetRoleAsync(userId, default);
						
			var claims = principal.Claims.Append(new Claim(Key.Role, userRole));
			var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
						
			tvc.Principal = new ClaimsPrincipal(identity);
		}
		
		async Task ChallengeHandler(JwtBearerChallengeContext ctx)
		{
			var response = ctx.Response;
			response.StatusCode = (int)HttpStatusCode.Unauthorized;
			
			await response.WriteAsJsonAsync(new FailResponse
			{
				Errors = new() { { ErrorKey.Auth, new[] { ErrorMessage.Unauthorized } } }
			});
					
			ctx.HandleResponse();
		}
	}
	
	public static WebApplicationBuilder AddAuthorizationWithPolicies(this WebApplicationBuilder builder)
	{
		builder.Services.AddAuthorization(o =>
		{
			o.FallbackPolicy = new AuthorizationPolicyBuilder()
				.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
				.RequireAuthenticatedUser()
				.Build();
				
			o.AddPolicy(Policy.UserPolicy, b =>
			{
				b.RequireClaim(Key.Role, Role.User);
			});
		});
		
		return builder;
	}
}