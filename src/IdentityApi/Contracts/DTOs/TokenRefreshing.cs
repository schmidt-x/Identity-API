namespace IdentityApi.Contracts.DTOs;

public class TokenRefreshing
{
	public string AccessToken { get; set; } = default!;
	public string RefreshToken { get; set; } = default!;
}