namespace IdentityApi.Domain.Constants;

public class ErrorMessage
{
	public const string SessionNotFound = "Session not found";
	
	public const string EmailAlreadyVerified = "Email address is already verified";
	public const string EmailNotVerified = "Email address is not verified";
	public const string OldEmailNotVerified = "Old email address is not verified";
	public const string NewEmailRequired = "New email address required";
	
	public const string WrongLoginPassword = "Wrong login/password";
	public const string WrongPassword = "Wrong password";
	
	public const string InvalidAccessToken = "Invalid access token";
	public const string InvalidRefreshToken = "Invalid refresh token";
	public const string RefreshTokenInvalidated = "Refresh token is invalidated";
	public const string RefreshTokenUsed = "Refresh token is already used";
	public const string RefreshTokenExpired = "Refresh token is expired";
	public const string TokensNotMatch = "Tokens do not match";
	
	
	
	
}