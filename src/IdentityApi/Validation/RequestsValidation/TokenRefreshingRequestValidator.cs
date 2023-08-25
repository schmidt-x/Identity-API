using FluentValidation;
using IdentityApi.Contracts.Requests;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Validation.RequestsValidation;

public class TokenRefreshingRequestValidator : AbstractValidator<TokenRefreshingRequest>
{
	public TokenRefreshingRequestValidator()
	{
		RuleFor(x => x.AccessToken)
			.NotEmpty().OverridePropertyName(ErrorKey.AccessToken)
				.WithMessage(ErrorMessage.AccessTokenRequired);
		
		RuleFor(x => x.RefreshToken)
			.NotEmpty().OverridePropertyName(ErrorKey.RefreshToken)
				.WithMessage(ErrorMessage.RefreshTokenRequired);
	}
}