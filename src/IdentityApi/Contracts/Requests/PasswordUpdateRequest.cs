using System.Text.Json.Serialization;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Contracts.Requests;

public class PasswordUpdateRequest
{
	public string Password { get; set; } = default!;
	
	[JsonPropertyName(Key.NewPassword)]
	public string NewPassword { get; set; } = default!;
}