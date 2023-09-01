using System.Collections.Generic;

namespace IdentityApi.Results;

public class SessionResult
{
	public string Id { get; set; } = default!;
	public string VerificationCode { get; set; } = default!;
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>>? Errors { get; set; }
}