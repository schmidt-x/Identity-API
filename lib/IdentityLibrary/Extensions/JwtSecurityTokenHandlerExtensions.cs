﻿namespace IdentityLibrary.Extensions;

public static class JwtSecurityTokenHandlerExtensions
{
	public static bool TryValidate(
		this JwtSecurityTokenHandler handler, 
		string token, 
		TokenValidationParameters parameters, 
		out SecurityToken? validatedToken)
	{
		try
		{
			_ = handler.ValidateToken(token, parameters, out validatedToken);
			return true;
		}
		catch
		{
			validatedToken = default;
			return false;
		}
	}
}