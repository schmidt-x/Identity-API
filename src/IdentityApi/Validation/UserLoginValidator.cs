namespace IdentityApi.Validation;

public class UserLoginValidator : AbstractValidator<UserLogin>
{
	public UserLoginValidator()
	{
		RuleFor(u => u.Email)
			.NotEmpty().WithMessage("Username is required");
			
		RuleFor(u => u.Password)
			.NotEmpty().WithMessage("Password is required");
	}
}