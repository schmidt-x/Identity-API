using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace IdentityApi.Installers;

public static class SwaggerInstaller
{
	public static IServiceCollection AddSwagger(this IServiceCollection services)
	{
		return services
			.AddSwaggerGen(o =>
			{
				o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
				{
					Description = "JWT Authorization header using the Bearer scheme\r\n\r\n(put ONLY your JWT Bearer token on textbox below)",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
					Scheme = JwtBearerDefaults.AuthenticationScheme
				});
				
				o.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{ new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Id = JwtBearerDefaults.AuthenticationScheme,
							Type = ReferenceType.SecurityScheme,
						},
					}, new List<string>()}
				});
				
				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
			});
	}
}