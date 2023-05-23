using System.Collections.Generic;
using IdentityApi.Models;

namespace IdentityApi.Results;

public class AuthenticationResult
{
	public UserClaims User { get; set; } = default!;
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>>? Errors { get; set; }
}