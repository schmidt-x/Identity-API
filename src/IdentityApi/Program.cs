using System.Data.SqlClient;
using System.Net;
using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityApi.Data.Repositories;
using IdentityApi.Extensions;
using IdentityApi.Factories;
using IdentityApi.Filters;
using IdentityApi.Installers;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace IdentityApi;

public class Program
{
	public static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		builder.Host.UseSerilog((context, config) =>
			config.ReadFrom.Configuration(context.Configuration));
		
		builder.Services.AddRouting(options => options.LowercaseUrls = true);
		
		builder
			.SetConnectionStringsOptions()
			.SetEmailOptions()
			.SetVerificationCodeOptions();
		
		builder.AddMssql();
		builder.Services.AddTransactionFactory();
		
		builder.Services.AddFilters();
		builder.Services.AddRepositories();
		builder.Services.AddDataAccess();
		builder.Services.AddServices();
		
		builder
			.AddJwtAuthentication()
			.AddAuthorizationWithPolicies();
		
		builder.AddFluentMigrator();
		
		builder.Services
			.AddValidatorsFromAssemblyContaining<Program>()
			.AddFluentValidationAutoValidation();
		
		builder.Services.AddHttpContextAccessor();
		
		builder.Services
			.AddControllers(options =>
			{
				options.Filters.Add(typeof(ModerlStateErrorsHandlerActionFilter));
				options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
			})
			.ConfigureApiBehaviorOptions(options => 
				options.SuppressModelStateInvalidFilter = true);
		
		builder.Services
			.AddSwagger()
			.AddFluentValidationRulesToSwagger();
		
		builder.Services.AddMemoryCache();
		
		var app = builder.Build();
		
		app.UseSerilogRequestLogging();
		app.UseExceptionHandlerMiddleware();
		
		app.MapControllers();
		
		app.UseSwagger();
		app.UseSwaggerUI();
		
		app.RunMigrations();
		
		app.UseAuthentication();
		app.UseAuthorization();
		
		app.Run();
	}
}