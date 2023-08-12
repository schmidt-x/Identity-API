namespace IdentityApi.Contracts.Requests;

public class UsernameUpdateRequest
{
	public string Username { get; set; } = default!;
	public string Password { get; set; } = default!;
}