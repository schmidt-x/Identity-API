namespace IdentityApi.Contracts.Requests;

public class TokenRefreshingRequest
{
	public string AccessToken { get; set; } = default!;
	public string RefreshToken { get; set; } = default!;
}