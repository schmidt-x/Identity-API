namespace IdentityApi.Validation;

public class EmailRequestValidator : AbstractValidator<EmailRequest>
{
	public EmailRequestValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty().WithMessage("Email address required")
			.EmailAddress().WithMessage("Invalid email address");
	}
}