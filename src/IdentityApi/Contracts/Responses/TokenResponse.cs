using System;
using System.Text.Json.Serialization;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Contracts.Responses;

public class TokenResponse
{
	[JsonPropertyName(Key.AccessToken)]
	public string AccessToken { get; set; } = default!;
	
	[JsonPropertyName(Key.RefreshToken)]
	public Guid RefreshToken { get; set; }
}