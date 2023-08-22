using System.Collections.Generic;

namespace IdentityApi.Contracts.Responses;

public class FailResponse
{
	public Dictionary<string, IEnumerable<string>>? Errors { get; set; }
}