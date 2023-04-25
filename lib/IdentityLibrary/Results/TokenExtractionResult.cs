namespace IdentityLibrary.Results;

public class TokenExtractionResult
{
	public bool Succeeded { get; set; }
	public Dictionary<string, string[]> Errors { get; set; }
	public string AccessToken { get; set; }
	public string RefreshToken { get; set; }
}