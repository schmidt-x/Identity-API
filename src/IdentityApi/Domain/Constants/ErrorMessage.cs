namespace IdentityApi.Domain.Constants;

public static class ErrorMessage
{
	
	public const string SessionNotFound = "Session is not found";
	public const string SessionIdNotFound = "Session ID is not found";
	public const string InvalidSessionId = "Invalid session ID";
	
	public const string Unauthorized = "Unauthorized. Register or log in";
	
	public const string UnexpectedError = "Unexpected error has occured";
	
	public const string EmailAlreadyVerified = "Email address is already verified";
	public const string EmailNotVerified = "Email address is not verified";
	public const string OldEmailNotVerified = "Old email address is not verified";
	public const string EmailRequired = "Email address is required";
	public const string NewEmailRequired = "New email address is required";
	public const string InvalidEmail = "Invalid email address";
	public const string EmailsEqual = "New email address cannot be the same as the old email";
	
	public const string UsernameRequired = "Username is required";
	public const string UsernameTooShort = "Username must contain at least 3 characters";
	public const string UsernameTooLong = "Username must not exceed the limit of 32 characters";
	public const string UsernameContainsRestrictedSymbols = "Username can only contain letters, numbers, underscores and periods";
	
	public const string WrongLoginPassword = "Wrong login/password";
	public const string WrongPassword = "Wrong password";
	public const string PasswordRequired = "Password is required";
	public const string PasswordTooShort = "Password must contain at least 8 characters";
	public const string PasswordsEqual = "New password cannot be the same as the old password";
	public const string PasswordMustContainLowerCase = "Password must contain at least one lower-case letter";
	public const string PasswordMustContainUpperCase = "Password must contain at least one upper-case letter";
	public const string PasswordMustContainDigit = "Password must contain at least one digit";
	public const string PasswordMustContainSymbol = "Password must contain at least one symbol";
	public const string PasswordContainsWhiteSpace = "Password must not contain any white spaces";
	
	public const string InvalidAccessToken = "Invalid access token";
	public const string AccessTokenRequired = "Access token is required";
	public const string InvalidRefreshToken = "Invalid refresh token";
	public const string RefreshTokenRequired = "Refresh token is required";
	public const string RefreshTokenInvalidated = "Refresh token is invalidated";
	public const string RefreshTokenUsed = "Refresh token is already used";
	public const string RefreshTokenExpired = "Refresh token is expired";
	public const string TokensNotMatch = "Tokens do not match";
	
	public const string VerificationCodeRequired = "Verification code is required";
}