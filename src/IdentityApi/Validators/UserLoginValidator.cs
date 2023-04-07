namespace IdentityApi.Validators;

public class UserLoginValidator : AbstractValidator<UserLogin>
{
	public UserLoginValidator()
	{
		RuleFor(u => u.Username)
			.Custom((username, context) =>
			{
				if (string.IsNullOrWhiteSpace(username))
					context.AddFailure("Username required");
			});
			
		RuleFor(u => u.Password)
			.Custom((username, context) =>
			{
				if (string.IsNullOrWhiteSpace(username))
					context.AddFailure("Password required");
			});
	}
}