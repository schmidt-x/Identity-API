using System;

namespace IdentityApi.Validation;

public static class ValidationHelper
{
	public static bool UsernameContainsRestrictedCharacters(string username)
	{
		foreach(var l in username)
			if (!char.IsLetter(l) && !char.IsNumber(l) && l != '_' && l != '.')
				return true;
		
		return false;
	}
	
	public static string[] ValidatePassword(string password)
	{
		var hasDigit = false;
		var hasSymbol = false;
		var hasLower = false;
		var hasUpper = false;
		var hasSpace = false;
		
		var totalErrors = 4;
		
		foreach(var l in password)
		{
			if (!hasLower && char.IsLower(l))
			{
				hasLower = true;
				totalErrors--;
			}
			else if (!hasUpper && char.IsUpper(l))
			{
				hasUpper = true;
				totalErrors--;
			}
			else if (!hasDigit && char.IsDigit(l))
			{
				hasDigit = true;
				totalErrors--;
			}
			else if (!hasSymbol && (char.IsSymbol(l) || char.IsPunctuation(l)))
			{
				hasSymbol = true;
				totalErrors--;
			}
			else if (!hasSpace && char.IsWhiteSpace(l))
			{
				hasSpace = true;
				totalErrors++;
			}
			else if (totalErrors == 1 && hasSpace)
				break;
		}
		
		if (totalErrors == 0)
			return Array.Empty<string>();
		
		var failures = new string[totalErrors];
		var i = 0;
		
		if (!hasLower) failures[i++] = "Password must contain at least one lower-case letter";
		if (!hasUpper) failures[i++] = "Password must contain at least one upper-case letter";
		if (!hasDigit) failures[i++] = "Password must contain at least one digit";
		if (!hasSymbol) failures[i++] = "Password must contain at least one symbol";
		if (hasSpace) failures[i] = "Password must not contain any white spaces";
		
		return failures;
	}
}