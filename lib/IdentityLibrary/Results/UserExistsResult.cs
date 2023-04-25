namespace IdentityLibrary.Results;

public class UserExistsResult
{
	public bool Exists { get; set; }
	public Dictionary<string, IEnumerable<string>> Errors { get; set; }
}