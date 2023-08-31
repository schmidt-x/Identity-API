namespace IdentityApi.Results;

public class TokensResult
{
	public string AccessToken { get; set; } = default!;
	public string RefreshToken { get; set; } = default!;
}