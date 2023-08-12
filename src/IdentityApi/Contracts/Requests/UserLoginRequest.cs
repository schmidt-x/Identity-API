namespace IdentityApi.Contracts.Requests;

public class UserLoginRequest
{
	public string Login { get; set; } = default!;
	public string Password { get; set; } = default!;
}