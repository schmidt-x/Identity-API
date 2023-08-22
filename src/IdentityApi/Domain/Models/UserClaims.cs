using System;

namespace IdentityApi.Domain.Models;

public class UserClaims
{
	public Guid Id { get; set; }
	public string Email { get; set; } = default!;
}