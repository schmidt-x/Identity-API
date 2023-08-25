using FluentValidation;
using IdentityApi.Contracts.Requests;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Validation.RequestsValidation;

public class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
	public UserLoginRequestValidator()
	{
		RuleFor(u => u.Login)
			.NotEmpty().OverridePropertyName(ErrorKey.Username)
				.WithMessage(ErrorMessage.UsernameRequired);
			
		RuleFor(u => u.Password)
			.NotEmpty().OverridePropertyName(ErrorKey.Password)
				.WithMessage(ErrorMessage.PasswordRequired);
	}
}