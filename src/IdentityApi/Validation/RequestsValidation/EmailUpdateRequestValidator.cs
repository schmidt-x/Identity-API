using FluentValidation;
using IdentityApi.Contracts.Requests;

namespace IdentityApi.Validation.RequestsValidation;

public class EmailUpdateRequestValidator : AbstractValidator<EmailRequest>
{
	public EmailUpdateRequestValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email address is required")
			.EmailAddress().WithMessage("Invalid email address");
	}
}