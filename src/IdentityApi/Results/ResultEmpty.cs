using System.Collections.Generic;

namespace IdentityApi.Results;

public class ResultEmpty
{
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>> Errors { get; set; } = default!;
}