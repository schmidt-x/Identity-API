using System;

namespace IdentityApi.Models;

public class UserClaims
{
	public Guid Id { get; set; }
	public string Email { get; set; } = default!;
}