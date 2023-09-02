using System.Collections.Generic;

namespace IdentityApi.Results;

public class SessionResult
{
	public string Id { get; set; } = default!;
	public string VerificationCode { get; set; } = default!;
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>>? Errors { get; set; }
	
	
	public static SessionResult Success(string id, string verificationCode) =>
		new SessionResult { Succeeded = true, Id = id, VerificationCode = verificationCode };
	
	public static SessionResult Fail(string key, params string[] errors) =>
		new SessionResult { Succeeded = false, Errors = new() { { key, errors } } };
}