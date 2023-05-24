using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityApi.Extensions;

public static class MigrationExtensions
{
	public static WebApplicationBuilder AddFluentMigrator(this WebApplicationBuilder builder)
	{
		builder.Services
			.AddFluentMigratorCore()
			.ConfigureRunner(mrb =>
			{
				mrb.AddSqlServer()
					 .WithGlobalConnectionString(builder.Configuration.GetConnectionString("Mssql"))
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