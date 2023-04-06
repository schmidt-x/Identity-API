namespace IdentityApi.Validators;

public class UserRegisterValidator : AbstractValidator<UserRegister>
{
	public UserRegisterValidator()
	{
		RuleFor(x => x.Username)
			.NotNull()
			.NotEmpty()
			.MinimumLength(3);
		
		RuleFor(x => x.Email)
			;
		
		RuleFor(x => x.Password)
			.NotNull()
			.NotEmpty()
			.MinimumLength(8)
			.Custom((password, context) =>
			{
				var letter = false;
				var digit = false;
				var symbol = false;
				var lower = false;
				var upper = false;
		
				foreach(var l in password)
				{
					if (char.IsLetter(l)) letter = true;
					if (char.IsDigit(l)) digit = true;
					if (char.IsPunctuation(l)) symbol = true;
					if (char.IsLower(l)) lower = true;
					if (char.IsUpper(l)) upper = true;
				}
				
				if(!letter) context.AddFailure("we need letter");
				if(!digit) context.AddFailure("we need digit");
				if(!symbol) context.AddFailure("we need symbol");
				if(!lower) context.AddFailure("we need lower");
				if(!upper) context.AddFailure("we need upper");
			});
		
		RuleFor(x => x.ConfirmPassword)
			.NotNull()
			.NotEmpty()
			.Equal(x => nameof(x.Password)).WithMessage("Passwords not match")
			;
	}
}