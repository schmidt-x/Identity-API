using IdentityApi.Contracts.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityApi.Extensions;

public static class WebApplicationBuilderExtensions 
{
	public static WebApplicationBuilder SetConnectionStringsOptions(this WebApplicationBuilder builder)
	{
		builder.Services
			.AddOptions<ConnectionStringsOptions>()
			.Bind(builder.Configuration.GetRequiredSection(ConnectionStringsOptions.ConnectionStrings))
			.Validate(o => !string.IsNullOrEmpty(o.Mssql), "Connection string is required")
			.ValidateOnStart();
		
		return builder;
	}
	
	public static WebApplicationBuilder SetEmailOptions(this WebApplicationBuilder builder)
	{
		builder.Services
			.AddOptions<EmailOptions>()
			.Bind(builder.Configuration.GetRequiredSection(EmailOptions.Email))
			// .ValidateDataAnnotations()
			.Validate(o => !string.IsNullOrEmpty(o.Address), "Email address is required")
			.Validate(o => !string.IsNullOrEmpty(o.Password), "Email password is required")
			.ValidateOnStart();
		
		return builder;
	}
}