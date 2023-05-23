namespace IdentityApi.Contracts.DTOs;

public class UserRegistration
{
	public string Username { get; set; } = default!;
	public string Password { get; set; } = default!;
	public string ConfirmPassword { get; set; } = default!;
}