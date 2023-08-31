using System;
using System.Collections.Generic;

namespace IdentityApi.Services;

public interface ITokenBlacklist
{
	void Add(string token, TimeSpan expirationTime);
	void AddRange(IEnumerable<string> tokens, TimeSpan expirationTime = default);
	bool Contains(string token);
}