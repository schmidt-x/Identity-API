using FluentValidation;
using IdentityApi.Contracts.Requests;

namespace IdentityApi.Validation.RequestsValidation;

public class UsernameUpdateRequestValidator : AbstractValidator<UsernameUpdateRequest>
{
	public UsernameUpdateRequestValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is required")
			.MinimumLength(3).WithMessage("Username must contain at least 3 characters");
		
		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required");
	}
}