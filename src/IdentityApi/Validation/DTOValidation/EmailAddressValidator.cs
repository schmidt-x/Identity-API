using FluentValidation;
using IdentityApi.Contracts.DTOs;

namespace IdentityApi.Validation.DTOValidation;

public class EmailAddressValidator : AbstractValidator<EmailAddress>
{
	public EmailAddressValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email address is required")
			.EmailAddress().WithMessage("Invalid email address");
	}
}