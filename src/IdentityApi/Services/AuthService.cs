using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Contracts.Options;
using IdentityApi.Data.Repositories;
using IdentityApi.Models;
using IdentityApi.Results;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityApi.Services;

public class AuthService : IAuthService
{
	private readonly IUserRepository _userRepo;
	private readonly IRefreshTokenRepository _tokenRepo;
	private readonly ICacheService _cacheService;
	private readonly TokenValidationParameters _tokenValidationParameters;
	private readonly ICodeGenerationService _codeService;
	private readonly JwtOptions _jwt;
	private readonly IPasswordService _passwordService;

	public AuthService(
		IUserRepository userRepo, 
		IRefreshTokenRepository tokenRepo,
		ICacheService cacheService,
		IOptions<JwtOptions> jwtOptions,
		TokenValidationParameters tokenValidationParameters,
		ICodeGenerationService codeService,
		IPasswordService passwordService)
	{
		_userRepo = userRepo;
		_tokenRepo = tokenRepo;
		_cacheService = cacheService;
		_tokenValidationParameters = tokenValidationParameters;
		_codeService = codeService;
		_jwt = jwtOptions.Value;
		_passwordService = passwordService;
	}
	
	
	public async Task<SessionResult> CreateSessionAsync(string email, CancellationToken ct)
	{
		if (await _userRepo.EmailExistsAsync(email, ct))
		{
			return SessionResultFail("email", $"Email address '{email}' is already taken");
		}
		
		string verificationCode = _codeService.Generate();
		
		var sessionId = Guid.NewGuid().ToString();
		var session = new UserSession
		{
			EmailAddress = email,
			VerificationCode = verificationCode,
		};
		
		_cacheService.Set(sessionId, session, TimeSpan.FromMinutes(5));
		
		return new SessionResult
		{
			Succeeded = true,
			Id = sessionId,
			VerificationCode = verificationCode
		};
	}
	
	public ResultEmpty VerifyEmail(string sessionId, string verificationCode)
	{
		if (!_cacheService.TryGetValue<UserSession>(sessionId, out var session))
		{
			return ResultEmptyFail("session", "No session was found");
		}
		
		if (session!.IsVerified)
		{
			return ResultEmptyFail("session", "Email address is already verified");
		} 
		
		if (session.VerificationCode != verificationCode)
		{
			var attempts = ++session.Attempts;
			var result = ResultEmptyFail("code", AttemptsErrors(attempts));
			
			if (attempts >= 3)
			{
				_cacheService.Remove(sessionId);
			}
			
			return result;
		}
		
		session.IsVerified = true;
		_cacheService.Refresh(sessionId, session, TimeSpan.FromMinutes(5));
		
		return ResultEmptySuccess();
	}
	
