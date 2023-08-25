using FluentValidation;
using IdentityApi.Contracts.Requests;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Validation.RequestsValidation;

public class CodeVerificationRequestValidator : AbstractValidator<CodeVerificationRequest>
{
	public CodeVerificationRequestValidator()
	{
		RuleFor(x => x.Code)
			.NotEmpty().OverridePropertyName(ErrorKey.Code)
				.WithMessage(ErrorMessage.VerificationCodeRequired);
	}
}