using System.Collections.Generic;

namespace IdentityApi.Results;

public class Result<T>
{
	public T Subject { get; set; } = default!;
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>>? Errors { get; set; }
}