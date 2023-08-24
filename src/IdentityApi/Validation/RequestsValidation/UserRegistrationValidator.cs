using FluentValidation;
using IdentityApi.Contracts.Requests;
using IdentityApi.Validation.Helpers;

namespace IdentityApi.Validation.RequestsValidation;

public class UserRegistrationRequestValidator : AbstractValidator<UserRegistrationRequest>
{
	public UserRegistrationRequestValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is required")
			.MinimumLength(3).WithMessage("Username must contain at least 3 characters")
			.MaximumLength(32).WithMessage("Username must not exceed the limit of 32 characters")
			.Custom((username, context) =>
			{
				if (ValidationHelper.ContainsRestrictedCharacters(username))
					context.AddFailure("Username can only contain letters, numbers, underscores and periods");
			});
		
		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required")
			.MinimumLength(8).WithMessage("Password must contain at least 8 characters")
			.Custom(ValidatePassword);
	}

	private static void ValidatePassword(string password, ValidationContext<UserRegistrationRequest> context)
	{
		if (string.IsNullOrWhiteSpace(password)) return;
				
		var hasLetter = false;
		var hasDigit = false;
		var hasSymbol = false;
		var hasLower = false;
		var hasUpper = false;
		var hasSpace = false;
		
		foreach(var l in password)
		{
			if (!hasLetter) hasLetter = char.IsLetter(l);
			if (!hasDigit) hasDigit = char.IsDigit(l);
			if (!hasSymbol) hasSymbol = char.IsSymbol(l) || char.IsPunctuation(l);
			if (!hasLower) hasLower = char.IsLower(l);
			if (!hasUpper) hasUpper = char.IsUpper(l);
			if (!hasSpace) hasSpace = char.IsWhiteSpace(l);
		}
				
		if (!hasLetter) context.AddFailure("Password must contain at least one letter");
		if (!hasDigit) context.AddFailure("Password must contain at least one digit");
		if (!hasSymbol) context.AddFailure("Password must contain at least one symbol");
		if (!hasLower) context.AddFailure("Password must contain at least one lower-case character");
		if (!hasUpper) context.AddFailure("Password must contain at least one upper-case character");
		if (hasSpace) context.AddFailure("Password must not contain any white spaces");
	}
}