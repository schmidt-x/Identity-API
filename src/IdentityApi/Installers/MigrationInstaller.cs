using System.Reflection;
using FluentMigrator.Runner;
using IdentityApi.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IdentityApi.Installers;

public static class MigrationInstaller
{
	public static WebApplicationBuilder AddFluentMigrator(this WebApplicationBuilder builder)
	{
		builder.Services
			.AddFluentMigratorCore()
			.ConfigureRunner(mrb =>
			{
				mrb.AddSqlServer()
					 .WithGlobalConnectionString(sp =>
					 {
						 var cnnOptions = sp.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
						 return cnnOptions.Mssql;
					 })
					 .ScanIn(Assembly.GetExecutingAssembly()).For
					 .Migrations();
			})
			.AddLogging(lb => lb.AddFluentMigratorConsole());
		
		return builder;
	}
	
	public static IApplicationBuilder RunMigrations(this IApplicationBuilder app)
	{
		using var scope = app.ApplicationServices.CreateScope();
		var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
		runner.MigrateUp();
		
		return app;
	}
}