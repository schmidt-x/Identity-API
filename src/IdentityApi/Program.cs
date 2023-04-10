namespace IdentityApi;

public class Program
{
	public static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		builder.Host.UseSerilog((context, config) =>
			config.ReadFrom.Configuration(context.Configuration));
		
		builder.Services.AddIdentityLibrary();
		
		builder.Services.AddControllers(options =>
		{
			options.Filters.Add<ValidationActionFilter>();
			options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
		}).ConfigureApiBehaviorOptions(abo => abo.SuppressModelStateInvalidFilter = true);
		
		builder.Services.AddUserValidation();
		builder.Services.AddFluentValidationAutoValidation();
		
		var app = builder.Build();
		
		app.MapControllers();
		app.UseSerilogRequestLogging();
		
		app.Run();
	}
}