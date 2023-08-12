using FluentValidation;
using IdentityApi.Contracts.Requests;

namespace IdentityApi.Validation.RequestsValidation;

public class TokenRefreshingRequestValidator : AbstractValidator<TokenRefreshingRequest>
{
	public TokenRefreshingRequestValidator()
	{
		RuleFor(x => x.AccessToken)
			.NotEmpty().WithMessage("Access token is required");
		
		RuleFor(x => x.RefreshToken)
			.NotEmpty().WithMessage("Refresh token is required");
	}
}