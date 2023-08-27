using System;
using System.Text.Json.Serialization;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Contracts.Responses;

public class Me
{
	public string Username { get; set; } = default!;
	public string Email { get; set; } = default!;
	
	[JsonPropertyName(Key.CreatedAt)]
	public DateTime CreatedAt { get; set; }
	
	[JsonPropertyName(Key.UpdatedAt)]
	public DateTime UpdatedAt { get; set; }
	
	public string Role { get; set; } = default!;
	public string Token { get; set; } = default!;
}