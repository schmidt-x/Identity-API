﻿namespace IdentityLibrary.Results;

public class AuthenticationResult
{
	public Guid UserId { get; set; }
	public bool Succeeded { get; set; }
	public Dictionary<string, IEnumerable<string>> Errors { get; set; }
}