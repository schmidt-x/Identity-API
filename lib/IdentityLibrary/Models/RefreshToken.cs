namespace IdentityLibrary.Models;

public class RefreshToken
{
	public Guid Id { get; set; }
	public Guid JwtId { get; set; }
	public long CreationDate { get; set; }
	public long ExpiryDate { get; set; }
	public bool Used { get; set; }
	public bool Invalidated { get; set; }
	public Guid UserId { get; set; }
}