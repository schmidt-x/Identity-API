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
			sb.Append("The secret key must contain at least 32 characters. ")
				.Append("Actual: ").Append(options.SecretKey.Length)
				.AppendLine();
		}
		
		if (string.IsNullOrEmpty(options.Issuer))
		{
			sb.Append("Issuer is required\n");
		}
		
		if (string.IsNullOrEmpty(options.Audience))
		{
			sb.Append("Audience is required\n");
		}
		
		if (options.AccessTokenLifeTime.TotalSeconds is > 300 or < 60)
		{
			sb.Append("The total time of access token must be in the range of 1 to 5 minutes. ")
				.Append("Actual: ").Append(options.AccessTokenLifeTime.ToString())
				.AppendLine();
		}

		if (options.RefreshTokenLifeTime.TotalDays < 7 || options.RefreshTokenLifeTime.TotalSeconds > (180 * 86400))
		{
			sb.Append("The total time of refresh token must be in the range of 7 to 180 days. ")
				.Append("Actual: ").Append(options.RefreshTokenLifeTime.ToString())
				.AppendLine();
		}

		if (options.ClockSkew.TotalSeconds > 180)
		{
			sb.Append("The total time of clock skew must not exceed 3 minutes. ")
				.Append("Actual: ").Append(options.ClockSkew.ToString())
				.AppendLine();
		}
		
		return sb.Length != 0
			? ValidateOptionsResult.Fail(sb.ToString())
			: ValidateOptionsResult.Success;
	}
}