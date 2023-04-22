namespace IdentityApi.Validation;

public class VerificationRequestValidator : AbstractValidator<VerificationRequest>
{
	public VerificationRequestValidator()
	{
		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Verification code is required")
			.Length(6, 6).WithMessage("Invalid verification code");
	}
}