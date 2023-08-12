using FluentValidation;
using IdentityApi.Contracts.Requests;

namespace IdentityApi.Validation.RequestsValidation;

public class CodeVerificationRequestValidator : AbstractValidator<CodeVerificationRequest>
{
	public CodeVerificationRequestValidator()
	{
		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Verificaiton code is required");
	}
}