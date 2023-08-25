using FluentValidation;
using IdentityApi.Contracts.Requests;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Validation.RequestsValidation;

public class UsernameUpdateRequestValidator : AbstractValidator<UsernameUpdateRequest>
{
	public UsernameUpdateRequestValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().OverridePropertyName(ErrorKey.Username)
				.WithMessage(ErrorMessage.UsernameRequired)
			.MinimumLength(3).WithMessage(ErrorMessage.UsernameTooShort)
			.MaximumLength(32).WithMessage(ErrorMessage.UsernameTooLong)
			.Custom((username, context) =>
			{
				if (ValidationHelper.UsernameContainsRestrictedSymbols(username))
					context.AddFailure(ErrorMessage.UsernameContainsRestrictedSymbols);
					
			}).When(x => !string.IsNullOrWhiteSpace(x.Username), ApplyConditionTo.CurrentValidator);
		
		RuleFor(x => x.Password)
			.NotEmpty().OverridePropertyName(ErrorKey.Password)
				.WithMessage(ErrorMessage.PasswordRequired);
	}
}