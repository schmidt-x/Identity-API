using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace IdentityApi.Extensions;

public static class ClaimsPrincipalExtensions
{
	public static string? FindEmail(this ClaimsPrincipal principal)
	{
		return principal.FindFirstValue(JwtRegisteredClaimNames.Email);
	}
}