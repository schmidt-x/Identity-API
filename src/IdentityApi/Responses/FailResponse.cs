namespace IdentityApi.Responses;

public class FailResponse
{
	public Dictionary<string, IEnumerable<string>> Errors { get; set; }
}