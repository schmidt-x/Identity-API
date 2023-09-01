using System;
using IdentityApi.Domain.Models;
using IdentityApi.Results;

namespace IdentityApi.Services;

public interface ISessionService
{
	Session Create(object key, Session value, TimeSpan absoluteExpirationRelativeToNow);
	Session Update(object key, Session value, TimeSpan absoluteExpirationRelativeToNow);
	
	bool TryGetValue<T>(object key, out T? value);
	void Remove(object key);
	
	ResultEmpty VerifySession(string sessionId, string verificationCode, bool removeIfAttemptsExceeded = true);
	string[] GetAttemptErrors(int attempts);
	void RemoveIfExceeded(int attempts, string sessionId);
}