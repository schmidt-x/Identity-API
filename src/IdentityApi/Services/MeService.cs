﻿using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Data.Repositories;
using IdentityApi.Models;
using IdentityApi.Responses;
using IdentityApi.Results;

namespace IdentityApi.Services;

public class MeService : IMeService
{
	private readonly IUserRepository _userRepo;
	private readonly IUserContext _userCtx;
	private readonly IPasswordService _passwordService;
	private readonly ICodeGenerationService _codeService;
	private readonly ICacheService _cacheService;

	public MeService(
		IUserRepository userRepo, 
		IUserContext userCtx, 
		IPasswordService passwordService,
		ICodeGenerationService codeService, 
		ICacheService cacheService)
	{
		_userRepo = userRepo;
		_userCtx = userCtx;
		_passwordService = passwordService;
		_codeService = codeService;
		_cacheService = cacheService;
	}
	
	
	public async Task<Me> GetAsync(CancellationToken ct)
	{
		var userId = _userCtx.GetId();
		var profile = await _userRepo.GetProfileAsync(userId, ct);
		
		return new Me
		{
			Username = profile.Username,
			Email = profile.Email,
			CreatedAt = profile.CreatedAt,
			UpdatedAt = profile.UpdatedAt,
			Role = profile.Role,
			Token = _userCtx.GetToken()
		};
	}

	public async Task<Result<Me>> UpdateUsernameAsync(UsernameUpdate update, CancellationToken ct)
	{
		var id = _userCtx.GetId();
		var user = (await _userRepo.GetAsync(id, ct))!;
		
		if (update.Username == user.Username)
		{
			return ResultFail<Me>("username", "Username cannot be the same");
		}
		
		if (!_passwordService.VerifyPassword(update.Password, user.PasswordHash))
		{
			return ResultFail<Me>("password", "Password is not correct");
		}
		
		var profile = await _userRepo.UpdateUsernameAsync(id, update.Username, ct);
		
		var me = new Me
		{
			Username = profile.Username,
			Email = profile.Email,
			CreatedAt = profile.CreatedAt,
			UpdatedAt = profile.UpdatedAt,
			Role = profile.Role,
			Token = _userCtx.GetToken()
		};
		
		return ResultSuccess(me);
	}

	public string CreateEmailUpdateSession()
	{
		var verificationCode = _codeService.Generate();
		
		var session = new UserSession
		{
			EmailAddress = null!, // it's for new email, we don't need it yet
			VerificationCode = verificationCode,
			IsVerified = false,
			Attempts = 0
		};
		
		var userId = _userCtx.GetId().ToString();
		_cacheService.Set(userId, session, TimeSpan.FromMinutes(5));
		
		return verificationCode;
	}
	
	public ResultEmpty VerifyOldEmail(string verificationCode)
	{
		var userId = _userCtx.GetId().ToString();
		
		if (!_cacheService.TryGetValue<UserSession>(userId, out var session))
		{
			return ResultEmptyFail("session", "No session was found");
		}
		
		if (session!.IsVerified)
		{
			return ResultEmptyFail("session", "Old email address is already verified");
		}
		
		if (session.VerificationCode != verificationCode)
		{
			var attempts = ++session.Attempts;
			var result = ResultEmptyFail("code", AttemptsErrors(attempts));
			
			if (attempts >= 3)
			{
				_cacheService.Remove(userId);
			}
			
			return result;
		}
		
		session.IsVerified = true;
		// refresh the life-time
		_cacheService.Refresh(userId, session, TimeSpan.FromMinutes(5));
		
		return new ResultEmpty { Succeeded = true };
	}

	public async Task<Result<string>> CacheNewEmailAsync(string newEmail, CancellationToken ct)
	{
		var userId = _userCtx.GetId().ToString();
		
		if (!_cacheService.TryGetValue<UserSession>(userId, out var session))
		{
			return ResultFail<string>("session", "No session was found");
		}
		
		if (session!.IsVerified == false) // checks if old email is verified
		{
			return ResultFail<string>("email", "Old email address is not verified");
		}
		
		if (await _userRepo.EmailExistsAsync(newEmail, ct))
		{
			return ResultFail<string>("email", $"Email address '{newEmail}' is already taken");
		}
		
		var verificationCode = _codeService.Generate();
		
		session.EmailAddress = newEmail;
		session.VerificationCode = verificationCode;
		session.Attempts = 0;
		
		_cacheService.Refresh(userId, session, TimeSpan.FromMinutes(5));
		
		return ResultSuccess(verificationCode);
	}

	public async Task<Result<Me>> UpdateEmailAsync(string verificationCode, CancellationToken ct)
	{
		var userId = _userCtx.GetId();
		var userIdAsString = userId.ToString();
		
		if (!_cacheService.TryGetValue<UserSession>(userIdAsString, out var session))
		{
			return ResultFail<Me>("session", "No session was found");
		}
		
		if (session!.IsVerified == false) // checks if old email is verified
		{
			return ResultFail<Me>("email", "Old email address is not verified");
		}
		
		if (session.VerificationCode != verificationCode)
		{
			var attempts = ++session.Attempts;
			var result = ResultFail<Me>("code", AttemptsErrors(attempts));
			
			if (attempts >= 3)
			{
				_cacheService.Remove(userIdAsString);
			}
			
			return result;
		}
		
		var profile = await _userRepo.UpdateEmailAsync(userId, session.EmailAddress, ct);
		
		var me = new Me
		{
			Username = profile.Username,
			Email = profile.Email,
			CreatedAt = profile.CreatedAt,
			UpdatedAt = profile.UpdatedAt,
			Role = profile.Role,
			Token = _userCtx.GetToken()
		};
		
		_cacheService.Remove(userIdAsString);
		
		return ResultSuccess(me);
	}
	
	
	private static string[] AttemptsErrors(int attempts)
	{
		var leftAttempts = 3 - attempts;
		
		var errorMessage = (leftAttempts) switch
		{
			1 => "1 last attempt is left",
			2 => "2 more attempts are left",
			_ => "No attempts are left"
		};
		
		return new[] { "Wrong verification code", errorMessage };
	}
	
	private static Result<T> ResultFail<T>(string key, params string[] errors) =>
		new() { Errors = new() {{ key, errors }}, Succeeded = false };
		
	private static Result<T> ResultSuccess<T>(T value) =>
		new() { Value = value , Succeeded = true };
		
	private static ResultEmpty ResultEmptyFail(string key, params string[] errors) =>
		new() { Errors = new() { { key, errors } } };
	
}