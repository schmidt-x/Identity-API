using System.Text.Json.Serialization;

namespace IdentityApi.Contracts.Requests;

public class TokenRefreshingRequest
{
	[JsonPropertyName("access_token")]
	public string AccessToken { get; set; } = default!;
	
	[JsonPropertyName("refresh_token")]
	public string RefreshToken { get; set; } = default!;
}