using System.Text;
using IdentityApi.Options;
using Microsoft.Extensions.Options;

namespace IdentityApi.Validation.OptionsValidation;

public class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
	public ValidateOptionsResult Validate(string? name, JwtOptions options)
	{
		var sb = new StringBuilder();
		
		if (string.IsNullOrEmpty(options.SecretKey))
		{
			sb.Append("Secret key is required\n");
		} 
		else if (options.SecretKey.Length < 32)
		{
			sb.Append($"The secret key must contain at least 32 characters. " +
								$"Actual: {options.SecretKey.Length}\n");
		}
		
		if (string.IsNullOrEmpty(options.Issuer))
		{
			sb.Append("Issuer is required\n");
		}
		
		if (string.IsNullOrEmpty(options.Audience))
		{
			sb.Append("Audience is required\n");
		}
		
		if (options.AccessTokenLifeTime.TotalMinutes is > 5 or < 1)
		{
			sb.Append($"The total minutes of access token must be in the range of 1 to 5. " +
								$"Actual: {options.AccessTokenLifeTime.TotalMinutes}\n");
		}
		
		if (options.RefreshTokenLifeTime.TotalDays is < 7 or > 180)
		{
			sb.Append($"The total days of refresh token must be in the range of 7 to 180. " +
								$"Actual: {options.RefreshTokenLifeTime.TotalDays}");
		}
		
		return sb.Length != 0
			? ValidateOptionsResult.Fail(sb.ToString())
			: ValidateOptionsResult.Success;
	}
}