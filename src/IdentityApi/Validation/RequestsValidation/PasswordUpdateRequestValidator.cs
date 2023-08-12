using FluentValidation;
using IdentityApi.Contracts.Requests;

namespace IdentityApi.Validation.RequestsValidation;

public class PasswordUpdateRequestValidator : AbstractValidator<PasswordUpdateRequest>
{
	public PasswordUpdateRequestValidator()
	{
		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required");
		
		RuleFor(x => x.NewPassword)
			.NotEmpty().WithMessage("New password is required")
			.NotEqual(x => x.Password).WithMessage("New password cannot be the same as old password");
	}
}