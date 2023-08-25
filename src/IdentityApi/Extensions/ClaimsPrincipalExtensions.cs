using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IdentityApi.Extensions;

public static class ClaimsPrincipalExtensions
{
	public static string? FindEmail(this ClaimsPrincipal principal, bool microsoftClaimTypes = false)
	{
		return microsoftClaimTypes
			? principal.FindFirstValue(ClaimTypes.Email)
			: principal.FindFirstValue(JwtRegisteredClaimNames.Email);
	}
	
	public static string? FindId(this ClaimsPrincipal principal, bool microsoftClaimTypes = false)
	{
		return microsoftClaimTypes
			? principal.FindFirstValue(ClaimTypes.NameIdentifier)
			: principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
	}
}