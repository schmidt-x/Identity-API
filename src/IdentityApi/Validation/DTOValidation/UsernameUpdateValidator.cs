using FluentValidation;
using IdentityApi.Contracts.DTOs;

namespace IdentityApi.Validation.DTOValidation;

public class UsernameUpdateValidator : AbstractValidator<UsernameUpdate>
{
	public UsernameUpdateValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is required")
			.MinimumLength(3).WithMessage("Username must contain at least 3 characters");
	}
}