namespace IdentityApi.Responses;

public class AuthResponse
{
	public string? Message { get; set; }
	public int StatusCode { get; set; }
	public Dictionary<string, string[]>? Errors { get; set; }
	public string? AccessToken { get; set; }
}