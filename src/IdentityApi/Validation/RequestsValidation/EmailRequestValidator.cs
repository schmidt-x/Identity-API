using FluentValidation;
using IdentityApi.Contracts.Requests;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Validation.RequestsValidation;

public class EmailRequestValidator : AbstractValidator<EmailRequest>
{
	public EmailRequestValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty().OverridePropertyName(ErrorKey.Email)
				.WithMessage(ErrorMessage.EmailRequired)
			.EmailAddress().WithMessage(ErrorMessage.EmailInvalid);
	}
}