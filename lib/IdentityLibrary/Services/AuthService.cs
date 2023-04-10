namespace IdentityLibrary.Services;

public class AuthService : IAuthService
{
	private readonly IConfiguration _config;
	private readonly IUserRepository _userRepo;
	private readonly IRefreshTokenRepository _tokenRepository;

	public AuthService(IConfiguration config, IUserRepository userRepo, IRefreshTokenRepository tokenRepository)
	{
		_config = config;
		_userRepo = userRepo;
		_tokenRepository = tokenRepository;
	}
	
	
	public async Task<AuthenticationResult> RegisterAsync(UserRegister userRegister)
	{
		var existsResult = await _userRepo.ExistsAsync(userRegister.Username, userRegister.Email);
		
		if (existsResult.Exists)
			return new() { Errors = existsResult.Errors };
		
		User user = new()
		{
			Id = Guid.NewGuid(),
			Username = userRegister.Username,
			Email = userRegister.Email,
			Password = HashValue(userRegister.Password),
			Role = "user",
			CreatedAt = DateTime.UtcNow
		};
		
		try
		{
			await _userRepo.SaveAsync(user);
		}
		catch(SqlException ex) when (ex.Number == 2627)
		{
			var sqlResult = GetSqlUQConstraint(ex);
			return new() { Errors = new() { { sqlResult.Column, sqlResult.ErrorMessage } } }; 
		}
		
		return new()
		{
			Success = true,
			UserClaims = new()
			{
				Id = user.Id,
				Email = user.Email,
				Role = user.Role
			}
		};
	}

	public async Task<AuthenticationResult> AuthenticateAsync(UserLogin userRegister)
	{
		var user = await _userRepo.GetAsync(userRegister.Username);
		
		if (user == null)
			return new AuthenticationResult { Errors = new() { {"auth", "incorrect login or password"} } };
		
		var isCorrect = ValidateHashedValue(userRegister.Password, user.Password);
		
		if (!isCorrect)
			return new AuthenticationResult { Errors = new() { {"auth", "incorrect login or password"} } };
		
		return new()
		{
			Success = true,
			UserClaims = new()
			{
				Id = user.Id,
				Email = user.Email,
				Role = user.Role
			}
		};
	}
	
	public async Task<TokenGenerationResult> GenerateTokensAsync(UserClaims claims)
	{
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
		var credentiasl = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
		var jwtHandler = new JwtSecurityTokenHandler(); 
		
		var identity = new ClaimsIdentity(new[]
		{
			new Claim("id", claims.Id.ToString()),
			new Claim("email", claims.Email),
			new Claim("role", claims.Role),
			new Claim("jti", Guid.NewGuid().ToString())
		}, "Bearer");
		
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = identity,
			Audience = _config["Jwt:Audience"],
			Issuer = _config["Jwt:Issuer"],
			Expires = DateTime.UtcNow.AddMinutes(5),
			SigningCredentials = credentiasl
		};
		
		var securityToken = jwtHandler.CreateToken(tokenDescriptor);
		
		var refreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			Jti = Guid.Parse(securityToken.Id),
			CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
			ExpiresAt = DateTimeOffset.UtcNow.AddMonths(6).ToUnixTimeSeconds(),
			UserId = claims.Id
		};
		
		await _tokenRepository.SaveAsync(refreshToken);
		
		return new()
		{
			AccessToken = jwtHandler.WriteToken(securityToken),
			RefreshToken = refreshToken.Id.ToString()
		};
	}

	public Task<AuthenticationResult> ValidateTokensAsync(string accessTokenRequest, string RefreshTokenRequest)
	{
		throw new System.NotImplementedException();
	}


	public TokenExtractionResult ExtractTokens(HttpRequest request)
	{
		throw new System.NotImplementedException();
	}
	
	public void SetRefreshToken(string refreshToken, HttpResponse response)
	{
		var base64UrlRefreshToken = Base64UrlEncoder.Encode(refreshToken);
		var cookieOptions = new CookieOptions
		{
			HttpOnly = true,
			Secure = true,
			Expires = DateTimeOffset.UtcNow.AddMonths(6)
		};
		
		response.Cookies.Append("jwt_refresh_token", base64UrlRefreshToken, cookieOptions);
	}
	private SqlConstraintResult GetSqlUQConstraint(SqlException ex)
	{
		var message = ex.Message;
		var startIndex = 36;
		var endIndex = message.IndexOf('.') - 1;
		
		var constraint = message.Substring(startIndex, endIndex - startIndex);
		
		// constraint name template is 'Constraint_Table_Column'
		// for example 'UQ_User_username' or 'UQ_User_email'
		var parts = constraint.Split('_'); 
		var column = parts[2];
		
		if (column == "id")
			throw new Exception("Collision of '[User](id)' column");
		
		var errorMessage = column switch
		{
			"username" => "Username is already taken",
			"email" => "Email address is already used",
			_ => throw new ArgumentException()
		};
		
		return new()
		{
			Constraint = parts[0],
			Table = parts[1],
			Column = column,
			ErrorMessage = errorMessage
		};
	}
	private string HashValue(string value)
	{
		var salt = Bcrypt.GenerateSalt();
		var hashedPassword = Bcrypt.HashPassword(value, salt);
		return hashedPassword;
	}
	private bool ValidateHashedValue(string value, string hashedValue)
	{
		return Bcrypt.Verify(value, hashedValue);
	}
}
