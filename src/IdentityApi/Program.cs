using System;
using System.IO;
using System.Reflection;
using System.Threading;
using FluentValidation.AspNetCore;
using IdentityApi.Extensions;
using IdentityApi.Installers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
		builder.Services.AddValidators();
		
		builder.AddJwtAuthentication();
		builder.Services.AddAuthorizationWithPolicies();
		
		builder.AddFluentMigrator();
		builder.SetConnectionStringsOptions();
		builder.SetEmailOptions();
		
		builder.Services.AddFluentValidationAutoValidation();
		builder.Services.AddHttpContextAccessor();
		
		builder.Services.AddControllers(options =>
		{
			options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
		}).ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
		
		builder.Services.AddSwaggerGen(o =>
		{
			var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
		});
		
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