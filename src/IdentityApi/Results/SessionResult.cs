using System.Collections.Generic;

namespace IdentityApi.Results;

public class SessionResult
{
	public string Id { get; set; }
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>> Errors { get; set; }
}