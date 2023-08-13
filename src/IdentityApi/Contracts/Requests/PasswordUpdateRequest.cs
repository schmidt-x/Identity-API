using System.Text.Json.Serialization;

namespace IdentityApi.Contracts.Requests;

public class PasswordUpdateRequest
{
	public string Password { get; set; } = default!;
	
	[JsonPropertyName("new_password")]
	public string NewPassword { get; set; } = default!;
}