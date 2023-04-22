namespace IdentityLibrary.Models;

public class UserSession
{
	public string EmailAddress { get; set; }
	public string VerificationCode { get; set; }
	public bool IsVerified { get; set; }
	public int Attempts { get; set; }
}