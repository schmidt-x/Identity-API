using System;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityApi.Services;

public class CacheService : ICacheService
{
	private readonly IMemoryCache _memoryCache;

	public CacheService(IMemoryCache memoryCache)
	{
		_memoryCache = memoryCache;
	}
	
	public T Set<T>(object key, T value, TimeSpan absoluteExpirationRelativeToNow)
	{
		return _memoryCache.Set(key, value, absoluteExpirationRelativeToNow);
	}

	public bool TryGetValue<T>(object key, out T? value)
	{
		return _memoryCache.TryGetValue(key, out value);
	}
}