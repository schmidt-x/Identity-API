using FluentValidation;
using IdentityApi.Contracts.DTOs;

namespace IdentityApi.Validation;

public class EmailRequestValidator : AbstractValidator<EmailRegistration>
{
	public EmailRequestValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email address is required")
			.EmailAddress().WithMessage("Invalid email address");
	}
}