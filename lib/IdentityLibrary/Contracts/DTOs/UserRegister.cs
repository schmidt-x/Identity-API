namespace IdentityLibrary.Contracts.DTOs;

public class UserRegister
{
	public string Username { get; set; }
	public string Password { get; set; }
	public string ConfirmPassword { get; set; }
}