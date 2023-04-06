using IdentityApi.Validators;

namespace IdentityApi;

public class Program
{
	public static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		builder.Host.UseSerilog((context, config) =>
			config.ReadFrom.Configuration(context.Configuration));
		
		builder.Services.AddScoped<ValidationFilter>();
		
		builder.Services.AddIdentityLibrary();
		builder.Services.AddControllers()
			.ConfigureApiBehaviorOptions(abo => abo.SuppressModelStateInvalidFilter = true);
		
		builder.Services.AddScoped<IValidator<UserRegister>, UserRegisterValidator>();
		builder.Services.AddFluentValidationAutoValidation();
		
		var app = builder.Build();
		
		app.MapControllers();
		app.UseSerilogRequestLogging();
		
		app.Run();
	}
}