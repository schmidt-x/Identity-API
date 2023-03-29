using Microsoft.AspNetCore.Builder;


namespace IdentityApi;

public class Program
{
	public static void Main()
	{
		var builder = WebApplication.CreateBuilder();
		
		
		var app = builder.Build();
		
		
		app.MapGet("/", () => "Hello World!");
		
		
		app.Run();
	}
}
