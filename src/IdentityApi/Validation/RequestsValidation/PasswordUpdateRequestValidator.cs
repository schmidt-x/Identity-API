using FluentValidation;
using IdentityApi.Contracts.Requests;
using IdentityApi.Validation;

namespace IdentityApi.Validation.RequestsValidation;

public class PasswordUpdateRequestValidator : AbstractValidator<PasswordUpdateRequest>
{
	public PasswordUpdateRequestValidator()
	{
		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required");
		
		RuleFor(x => x.NewPassword)
			.NotEmpty().WithMessage("New password is required")
			.NotEqual(x => x.Password).WithMessage("New password cannot be the same as the current password")
			.Custom((password, context) =>
			{
				foreach(var failure in ValidationHelper.ValidatePassword(password))
					context.AddFailure(failure);
					
			}).When(x => !string.IsNullOrWhiteSpace(x.Password), ApplyConditionTo.CurrentValidator);
	}
}