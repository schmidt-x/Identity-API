namespace IdentityLibrary.Results;

public class AuthenticationResult
{
	public bool Success { get; set; }
	public Dictionary<string, string> Errors { get; set; }
	public UserClaims UserClaims { get; set; }
}