namespace IdentityLibrary.Results;

public class AuthenticationResult
{
	public bool Success { get; set; }
	public IEnumerable<string> Errors { get; set; }
	public UserClaims UserClaims { get; set; }
}