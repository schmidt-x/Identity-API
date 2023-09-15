using System;

namespace IdentityApi.Domain.Models;

/// <summary>
/// Represents the claims to be included in a Jwt access token
/// </summary>
public class UserClaims
{
	public Guid Id { get; set; }
	public string Email { get; set; } = default!;
}