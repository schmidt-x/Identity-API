namespace IdentityLibrary.Results;

public class ExistsResult
{
	public bool Exists { get; set; }
	public Dictionary<string, string> Errors { get; set; }
}