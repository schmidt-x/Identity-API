using System.Linq;

namespace IdentityApi.Validators;

public class UserRegisterValidator : AbstractValidator<UserRegister>
{
	public UserRegisterValidator()
	{
		RuleFor(x => x.Username)
			.Custom(ValidateUsername);
		
		RuleFor(x => x.Email)
			.Custom(ValidateEmail);
		
		RuleFor(x => x.Password)
			.Custom(ValidatePassword);
		
		RuleFor(x => x.ConfirmPassword)
			.Custom(ValidateConfirmPassword);
	}

	private static void ValidateUsername(string username, ValidationContext<UserRegister> context)
	{
		if (string.IsNullOrWhiteSpace(username))
		{
			context.AddFailure("Username required");
			return;
		}

		if (username.Length < 3)
			context.AddFailure("Username must contain at least 3 characters");
	}
	private static void ValidateConfirmPassword(string confirmPassword, ValidationContext<UserRegister> context)
	{
		if (string.IsNullOrWhiteSpace(confirmPassword))
		{
			context.AddFailure("Password confirmation required");
			return;
		}

		if (confirmPassword != context.InstanceToValidate.Password)
			context.AddFailure("Password do not match");
	}
	private static void ValidatePassword(string? password, ValidationContext<UserRegister> context)
	{
		if (string.IsNullOrWhiteSpace(password))
		{
			context.AddFailure("Password required");
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
	private static void ValidateEmail(string? email, ValidationContext<UserRegister> context)
	{
		if (string.IsNullOrWhiteSpace(email))
		{
			context.AddFailure("Email required");
			return;
		}
		
		var errorMessage = "Email invalid";
		
		if (!char.IsLetter(email[0]))
		{
			context.AddFailure(errorMessage);
			return;
		}
		
		var parts = email.Split('@', StringSplitOptions.RemoveEmptyEntries);
		
		if (parts.Length != 2)
		{
			context.AddFailure(errorMessage);
			return;
		}
		 
		var local = parts[0];
		
		if (!char.IsLetter(local[^1]))
		{
			context.AddFailure(errorMessage);
			return;
		}
		
		var domainParts = parts[1].Split('.');
		
		if (domainParts.Length < 2)
		{
			context.AddFailure(errorMessage);
			return;
		}
		
		foreach(var part in domainParts)
		{
			if (part.Length < 1)
			{
				context.AddFailure(errorMessage);
				return;
			}
			
			if (part.Any(l => !char.IsLetter(l) && !char.IsDigit(l) && l != '-'))
			{
				context.AddFailure(errorMessage);
				return;
			}
		}
	}
}