namespace IdentityApi.Domain.Models;

public class EmailSession
{
	public string EmailAddress { get; set; } = default!;
	public string VerificationCode { get; set; } = default!;
	public bool IsVerified { get; set; }
	public int Attempts { get; set; }
}