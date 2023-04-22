namespace IdentityLibrary.Results;

public class AuthenticationResult
{
	public Guid UserId { get; set; }
	public bool Success { get; set; }
	public Dictionary<string, string[]> Errors { get; set; }
}