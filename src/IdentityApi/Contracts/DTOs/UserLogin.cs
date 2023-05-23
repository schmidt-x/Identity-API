namespace IdentityApi.Contracts.DTOs;

public class UserLogin
{
	public string Email { get; set; } = default!;
	public string Password { get; set; } = default!;
}