namespace IdentityLibrary.Results;

public class TokenExtractionResult
{
	public bool Success { get; set; }
	public IEnumerable<string> Errors { get; set; }
	public string AccessToken { get; set; }
	public string RefreshToken { get; set; }
}