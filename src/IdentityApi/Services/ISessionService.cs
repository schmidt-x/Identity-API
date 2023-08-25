using System;

namespace IdentityApi.Services;

public interface ISessionService
{
	T Create<T>(object key, T value, TimeSpan absoluteExpirationRelativeToNow);
	bool TryGetValue<T>(object key, out T? value);
	void Remove(object key);
	T Update<T>(object key, T value, TimeSpan absoluteExpirationRelativeToNow);
}