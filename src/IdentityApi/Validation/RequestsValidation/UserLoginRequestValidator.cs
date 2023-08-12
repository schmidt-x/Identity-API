using FluentValidation;
using IdentityApi.Contracts.Requests;

namespace IdentityApi.Validation.RequestsValidation;

public class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
	public UserLoginRequestValidator()
	{
		RuleFor(u => u.Login)
			.NotEmpty().WithMessage("Username is required");
			
		RuleFor(u => u.Password)
			.NotEmpty().WithMessage("Password is required");
	}
}