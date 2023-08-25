using FluentValidation;
using IdentityApi.Contracts.Requests;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Validation.RequestsValidation;

public class PasswordUpdateRequestValidator : AbstractValidator<PasswordUpdateRequest>
{
	public PasswordUpdateRequestValidator()
	{
		RuleFor(x => x.Password)
			.NotEmpty().OverridePropertyName(ErrorKey.Password)
				.WithMessage(ErrorMessage.PasswordRequired);
		
		RuleFor(x => x.NewPassword)
			.NotEmpty().OverridePropertyName(ErrorKey.NewPassword)
				.WithMessage(ErrorMessage.PasswordRequired)
			.MinimumLength(8).WithMessage(ErrorMessage.PasswordTooShort)
			.NotEqual(x => x.Password).WithMessage(ErrorMessage.PasswordsEqual)
			.Custom((password, context) =>
			{
				foreach(var failure in ValidationHelper.ValidatePassword(password))
					context.AddFailure(failure);
					
			}).When(x => !string.IsNullOrWhiteSpace(x.Password), ApplyConditionTo.CurrentValidator);
	}
}