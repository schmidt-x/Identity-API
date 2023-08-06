using FluentValidation;
using IdentityApi.Contracts.DTOs;

namespace IdentityApi.Validation.DTOValidation;

public class UserRegisterValidator : AbstractValidator<UserRegistration>
{
	public UserRegisterValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username is required")
			.MinimumLength(3).WithMessage("Username must contain at least 3 characters");
		
		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Password is required")
			.MinimumLength(8).WithMessage("Password must contain at least 8 characters")
			.Custom(ValidatePassword);
	}

	private static void ValidatePassword(string? password, ValidationContext<UserRegistration> context)
	{
		// the reason why I didn't handle 'Not empty' and 'MinimumLength' requirements here
		// is that the method 'AddFluentValidationRulesToSwagger' does not detect 'Custom' restrictions
		// and they are not applied to the Swagger documentation
		
		if (string.IsNullOrWhiteSpace(password)) return;
				
		var letter = false;
		var digit = false;
		var symbol = false;
		var lower = false;
		var upper = false;
			
		foreach(var l in password)
		{
			if (char.IsLetter(l)) letter = true;
			if (char.IsDigit(l)) digit = true;
			if (char.IsPunctuation(l) || char.IsSymbol(l)) symbol = true;
			if (char.IsLower(l)) lower = true;
			if (char.IsUpper(l)) upper = true;
		}
				
		if(!letter) context.AddFailure("Password must contain at least one letter");
		if(!digit) context.AddFailure("Password must contain at least one digit");
		if(!symbol) context.AddFailure("Password must contain at least one symbol");
		if(!lower) context.AddFailure("Password must contain at least one lower-case character");
		if(!upper) context.AddFailure("Password must contain at least one upper-case character");
	}
}