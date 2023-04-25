namespace IdentityApi.Validation;

public class CodeVerificaitonValidator : AbstractValidator<CodeVerification>
{
	public CodeVerificaitonValidator()
	{
		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Verificaiton code is required");
	}
}