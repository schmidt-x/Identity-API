namespace IdentityApi.Contracts.DTOs;

public class PasswordChangeRequest
{
	public string Password { get; set; } = default!;
	public string NewPassword { get; set; } = default!;
}