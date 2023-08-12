using FluentValidation;
using IdentityApi.Contracts.DTOs;

namespace IdentityApi.Validation.DTOValidation;

public class PasswordChangeRequestValidator : AbstractValidator<PasswordChangeRequest>
{
	public PasswordChangeRequestValidator()
	{
		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required");
		
		RuleFor(x => x.NewPassword)
			.NotEmpty().WithMessage("New password is required")
			.NotEqual(x => x.Password).WithMessage("New password cannot be the same as old password");
	}
}