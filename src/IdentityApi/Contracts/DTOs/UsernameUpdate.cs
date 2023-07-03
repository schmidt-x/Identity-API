namespace IdentityApi.Contracts.DTOs;

public class UsernameUpdate
{
	public string Username { get; set; } = default!;
	public string Password { get; set; } = default!;
}