	public async Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegistration userRegistration, CancellationToken ct)
	{
		if (!_cacheService.TryGetValue<UserSession>(sessionId, out var session))
		{
			return AuthResultFail("session", "No session was found");
		}
		
		if (session!.IsVerified == false)
		{
			return AuthResultFail("email", "Email address is not verified");
		}
		
		if (await _userRepo.UsernameExistsAsync(userRegistration.Username, ct))
		{
			return AuthResultFail("username", $"Username '{userRegistration.Username}' is already taken");
		}
		
		var timeNow = DateTime.UtcNow;
		var user = new User
		{
			Id = Guid.NewGuid(),
			Username = userRegistration.Username,
			PasswordHash = _passwordService.HashPassword(userRegistration.Password),
			CreatedAt = timeNow,
			UpdatedAt = timeNow,
			Email = session.EmailAddress,
			Role = "user"
		};
		
		try
		{
			await _userRepo.SaveAsync(user, ct);
		}
		catch(SqlException ex) when (ex.Number == 2627) // in case if 'Race condition' occurs
		{
			var result = GetSqlUQConstraintMessage(ex);

			result.error = result.key switch
			{
				// error has a placeholder 'Username/Email address '{0}' is already taken' 
				"username" => string.Format(result.error, user.Username),
				"email" => string.Format(result.error, user.Email),
				_ => result.error
			};
			
			// TODO if race condition occured and it's email,
			// invalidate the cache so the user would start new registration session
			
			return AuthResultFail(result.key, result.error);
		}
		
		_cacheService.Remove(sessionId);
		
		return AuthResultSuccess(user.Id, user.Email);
	}
	
	public async Task<TokenGenerationResult> GenerateTokensAsync(UserClaims user, CancellationToken ct)
	{
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		var handler = new JwtSecurityTokenHandler();
		
		var identity = new ClaimsIdentity(new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim(JwtRegisteredClaimNames.Email, user.Email)
		}, JwtBearerDefaults.AuthenticationScheme);
		
		var descriptor = new SecurityTokenDescriptor
		{
			Subject = identity,
			Audience = _jwt.Audience,
			Issuer = _jwt.Issuer,
			Expires = DateTime.UtcNow.Add(_jwt.AccessTokenLifeTime),
			SigningCredentials = credentials
		};
		
		var securityToken = handler.CreateToken(descriptor);
		
		var timeNow = DateTimeOffset.UtcNow;
		var refreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			Jti = securityToken.Id,
			CreatedAt = timeNow.ToUnixTimeSeconds(),
			ExpiresAt = timeNow.Add(_jwt.RefreshTokenLifeTime).ToUnixTimeSeconds(),
			UserId = user.Id,
			Invalidated = false,
			Used = false
		};
		
		await _tokenRepo.SaveAsync(refreshToken, ct);
		
		return new TokenGenerationResult
		{
			AccessToken = handler.WriteToken(securityToken),
			RefreshToken = refreshToken.Id
		};
	}
	
	public async Task<AuthenticationResult> AuthenticateAsync(UserLogin userLogin, CancellationToken ct)
	{
		var user = await _userRepo.GetAsync(userLogin.Login, ct);
		
		if (user == null || !_passwordService.VerifyPassword(userLogin.Password, user.PasswordHash))
		{
			return AuthResultFail("user", "Incorrect login/password");
		}
		
		return AuthResultSuccess(user.Id, user.Email);
	}
	
	public async Task<AuthenticationResult> ValidateTokensAsync(TokenRefreshing tokens, CancellationToken ct)
	{
		if (!Guid.TryParse(tokens.RefreshToken, out var refreshTokenId))
		{
			return AuthResultFail("refreshToken", "Invalid refresh token");
		}
		
		var userPrincipal = ValidateTokenExceptLifetime(tokens.AccessToken, out var securityToken);
		
		if (userPrincipal == null)
		{
			return AuthResultFail("accessToken", "Invalid access token");
		}
		
		var refreshToken = await _tokenRepo.GetAsync(refreshTokenId, ct);
		
		if (refreshToken == null)
		{
			return AuthResultFail("refreshToken", "Invalid refresh token");
		}
		
		if (refreshToken.Invalidated)
		{
			return AuthResultFail("refreshToken", "Refresh token is invalidated");
		}
		
		if (refreshToken.Used)
		{
			return AuthResultFail("refreshToken", "Refresh token has already been used");
		}
		
		if (refreshToken.ExpiresAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
		{
			return AuthResultFail("refreshToken", "Refresh token has been expired");
		}
		
		if (refreshToken.Jti != securityToken!.Id)
		{
			return AuthResultFail("accessToken", "Tokens do not match");
		}
		
		await _tokenRepo.SetUsedAsync(refreshTokenId, ct);
		var email = userPrincipal.FindFirstValue(ClaimTypes.Email)!;
		
		return AuthResultSuccess(refreshToken.UserId, email);
	}
	
	// Since the user's email address is stored in Jwt token, on 'ChangeEmail' action
	// we need to either re-issue new access token,
	// or just replace the claim value and recompute the signature.
	// This method does the second one
	public string UpdateAccessTokenEmail(string oldToken, string newEmail)
	{
		var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(oldToken);
		var jwtClaims = jwtSecurityToken.Payload;
		
		jwtClaims[JwtRegisteredClaimNames.Email] = newEmail;
		
		object secondsNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		jwtClaims[JwtRegisteredClaimNames.Iat] = secondsNow;
		jwtClaims[JwtRegisteredClaimNames.Nbf] = secondsNow;
		
		// I don't know how to name the part that consists of Header and Payload
		var newSomething = $"{jwtSecurityToken.EncodedHeader}.{jwtSecurityToken.EncodedPayload}";
		var jwtHeader = JwtHeader.Base64UrlDeserialize(oldToken.Split('.', 2)[0]);
		
		var newToken = newSomething + '.' + jwtHeader.Alg switch
		{
			"HS256" => ComputeHashHS256(_jwt.SecretKey, newSomething),
			_ => throw new NotImplementedException()
		};
		
		return newToken;
	}
	
	
	/// <summary>
	/// Computes the HMAC-SHA256 hash of the given value using the provided secret key
	/// </summary>
	/// <param name="secretKey">Secret key used for hashing algorithm</param>
	/// <param name="value">Value to hash</param>
	/// <returns>Hashed value encoded to Base64Url string</returns>
	private static string ComputeHashHS256(string secretKey, string value)
	{
		var sha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
		var newSignature = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
		
		return Base64UrlEncoder.Encode(newSignature);
	}
	
	private static (string key, string error) GetSqlUQConstraintMessage(SqlException ex)
	{
		var message = ex.Message;
		const int startIndex = 36;
		var endIndex = message.IndexOf('.') - 1;
		
		var constraint = message.Substring(startIndex, endIndex - startIndex);
		
		// constraint name template is 'Constraint_Table_column'
		// for example 'UQ_User_email' or 'UQ_User_username'
		var parts = constraint.Split('_');
		var key = parts[2];
		
		var error = key switch
		{
			"email" => "Email address '{0}' is already taken",
			"username" => "Username '{0}' is already taken",
			_ => throw new ArgumentException($"Unexpected column name: '{key}'") 
		};
		
		return (key, error);
	}
	
	private ClaimsPrincipal? ValidateTokenExceptLifetime(string token, out SecurityToken? securityToken)
	{
		var handler = new JwtSecurityTokenHandler();
		securityToken = default;
		
		try
		{
			_tokenValidationParameters.ValidateLifetime = false;
			var claimsPrincipal = handler.ValidateToken(token, _tokenValidationParameters, out var secToken);
			
			if (!IsValidSecurityAlgorithm(secToken))
				return null;
			
			securityToken = secToken;
			return claimsPrincipal;
		}
		catch
		{
			return null;
		}
		finally
		{
			_tokenValidationParameters.ValidateLifetime = true;
		}
	}
	
	private static bool IsValidSecurityAlgorithm(SecurityToken token)
	{
		return token is JwtSecurityToken jwtSecToken
			&& jwtSecToken.Header.Alg.Equals(
			SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
	}
	
	
	private static AuthenticationResult AuthResultFail(string key, params string[] errors) =>
		new() { Errors = new() { { key, errors } } };
	
	private static AuthenticationResult AuthResultSuccess(Guid userId, string email) =>
		new() { Succeeded = true, Claims = new() { Id = userId, Email = email } };
	
	private static ResultEmpty ResultEmptyFail(string key, params string[] errors) =>
		new() { Errors = new() { { key, errors } } };
	
	private static ResultEmpty ResultEmptySuccess() => new() { Succeeded = true };
		
	private static SessionResult SessionResultFail(string key, params string[] errors) =>
		new() { Errors = new() { { key, errors } } };
		
	private static string[] AttemptsErrors(int attempts)
	{
		var leftAttempts = 3 - attempts;
		
		var error = (leftAttempts) switch
		{
			1 => "1 last attempt is left",
			2 => "2 more attempts are left",
			_ => "No attempts are left"
		};
		
		return new[] { "Wrong verification code", error };
	}
}
