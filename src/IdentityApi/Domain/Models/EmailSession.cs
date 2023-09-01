namespace IdentityApi.Domain.Models;

public class EmailSession : Session
{
	public string EmailAddress { get; set; } = default!;
}