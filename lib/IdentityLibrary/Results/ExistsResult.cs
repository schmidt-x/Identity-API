namespace IdentityLibrary.Results;

public class ExistsResult
{
	public bool Exists { get; set; }
	public ICollection<string> Errors { get; set; }
}