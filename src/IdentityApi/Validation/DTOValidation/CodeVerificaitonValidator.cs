using FluentValidation;
using IdentityApi.Contracts.DTOs;

namespace IdentityApi.Validation.DTOValidation;

public class CodeVerificaitonValidator : AbstractValidator<CodeVerification>
{
	public CodeVerificaitonValidator()
	{
		RuleFor(x => x.Code)
			.NotEmpty().WithMessage("Verificaiton code is required");
	}
}