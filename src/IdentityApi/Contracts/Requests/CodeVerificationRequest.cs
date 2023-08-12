namespace IdentityApi.Contracts.Requests;

public class CodeVerificationRequest
{
	public string Code { get; set; } = default!;
}