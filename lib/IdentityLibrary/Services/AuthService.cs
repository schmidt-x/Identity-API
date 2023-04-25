using Microsoft.Extensions.Caching.Memory;

namespace IdentityLibrary.Services;

public class AuthService : IAuthService
{
	private readonly IUserRepository _userRepo;
	private readonly IRefreshTokenRepository _tokenRepo;
	private readonly JwtConfig _jwtConfig;
	private readonly EmailConfig _emailConfig;
	private readonly IMemoryCache _userSession;

	public AuthService(
		IUserRepository userRepo, 
		IRefreshTokenRepository tokenRepo,
		JwtConfig jwtConfig,
		IMemoryCache userSession,
		EmailConfig emailConfig)
	{
		_userRepo = userRepo;
		_tokenRepo = tokenRepo;
		_jwtConfig = jwtConfig;
		_userSession = userSession;
		_emailConfig = emailConfig;
	}
	
	
	public async Task<SessionResult> CreateSessionAsync(string email)
	{
		if (await _userRepo.EmailExistsAsync(email))
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
	
	public async Task<AuthenticationResult> RegisterAsync(string sessionId, UserRegistration userRegistration)
	{
		var key = "";
		string[]? errors = null;
		
		if (!_userSession.TryGetValue<UserSession>(sessionId, out var session))
		{
			key = "session";
			errors = new[] { "No session is found" };
		}
		else if (session!.IsVerified == false)
		{
			key = "email";
			errors = new[] { "Email address is not verified" };
		}
		
		if (errors != null)
		{
			return new AuthenticationResult { Errors = new() { { key, errors } } };
		}
		
		var userResult = await _userRepo.UserExistsAsync(session!.EmailAddress, userRegistration.Username);
		
		if (userResult.Exists)
		{
			return new AuthenticationResult { Errors = userResult.Errors };
		}
		
		var user = new User
		{
			Id = Guid.NewGuid(),
			Username = userRegistration.Username,
			Password = HashValue(userRegistration.Password),
			CreatedAt = DateTime.UtcNow,
			// LastUpdatedAt = DateTime.UtcNow, // TODO
			Email = session.EmailAddress,
			Role = "user"
		};
		
		try
		{
			await _userRepo.SaveAsync(user);
		}
		catch(SqlException ex) when (ex.Number == 2627)
		{
			// TODO to log
			return new AuthenticationResult { Errors = GetSqlUQConstraintMessage(ex) };
		}
		
		return new AuthenticationResult { Succeeded = true, UserId = user.Id };
	}
	
	public async Task<TokenGenerationResult> GenerateTokensAsync(Guid userId)
	{
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		var handler = new JwtSecurityTokenHandler();
		
		var jti = Guid.NewGuid();
		
		var identity = new ClaimsIdentity(new[]
		{
			new Claim("id", userId.ToString()),
			new Claim("jti", jti.ToString()),
		}, "Bearer");
		
		var descriptor = new SecurityTokenDescriptor
		{
			Subject = identity,
			Audience = _jwtConfig.Audience,
			Issuer = _jwtConfig.Issuer,
			Expires = DateTime.UtcNow.AddMinutes(5),
			SigningCredentials = credentials
		};
		
		var securityToken = handler.CreateToken(descriptor);
		
		var refreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			Jti = jti,
			CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			ExpiresAt = DateTimeOffset.UtcNow.AddMonths(6).ToUnixTimeMilliseconds(),
			UserId = userId,
		};
		
		await _tokenRepo.SaveAsync(refreshToken);
		
		return new TokenGenerationResult
		{
			AccessToken = handler.WriteToken(securityToken),
			RefreshToken = refreshToken.Id
		};
	}
	
	public async Task<AuthenticationResult> AuthenticateAsync(UserLogin userLogin)
	{
		var user = await _userRepo.GetByEmailAsync(userLogin.Email);
		
		if (user == null || !VerifyHashedValue(userLogin.Password, user.Password))
		{
			return new AuthenticationResult { Errors = new()
			{
				{ "user", new[] { "Incorrect login/password" } }
			}};
		}
		
		return new AuthenticationResult { Succeeded = true, UserId = user.Id };
	}
	
	
	private string GenerateVerificationCode()
	{
		var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		
		return new string(Enumerable
			.Repeat(chars, 6)
			.Select(x => x[Random.Shared.Next(x.Length)])
			.ToArray());
	}
	
	private void SendEmail(string emailTo, string message)
	{
		using var emailMessage = new MailMessage
		{
			From = new(_emailConfig.Address, "IdentityApi"),
			To = { new(emailTo) },
			Subject = "Email verification",
			Body = message,
		};
		
		var smtpClient = new SmtpClient("smtp.gmail.com", 587)
		{
			EnableSsl = true,
			DeliveryMethod = SmtpDeliveryMethod.Network,
			UseDefaultCredentials = false,
			Credentials = new NetworkCredential(emailMessage.From.Address, _emailConfig.Password)
		};
		
		smtpClient.Send(emailMessage);
	}
	
	private string HashValue(string value)
	{
		var salt = Bcrypt.GenerateSalt();
		return Bcrypt.HashPassword(value, salt);
	}
	
	private bool VerifyHashedValue(string value, string hashedValue)
	{
		return Bcrypt.Verify(value, hashedValue);
	}
	
	private Dictionary<string, IEnumerable<string>> GetSqlUQConstraintMessage(SqlException ex)
	{
		var message = ex.Message;
		var startIndex = 36;
		var endIndex = message.IndexOf('.') - 1;
		
		var constraint = message.Substring(startIndex, endIndex - startIndex);
		
		// constraint name template is 'Constraint_Table_Column'
		// for example 'UQ_User_email' or 'UQ_User_username'
		var parts = constraint.Split('_');
		var key = parts[2];
		
		if (key == "id")
			throw new ArgumentException($"Collision of guid. Constraint: ({constraint})");
		
		var value = key switch
		{
			"email" => "Email address is already taken",
			"username" => "Username is already taken",
			_ => string.Empty, // TODO What should I do here?
		};
		
		return new() { { key, new[] { value } } };
		
	}
}