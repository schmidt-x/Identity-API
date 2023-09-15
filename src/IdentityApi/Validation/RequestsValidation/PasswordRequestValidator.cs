using FluentValidation;
using IdentityApi.Contracts.Requests;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Validation.RequestsValidation;

public class PasswordRequestValidator : AbstractValidator<PasswordRequest>
{
	public PasswordRequestValidator()
	{
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