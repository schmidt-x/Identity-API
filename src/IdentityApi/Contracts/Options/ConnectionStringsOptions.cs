namespace IdentityApi.Contracts.Options;

public class ConnectionStringsOptions
{
	public const string ConnectionStrings = "ConnectionStrings";

	public string Mssql { get; set; } = string.Empty;
}