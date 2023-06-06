using FluentValidation;
using IdentityApi.Contracts.DTOs;

namespace IdentityApi.Validation.DTOValidation;

public class UserRegisterValidator : AbstractValidator<UserRegistration>
{
	public UserRegisterValidator()
	{
		RuleFor(x => x.Username)
			.NotEmpty().WithMessage("Username required")
			.MinimumLength(3).WithMessage("Username must contain at least 3 characters");
		
		RuleFor(x => x.Password)
			.Custom(ValidatePassword);
		
		RuleFor(x => x.ConfirmPassword)
			.NotEmpty().WithMessage("Password confirmation is required")
			.Equal(user => user.Password).WithMessage("Passwords do not match");
	}

	private static void ValidatePassword(string? password, ValidationContext<UserRegistration> context)
	{
		if (string.IsNullOrWhiteSpace(password))
		{
			context.AddFailure("Password is required");
			return;
		}
				
		if (password.Length < 8)
			context.AddFailure("Password must contain at least 8 characters");
				
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