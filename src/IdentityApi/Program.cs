namespace IdentityApi;

public class Program
{
	public static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		builder.Host.UseSerilog((context, config) =>
			config.ReadFrom.Configuration(context.Configuration));
		
		builder.Services.SetIdentityConfiguration(builder.Configuration);
		builder.Services.AddFilters();
		builder.Services.AddRequestValidation();
		builder.Services.AddIdentityLibrary();
		
		builder.Services.AddFluentValidationAutoValidation();
		builder.Services.AddHttpContextAccessor();
		
		builder.Services.AddControllers(options =>
		{
			options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
		}).ConfigureApiBehaviorOptions(abo => abo.SuppressModelStateInvalidFilter = true);
		
		builder.Services.AddSwaggerGen(o =>
		{
    	var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    	o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFile));
		});
		
		builder.Services.AddMemoryCache();
		
		var app = builder.Build();
		
		app.UseExceptionHandlerMiddleware();
		app.UseSwagger();
		app.UseSwaggerUI();
		
		// Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
		
		app.MapControllers();
		app.UseSerilogRequestLogging();
		
		app.Run();
	}
}