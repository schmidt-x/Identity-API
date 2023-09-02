using IdentityApi.Domain.Models;

namespace IdentityApi.Results;

public class Tokens
{
	public string AccessToken { get; set; } = default!;
	public RefreshToken RefreshToken { get; set; } = default!;
}