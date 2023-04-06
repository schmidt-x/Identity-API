namespace IdentityLibrary.Models;

public class UserClaims
{
	public Guid Id { get; set; }
	public string Email { get; set; }
	public string Role { get; set; }
}