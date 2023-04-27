namespace IdentityLibrary.Results;

public class AuthenticationResult
{
	public UserClaims User { get; set; }
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>> Errors { get; set; }
}