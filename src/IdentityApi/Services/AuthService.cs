using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityApi.Contracts.DTOs;
using IdentityApi.Contracts.Options;
using IdentityApi.Data.Repositories;
using IdentityApi.Extensions;
using IdentityApi.Models;
using IdentityApi.Results;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Bcrypt = BCrypt.Net.BCrypt;

namespace IdentityApi.Services;

public class AuthService : IAuthService
{
	private readonly IUserRepository _userRepo;
	private readonly IRefreshTokenRepository _tokenRepo;
	private readonly IMemoryCache _userSession;
	private readonly JwtOptions _jwt;
	private readonly EmailOptions _email;

	public AuthService(
		IUserRepository userRepo, 
		IRefreshTokenRepository tokenRepo,
		IMemoryCache userSession,
		IOptions<JwtOptions> jwtOptions,
		IOptions<EmailOptions> emailOptions)
	{
		_userRepo = userRepo;
		_tokenRepo = tokenRepo;
		_userSession = userSession;
		_jwt = jwtOptions.Value;
		_email = emailOptions.Value;
	}
	
	
	public async Task<SessionResult> CreateSessionAsync(string email, CancellationToken ct)
	{
		if (await _userRepo.EmailExistsAsync(email, ct))
			return new SessionResult { Errors = new()
			{
				{ "Email", new[] { $"Email address '{email}' is already taken" } }
			}};
		
		string verificationCode = GenerateVerificationCode();
		
		// SendEmail(email, verificationCode);
		Console.WriteLine(verificationCode);
		
		var sessionId = Guid.NewGuid().ToString();
		var session = new UserSession
		{
			EmailAddress = email,
			VerificationCode = verificationCode,
		};
		
		_userSession.Set(sessionId, session, TimeSpan.FromMinutes(5));
		
		return new SessionResult
		{
			Succeeded = true,
			Id = sessionId
		};
	}
	
	public SessionResult VerifyEmail(string sessionId, string verificationCode)
	{
		string key = "session";
		string[]? error = null;
		
		if (!_userSession.TryGetValue<UserSession>(sessionId, out var session))
		{
			error = new[] { "No session was found" };
		}
		else if (session!.IsVerified)
		{
			error = new[] { "Email has already been verified" };
		} 
		else if (session.Attempts >= 3)
		{
			error = new[] { "No attmempts are left" };
		}
		else if (session.VerificationCode != verificationCode)
		{
			key = "code";
			var attempts = 3 - (++session.Attempts);
			
			var errorMessage = (attempts) switch
			{
				1 => "1 last attempt is left",
				2 => "2 more attempts are left",
				_ => "No attempts are left"
			};
			
			error = new[] { "Wrong verification code", errorMessage };
		}
		
		if (error != null)
		{
			return new SessionResult { Errors = new() { { key, error } }};
		}
		
		session!.IsVerified = true;
		return new SessionResult { Succeeded = true };
	}
	
