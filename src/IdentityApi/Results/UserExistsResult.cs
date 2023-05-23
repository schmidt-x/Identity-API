using System.Collections.Generic;

namespace IdentityApi.Results;

public class UserExistsResult
{
	public bool Exists { get; set; }
	public Dictionary<string, IEnumerable<string>>? Errors { get; set; }
}