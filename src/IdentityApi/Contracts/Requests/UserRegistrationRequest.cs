namespace IdentityApi.Contracts.Requests;

public class UserRegistrationRequest
{
	public string Username { get; set; } = default!;
	public string Password { get; set; } = default!;
}