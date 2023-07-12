using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityApi.Extensions;
using IdentityApi.Filters;
using IdentityApi.Installers;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
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
		
		builder.Services.AddFilters();
		builder.Services.AddRepositories();
		builder.Services.AddDataAccess();
		builder.Services.AddServices();
		
		builder.AddJwtAuthentication();
		builder.Services.AddAuthorizationWithPolicies();
		
		builder.AddFluentMigrator();
		
		builder
			.SetConnectionStringsOptions()
			.SetEmailOptions()
			.SetVerificationCodeOptions();
		
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
		
		app.UseExceptionHandlerMiddleware();
		app.UseSerilogRequestLogging();
		app.MapControllers();
		
		app.UseSwagger();
		app.UseSwaggerUI();
		
		app.RunMigrations();
		
		app.UseAuthentication();
		app.UseAuthorization();
		
		app.Run();
	}
}