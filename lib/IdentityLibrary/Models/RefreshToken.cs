namespace IdentityLibrary.Models;

public class RefreshToken
{
	public Guid Id { get; set; }
	public Guid Jti { get; set; }
	public long CreatedAt { get; set; }
	public long ExpiresAt { get; set; }
	public bool Used { get; set; }
	public bool Invalidated { get; set; }
	public Guid UserId { get; set; }
}