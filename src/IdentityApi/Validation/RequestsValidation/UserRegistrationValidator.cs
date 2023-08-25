using FluentValidation;
using IdentityApi.Contracts.Requests;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Validation.RequestsValidation;

public class UserRegistrationRequestValidator : AbstractValidator<UserRegistrationRequest>
{
	public UserRegistrationRequestValidator()
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
				.WithMessage(ErrorMessage.PasswordRequired)
			.MinimumLength(8).WithMessage(ErrorMessage.PasswordTooShort)
			.Custom((password, context) =>
			{
				foreach(var failure in ValidationHelper.ValidatePassword(password))
					context.AddFailure(failure);
					
			}).When(x => !string.IsNullOrWhiteSpace(x.Password), ApplyConditionTo.CurrentValidator);
	}
}