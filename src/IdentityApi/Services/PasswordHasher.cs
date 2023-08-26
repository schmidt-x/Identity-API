namespace IdentityApi.Services;

using Bcrypt = BCrypt.Net.BCrypt;

public class PasswordHasher : IPasswordHasher
{
	public string HashPassword(string password)
	{
		return Bcrypt.HashPassword(password, Bcrypt.GenerateSalt());
	}

	public bool VerifyPassword(string password, string hashedPassword)
	{
		return Bcrypt.Verify(password, hashedPassword);
	}
}