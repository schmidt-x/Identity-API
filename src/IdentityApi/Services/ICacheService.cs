using System;

namespace IdentityApi.Services;

public interface ICacheService
{
	T Set<T>(object key, T value, TimeSpan absoluteExpirationRelativeToNow);
	bool TryGetValue<T>(object key, out T? value);
	void Remove(object key);
}