using System.Collections.Generic;

namespace IdentityApi.Results;

public class Result<T>
{
	public T Value { get; set; } = default!;
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>>? Errors { get; set; }
	
	
	public static Result<T> Success(T value) =>
		new Result<T> { Succeeded = true, Value = value };
	
	public static Result<T> Fail(string key, params string[] errors) =>
		new Result<T> { Succeeded = false, Errors = new() { { key, errors } } };
}