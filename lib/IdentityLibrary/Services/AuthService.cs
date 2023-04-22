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
		// if (await _userRepo.EmailExistsAsync(email))
			// return new SessionResult { Errors = new()
			// {
				// { "Email", new[] { $"This '{email}' email address is already in use" } }
			// }};
		
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
			Success = true,
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
			error = new[] { "No attmempts left" };
		}
		else if (session.VerificationCode != verificationCode)
		{
			key = "code";
			var attempts = 3 - (++session.Attempts);
			
			var errorMessage = (attempts) switch
			{
				1 => "One last attempt left",
				2 => "Two more attempts left",
				_ => "No attempts left"
			};
			
			error = new[] { "Wrong verification code", errorMessage };
		}
		
		if (error != null)
		{
			return new SessionResult { Errors = new() { { key, error } }};
		}
		
		session!.IsVerified = true;
		return new SessionResult { Success = true };
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
	
	// public void SetHttpSecureCookie(string key, string value, DateTimeOffset expires)
	// {
	// 	var cookieOptions = new CookieOptions
	// 	{
	// 		HttpOnly = true,
	// 		Secure = true,
	// 		Expires = expires
	// 	};
	// 	
	// 	_context.Response.Cookies.Append(key, value, cookieOptions);
	// }
	//
	// public SessionResult VerifyCode(string verificationCode)
	// {
	// 	var key = "sessionId";
	// 	string? error;
	// 	 
	// 	if (!_context.Request.Cookies.TryGetValue("session_id", out var rawSessionId))
	// 	{
	// 		error = "Session ID is required";
	// 	}
	// 	else if (!Guid.TryParse(rawSessionId, out var sessionId))
	// 	{
	// 		error = "Invalid Session ID";
	// 	}
	// 	else if (!_userSessions.TryGetValue(sessionId, out var session))
	// 	{
	// 		error = "No Session was found";
	// 	}
	// 	else if (session.VerificationCode != verificationCode)
	// 	{
	// 		key = "code";
	// 		error = "Wrong verification code";
	// 	}
	// 	else
	// 	{
	// 		session.IsVerified = true;
	// 		return new SessionResult { Success = true };
	// 	}
	// 	
	// 	return new SessionResult { Errors = new() { { key, new[] { error } } } };
	// }
	//
	// public async Task<AuthenticationResult> RegisterAsync(UserRegister userRegister)
	// {
	// 	// TODO make a method that extracts and validates the session
	// 	
	// 	var sessionResult = GetSession();
	// 	
	// 	if (!sessionResult.Success)
	// 		return new AuthenticationResult { Errors = sessionResult.Errors };
	// 	
	// 	if (!sessionResult.Session.IsVerified)
	// 		return new AuthenticationResult { Errors = new() { { "Email", new[] { "Email address is not verified" } } } };
	// 	
	// 	if (await _userRepo.UsernameExistsAsync(userRegister.Username))
	// 		return new AuthenticationResult { Errors = new()
	// 		{
	// 			{ "username", new[] { $"Username '{userRegister.Username}' is already taken" } }
	// 		}};
	// 	
	// 	
	// 	var user = new User
	// 	{
	// 		Id = Guid.NewGuid(),
	// 		Username = userRegister.Username,
	// 		Email = sessionResult.Session.EmailAddress,
	// 		Password = HashValue(userRegister.Password),
	// 		Role = "user",
	// 		CreatedAt = DateTime.UtcNow
	// 	};
	// 	
	// 	try
	// 	{
	// 		await _userRepo.SaveAsync(user);
	// 	}
	// 	catch(SqlException ex) when (ex.Number == 2627)
	// 	{
	// 		var sqlResult = GetSqlUQConstraint(ex);
	// 		
	// 		return new AuthenticationResult { Errors = new()
	// 		{
	// 			{ sqlResult.Column, new[] { sqlResult.ErrorMessage } }
	// 		}}; 
	// 	}
	// 	
	// 	return new AuthenticationResult
	// 	{
	// 		Success = true,
	// 		UserId = user.Id
	// 	};
	// }
	//
	// public async Task<AuthenticationResult> AuthenticateAsync(UserLogin userRegister)
	// {
	// 	var user = await _userRepo.GetAsync(userRegister.Username);
	// 	
	// 	if (user == null)
	// 		return new AuthenticationResult { Errors = new()
	// 		{
	// 			{ "user", new[] { "Invalid Login/Password" } }
	// 		}};
	// 	
	// 	var isCorrect = ValidateHashedValue(userRegister.Password, user.Password);
	// 	
	// 	if (!isCorrect)
	// 		return new AuthenticationResult { Errors = new()
	// 		{
	// 			{ "user", new[] { "Invalid Login/Password" } } 
	// 		}};
	// 	
	// 	return new()
	// 	{
	// 		Success = true,
	// 		UserId = user.Id
	// 	};
	// }
	//
	// public async Task<TokenGenerationResult> GenerateTokensAsync(Guid userId)
	// {
	// 	var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey));
	// 	var credentiasl = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
	// 	var jwtHandler = new JwtSecurityTokenHandler(); 
	// 	
	// 	var jti = Guid.NewGuid();
	// 	
	// 	var identity = new ClaimsIdentity(new[]
	// 	{
	// 		new Claim("id", userId.ToString()),
	// 		new Claim("jti", jti.ToString())
	// 	}, "Bearer");
	// 	
	// 	var tokenDescriptor = new SecurityTokenDescriptor
	// 	{
	// 		Subject = identity,
	// 		Audience = _jwtConfig.Audience,
	// 		Issuer = _jwtConfig.Issuer,
	// 		Expires = DateTime.UtcNow.AddMinutes(5),
	// 		SigningCredentials = credentiasl
	// 	};
	// 	
	// 	var securityToken = jwtHandler.CreateToken(tokenDescriptor);
	// 	
	// 	var refreshToken = new RefreshToken
	// 	{
	// 		Id = Guid.NewGuid(),
	// 		Jti = jti,
	// 		CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
	// 		ExpiresAt = DateTimeOffset.UtcNow.AddMonths(6).ToUnixTimeSeconds(),
	// 		UserId = userId
	// 	};
	// 	
	// 	await _tokenRepo.SaveAsync(refreshToken);
	// 	
	// 	return new()
	// 	{
	// 		AccessToken = jwtHandler.WriteToken(securityToken),
	// 		RefreshToken = refreshToken.Id
	// 	};
	// }
	//
	// public Task<AuthenticationResult> ValidateTokensAsync(string accessTokenRequest, string RefreshTokenRequest)
	// {
	// 	
	// 	
	// 	
	// 	
	// 	// var refreshToken = await _tokenRepository.GetAsync()
	// 	throw new NotImplementedException();
	// }
	//
	// public TokenExtractionResult ExtractTokens(HttpRequest request)
	// {
	// 	
	// 	
	// 	
	// 	throw new System.NotImplementedException();
	// }
	//
	//
	//
	//
	// private SessionResult GetSession()
	// {
	// 	var key = "sessionId";
	// 	string? error = null;
	// 	UserSession? session = null;
	// 	
	// 	if (!_context.Request.Cookies.TryGetValue("session_id", out var rawSessionId))
	// 	{
	// 		error = "Session ID is required";
	// 	}
	// 	else if (!Guid.TryParse(rawSessionId, out var sessionId))
	// 	{
	// 		error = "Invalid Session ID";
	// 	}
	// 	else if (!_userSessions.TryGetValue(sessionId, out session))
	// 	{
	// 		error = "No Session was found";
	// 	}
	// 	
	// 	return error != null
	// 		? new SessionResult { Errors = new() { { key, new[] { error } } } }
	// 		: new SessionResult { Success = true, Session = session! };
	// }
	//
	// private SqlConstraintResult GetSqlUQConstraint(SqlException ex)
	// {
	// 	var message = ex.Message;
	// 	var startIndex = 36;
	// 	var endIndex = message.IndexOf('.') - 1;
	// 	
	// 	var constraint = message.Substring(startIndex, endIndex - startIndex);
	// 	
	// 	// constraint name template is 'Constraint_Table_Column'
	// 	// for example 'UQ_User_username' or 'UQ_User_email'
	// 	var parts = constraint.Split('_'); 
	// 	var column = parts[2];
	// 	
	// 	if (column == "id")
	// 		throw new Exception("Collision of '[User](id)' column");
	// 	
	// 	var errorMessage = column switch
	// 	{
	// 		"username" => "Username is already taken",
	// 		"email" => "Email address is already in use",
	// 		_ => throw new ArgumentException()
	// 	};
	// 	
	// 	return new SqlConstraintResult
	// 	{
	// 		Constraint = parts[0],
	// 		Table = parts[1],
	// 		Column = column,
	// 		ErrorMessage = errorMessage
	// 	};
	// }
	//
	// private string HashValue(string value)
	// {
	// 	var salt = Bcrypt.GenerateSalt();
	// 	var hashedPassword = Bcrypt.HashPassword(value, salt);
	// 	return hashedPassword;
	// }
	//
	// private bool ValidateHashedValue(string value, string hashedValue)
	// {
	// 	return Bcrypt.Verify(value, hashedValue);
	// }
	//
	// private string ExtractCookie()
	// {
	// 	// if 
	// 	// var rawCookie = _context.Request.Cookies
	// 	
	// 	
	// 	return "";
	// }
}
