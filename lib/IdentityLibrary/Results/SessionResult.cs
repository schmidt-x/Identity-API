namespace IdentityLibrary.Results;

public class SessionResult
{
	public string Id { get; set; }
	public bool Success { get; set; }
	public Dictionary<string, IEnumerable<string>> Errors { get; set; }
}