using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation.AspNetCore;
using IdentityApi.Extensions;
using IdentityApi.Filters;
using IdentityApi.Installers;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
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
		
		builder.SetConnectionStringsOptions();
		builder.SetEmailOptions();
		
		builder.Services
			.AddValidators()
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
			.AddSwaggerGen(o =>
			{
				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
			})
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
		
		// Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
		
		app.Run();
	}
}