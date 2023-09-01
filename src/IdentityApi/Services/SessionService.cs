using System;
using IdentityApi.Domain.Constants;
using IdentityApi.Domain.Models;
using IdentityApi.Results;
using Microsoft.Extensions.Caching.Memory;

namespace IdentityApi.Services;

public class SessionService : ISessionService
{
	private readonly IMemoryCache _memoryCache;
	private const int _maxAttempts = 3;

	public SessionService(IMemoryCache memoryCache)
	{
		_memoryCache = memoryCache;
	}
	
	public Session Create(object key, Session value, TimeSpan absoluteExpirationRelativeToNow)
	{
		return _memoryCache.Set(key, value, absoluteExpirationRelativeToNow);
	}

	public bool TryGetValue<T>(object key, out T? session)
	{
		if (_memoryCache.TryGetValue<Session>(key, out var baseSession) && baseSession is T TSession)
		{
			session = TSession;
			return true;
		}
		
		session = default;
		return false;
	}
	
	public void Remove(object key)
	{
		_memoryCache.Remove(key);
	}
	
	public Session Update(object key, Session value, TimeSpan absoluteExpirationRelativeToNow)
	{
		return Create(key, value, absoluteExpirationRelativeToNow);
	}
	
	public ResultEmpty VerifySession(string sessionId, string verificationCode, bool removeIfAttemptsExceeded = true)
	{
		if (!_memoryCache.TryGetValue<Session>(sessionId, out var session))
		{
			return ResultEmptyFail(ErrorKey.Session, ErrorMessage.SessionNotFound);
		}
		
		if (session!.IsVerified)
		{
			return ResultEmptyFail(ErrorKey.Session, "Session is already verified"); // TODO
		}
		
		if (session.VerificationCode != verificationCode)
		{
			var attempts = ++session.Attempts;
			var result = ResultEmptyFail(ErrorKey.Code, GetAttemptErrors(attempts));
			
			if (removeIfAttemptsExceeded && attempts >= _maxAttempts)
				_memoryCache.Remove(sessionId);
			
			return result;
		}
		
		session.IsVerified = true;
		
		// refresh the life-time
		_memoryCache.Set(sessionId, session, TimeSpan.FromMinutes(5));
		
		return ResultEmptySuccess();
	}
	
	public string[] GetAttemptErrors(int attempts)
	{
		var attemptsLeft = _maxAttempts - attempts;
		
		var error = (attemptsLeft) switch
		{
			1 => "1 last attempt left",
			> 1 => $"{attemptsLeft} more attempts left",
			_ => "No attempts left"
		};
		
		return new[] { "Wrong verification code", error };
	}
	
	public void RemoveIfExceeded(int attempts, string sessionId)
	{
		if (attempts >= _maxAttempts)
			_memoryCache.Remove(sessionId);
	}
	
	
	private static ResultEmpty ResultEmptyFail(string key, params string[] errors) =>
		new() { Errors = new() { { key, errors } } };
	
	private static ResultEmpty ResultEmptySuccess() => new() { Succeeded = true };
	
}
