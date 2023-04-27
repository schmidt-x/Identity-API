namespace IdentityApi.Validation;

public class RefreshTokenRequestValidation : AbstractValidator<RefreshTokenRequest>
{
	public RefreshTokenRequestValidation()
	{
		RuleFor(x => x.AccessToken)
			.NotEmpty().WithMessage("Access token is required");
		
		RuleFor(x => x.RefreshToken)
			.NotEmpty().WithMessage("Refresh token is required");
	}
}