namespace IdentityApi.Validation;

public class UserRegisterValidator : AbstractValidator<UserRegistration>
{
	public UserRegisterValidator()
	{
		RuleFor(x => x.Username)
			.Custom(ValidateUsername);
		
		RuleFor(x => x.Password)
			.Custom(ValidatePassword);
		
		RuleFor(x => x.ConfirmPassword)
			.Custom(ValidateConfirmPassword);
	}

	private static void ValidateUsername(string username, ValidationContext<UserRegistration> context)
	{
		if (string.IsNullOrWhiteSpace(username))
		{
			context.AddFailure("Username required");
			return;
		}

		if (username.Length < 3)
			context.AddFailure("Username must contain at least 3 characters");
	}
	private static void ValidateConfirmPassword(string confirmPassword, ValidationContext<UserRegistration> context)
	{
		if (string.IsNullOrWhiteSpace(confirmPassword))
		{
			context.AddFailure("Password confirmation required");
			return;
		}

		if (confirmPassword != context.InstanceToValidate.Password)
			context.AddFailure("Passwords not match");
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