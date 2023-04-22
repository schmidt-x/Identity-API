namespace IdentityLibrary.Results;

public class TokenGenerationResult
{
	public string AccessToken { get; set; }
	public Guid RefreshToken { get; set; }
}