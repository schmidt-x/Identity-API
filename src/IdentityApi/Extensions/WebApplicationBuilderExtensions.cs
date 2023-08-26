using IdentityApi.Options;
using Microsoft.AspNetCore.Builder;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

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
	
	public static WebApplicationBuilder AddMssql(this WebApplicationBuilder builder)
	{
		builder.Services.AddScoped<SqlConnection>(sp =>
		{
			var cnnOptions = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
			
			return new SqlConnection(cnnOptions.Mssql);
		});
		
		return builder;
	}
	
	public static WebApplicationBuilder SetEmailOptions(this WebApplicationBuilder builder)
	{
		builder.Services
			.AddOptions<EmailOptions>()
			.Bind(builder.Configuration.GetRequiredSection(EmailOptions.Email))
			.Validate(o => !string.IsNullOrEmpty(o.Address), "Email address is required")
			.Validate(o => !string.IsNullOrEmpty(o.Password), "Email password is required")
			.ValidateOnStart();
		
		return builder;
	}
	
	public static WebApplicationBuilder SetVerificationCodeOptions(this WebApplicationBuilder builder)
	{
		builder.Services
			.AddOptions<VerificationCodeOptions>()
			.Bind(builder.Configuration.GetRequiredSection(VerificationCodeOptions.VerificationCode))
			.Validate(x => !string.IsNullOrEmpty(x.Text), "Text for code generation is required")
			.Validate(x => x.Length >= 6, "Length for code generation must contain at least 6 characters")
			.ValidateOnStart();
		
		return builder;
	}
	
	public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
	{
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(builder.Configuration)
			.CreateLogger();
		
		// replaces built-in logger with Serilog and registers it as a singleton 
		builder.Host.UseSerilog(Log.Logger, true);
		
		return builder;
	}
}