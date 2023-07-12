using System.Collections.Generic;

namespace IdentityApi.Results;

public class ErrorsResult
{
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>> Errors { get; set; } = default!;
}