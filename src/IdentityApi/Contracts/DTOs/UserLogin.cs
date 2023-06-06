namespace IdentityApi.Contracts.DTOs;

public class UserLogin
{
	public string Login { get; set; } = default!;
	public string Password { get; set; } = default!;
}