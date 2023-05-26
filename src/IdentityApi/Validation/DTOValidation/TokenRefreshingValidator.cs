using FluentValidation;
using IdentityApi.Contracts.DTOs;

namespace IdentityApi.Validation.DTOValidation;

public class TokenRefreshingValidator : AbstractValidator<TokenRefreshing>
{
	public TokenRefreshingValidator()
	{
		RuleFor(x => x.AccessToken)
			.NotEmpty().WithMessage("Access token is required");
		
		RuleFor(x => x.RefreshToken)
			.NotEmpty().WithMessage("Refresh token is required");
	}
}