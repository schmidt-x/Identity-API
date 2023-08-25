using System;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityApi.Services;

public class SessionService : ISessionService
{
	private readonly IMemoryCache _memoryCache;

	public SessionService(IMemoryCache memoryCache)
	{
		_memoryCache = memoryCache;
	}
	
	public T Create<T>(object key, T value, TimeSpan absoluteExpirationRelativeToNow)
	{
		return _memoryCache.Set(key, value, absoluteExpirationRelativeToNow);
	}

	public bool TryGetValue<T>(object key, out T? value)
	{
		return _memoryCache.TryGetValue(key, out value);
	}
	
	public void Remove(object key)
	{
		_memoryCache.Remove(key);
	}
	
	public T Update<T>(object key, T value, TimeSpan absoluteExpirationRelativeToNow)
	{
		return Create(key, value, absoluteExpirationRelativeToNow);
	}
}
