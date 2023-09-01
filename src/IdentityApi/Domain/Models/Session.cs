namespace IdentityApi.Domain.Models;

public class Session
{
	public string VerificationCode { get; set; } = default!;
	public bool IsVerified { get; set; }
	public int Attempts { get; set; }
}