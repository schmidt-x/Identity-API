using FluentValidation;
using IdentityApi.Contracts.Requests;
using IdentityApi.Validation;

namespace IdentityApi.Validation.RequestsValidation;

public class UsernameUpdateRequestValidator : AbstractValidator<UsernameUpdateRequest>
{
	public UsernameUpdateRequestValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is required")
			.MinimumLength(3).WithMessage("Username must contain at least 3 characters")
			.MaximumLength(32).WithMessage("Username must not exceed the limit of 32 characters")
			.Custom((username, context) =>
			{
				if (ValidationHelper.UsernameContainsRestrictedCharacters(username))
					context.AddFailure("Username can only contain letters, numbers, underscores and periods");
					
			}).When(x => !string.IsNullOrWhiteSpace(x.Username), ApplyConditionTo.CurrentValidator);
		
		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required");
	}
}