	public async Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegistration userRegistration, CancellationToken ct)
	{
		if (!_userSession.TryGetValue<UserSession>(sessionId, out var session))
		{
			return AuthenticationResultFail("Session", "No session was found");
		}
		
		if (session!.IsVerified == false)
		{
			return AuthenticationResultFail("Email", "Email address is not verified");
		}
		
		var userResult = await _userRepo.UserExistsAsync(session.EmailAddress, userRegistration.Username, ct); // TODO
		
		if (userResult.Exists)
		{
			return AuthenticationResultFail(userResult.Errors);
		}
		
		var timeNow = DateTime.UtcNow;
		var user = new User
		{
			Id = Guid.NewGuid(),
			Username = userRegistration.Username,
			Password = HashValue(userRegistration.Password),
			CreatedAt = timeNow,
			UpdatedAt = timeNow,
			Email = session.EmailAddress,
			Role = "user"
		};
		
		try
		{
			await _userRepo.SaveAsync(user, ct);
		}
		catch(SqlException ex) when (ex.Number == 2627)
		{
			return AuthenticationResultFail(GetSqlUQConstraintMessage(ex));
		}
		
		return AuthenticationResultSuccess(user.Id, user.Email);
	}
	
	public async Task<TokenGenerationResult> GenerateTokensAsync(UserClaims user, CancellationToken ct)
	{
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		var handler = new JwtSecurityTokenHandler();
		
		var identity = new ClaimsIdentity(new[]
		{
			new Claim("jti", Guid.NewGuid().ToString()),
			new Claim("id", user.Id.ToString()),
			new Claim("email", user.Email)
		}, "Bearer");
		
		var descriptor = new SecurityTokenDescriptor
		{
			Subject = identity,
			Audience = _jwt.Audience,
			Issuer = _jwt.Issuer,
			Expires = DateTime.UtcNow.AddMinutes(5),
			SigningCredentials = credentials
		};
		
		var securityToken = handler.CreateToken(descriptor);
		
		var timeNow = DateTimeOffset.UtcNow;
		var refreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			Jti = securityToken.Id,
			CreatedAt = timeNow.ToUnixTimeSeconds(),
			ExpiresAt = timeNow.AddMonths(6).ToUnixTimeMilliseconds(),
			UserId = user.Id,
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
		var user = await _userRepo.GetByEmailAsync(userLogin.Email, ct);
		
		if (user == null || !VerifyHashedValue(userLogin.Password, user.Password))
		{
			return AuthenticationResultFail("User", "Incorrect login/password");
		}
		
		return AuthenticationResultSuccess(user.Id, user.Email);
	}
	
	public async Task<AuthenticationResult> ValidateTokensAsync(TokenRefreshing tokens, CancellationToken ct)
	{
		if (!Guid.TryParse(tokens.RefreshToken, out var refreshToken))
		{
			return AuthenticationResultFail("RefreshToken", "Invalid refresh token");
		}
		
		var tokenHandler = new JwtSecurityTokenHandler();
		
		var parameters = new TokenValidationParameters // TODO 
		{
			ValidIssuer = _jwt.Issuer,
			ValidAudience = _jwt.Audience,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey)),
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateIssuerSigningKey = true,
			ValidateLifetime = false
		};
		
		if (!tokenHandler.TryValidate(tokens.AccessToken, parameters, out var validatedToken))
		{
			return AuthenticationResultFail("AccessToken", "Invalid access token");
		}
		
		var storedRefreshToken = await _tokenRepo.GetAsync(refreshToken, ct);
		
		if (storedRefreshToken == null)
		{
			return AuthenticationResultFail("RefreshToken", "Refresh token does not exist");
		}
		
		if (storedRefreshToken.Invalidated)
		{
			return AuthenticationResultFail("RefreshToken", "Refresh token is invalidated");
		}
		
		if (storedRefreshToken.Used)
		{
			return AuthenticationResultFail("RefreshToken", "Refresh token has already been used");
		}
		
		if (storedRefreshToken.ExpiresAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
		{
			return AuthenticationResultFail("RefreshToken", "Refresh token has been expired");
		}
		
		if (storedRefreshToken.Jti != validatedToken!.Id)
		{
			return AuthenticationResultFail("AccessToken", "Tokens do not match");
		}
		
		await _tokenRepo.SetUsedAsync(refreshToken, ct);
		
		var user = await _userRepo.GetClaims(storedRefreshToken.UserId, ct);
		// TODO Should I handle user == null? Is it possible?
		
		return AuthenticationResultSuccess(user!);
	}
	
	
	private static string GenerateVerificationCode()
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		
		return new string(Enumerable
			.Repeat(chars, 6)
			.Select(x => x[Random.Shared.Next(x.Length)])
			.ToArray());
	}
	
	private void SendEmail(string emailTo, string message)
	{
		using var emailMessage = new MailMessage
		{
			From = new(_email.Address, "IdentityApi"),
			To = { new(emailTo) },
			Subject = "Email verification",
			Body = message,
		};
		
		var smtpClient = new SmtpClient("smtp.gmail.com", 587)
		{
			EnableSsl = true,
			DeliveryMethod = SmtpDeliveryMethod.Network,
			UseDefaultCredentials = false,
			Credentials = new NetworkCredential(emailMessage.From.Address, _email.Password)
		};
		
		smtpClient.Send(emailMessage);
	}
	
	private static string HashValue(string value)
	{
		var salt = Bcrypt.GenerateSalt();
		return Bcrypt.HashPassword(value, salt);
	}
	
	private static bool VerifyHashedValue(string value, string hashedValue)
	{
		return Bcrypt.Verify(value, hashedValue);
	}
	
	private static Dictionary<string, IEnumerable<string>> GetSqlUQConstraintMessage(SqlException ex)
	{
		var message = ex.Message;
		const int startIndex = 36;
		var endIndex = message.IndexOf('.') - 1;
		
		var constraint = message.Substring(startIndex, endIndex - startIndex);
		
		// constraint name template is 'Constraint_Table_column'
		// for example 'UQ_User_email', 'UQ_User_username', etc.
		var parts = constraint.Split('_');
		var key = parts[2];
		
		if (key == "id")
			throw new ArgumentException($"Collision of guid. Constraint: '{constraint}'");
		
		var value = key switch
		{
			"email" => "Email address is already taken",
			"username" => "Username is already taken",
			_ => string.Empty, // TODO What should I do here?
		};
		
		return new() { { key, new[] { value } } };
	}
	
	
	private static AuthenticationResult AuthenticationResultFail(string key, params string[] errors)
	{
		return new AuthenticationResult 
		{ 
			Succeeded = false,
			Errors = new() { { key, errors } } 
		};
	}
	
	private static AuthenticationResult AuthenticationResultFail(Dictionary<string, IEnumerable<string>> errors)
	{
		return new AuthenticationResult { Errors = errors };
	}
	
	private static AuthenticationResult AuthenticationResultSuccess(Guid userId, string email)
	{
		return new AuthenticationResult
		{
			Succeeded = true,
			User = new()
			{
				Id = userId,
				Email = email
			}
		};
	}
	
	private static AuthenticationResult AuthenticationResultSuccess(UserClaims user)
	{
		return new AuthenticationResult
		{
			Succeeded = true,
			User = user,
		};
	}
}