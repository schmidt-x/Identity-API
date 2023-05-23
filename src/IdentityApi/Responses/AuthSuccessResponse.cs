using System;

namespace IdentityApi.Responses;

public class AuthSuccessResponse
{
	public string Message { get; set; } = default!;
	public string AccessToken { get; set; } = default!;
	public Guid RefreshToken { get; set; }
}