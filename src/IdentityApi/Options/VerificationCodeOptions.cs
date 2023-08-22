namespace IdentityApi.Options;

public class VerificationCodeOptions
{
	public const string VerificationCode = "VerificationCode";

	public string Text { get; set; } = default!;
	public int Length { get; set; }
}