using System.Collections.Generic;

namespace IdentityApi.Results;

public class ValidationResult
{
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>> Errors { get; set; } = default!;
	
	
	public static ValidationResult Fail(string key, params string[] errors) =>
		new ValidationResult { Succeeded = false, Errors = new() { { key, errors } } };
	
	public static ValidationResult Success() => 
		new ValidationResult { Succeeded = true };
}