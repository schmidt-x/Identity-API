namespace IdentityApi.Contracts.Requests;

public class PasswordUpdateRequest
{
	public string Password { get; set; } = default!;
	public string NewPassword { get; set; } = default!;
}