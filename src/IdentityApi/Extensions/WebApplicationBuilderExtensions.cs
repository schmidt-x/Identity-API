using IdentityApi.Contracts.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityApi.Extensions;

public static class WebApplicationBuilderExtensions 
{
	public static WebApplicationBuilder AddOptions(this WebApplicationBuilder builder)
	{
		builder.Services.Configure<ConnectionStringsOptions>(
			builder.Configuration.GetSection(ConnectionStringsOptions.ConnectionStrings));
		
		builder.Services.Configure<JwtOptions>(
			builder.Configuration.GetSection(JwtOptions.Jwt));
		
		builder.Services.Configure<EmailOptions>(
			builder.Configuration.GetSection(EmailOptions.Email));
		
		return builder;
	}
}