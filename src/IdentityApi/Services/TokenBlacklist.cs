using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityApi.Services;

public class TokenBlacklist : ITokenBlacklist
{
	private readonly IMemoryCache _cache;

	public TokenBlacklist(IMemoryCache cache)
	{
		_cache = cache;
	}


	public void Add(string token, TimeSpan expirationTime)
	{
		_cache.Set(token, true, expirationTime);
	}
	
	public void AddRange(IEnumerable<string> tokens, TimeSpan expirationTime = default)
	{
		foreach(var token in tokens)
			_cache.Set(token, expirationTime);
	}

	public bool Contains(string token)
	{
		return _cache.TryGetValue(token, out _);
	}
}