using System.Collections.Generic;
using IdentityApi.Domain.Models;

namespace IdentityApi.Results;

public class AuthResult
{
	public UserClaims Claims { get; set; } = default!;
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>>? Errors { get; set; }
	
	
	public static AuthResult Fail(string key, params string[] errors) =>
		new AuthResult { Succeeded = false, Errors = new() { { key, errors } } };
	
	public static AuthResult Fail(Dictionary<string, IEnumerable<string>> errors) =>
		new AuthResult { Succeeded = false, Errors = errors };

	public static AuthResult Success(UserClaims claims) =>
		new AuthResult { Succeeded = true, Claims = claims };
	
}