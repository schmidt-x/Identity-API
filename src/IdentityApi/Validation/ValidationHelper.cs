using System;
using IdentityApi.Domain.Constants;

namespace IdentityApi.Validation;

public static class ValidationHelper
{
	public static bool UsernameContainsRestrictedSymbols(string username)
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
		
		if (!hasLower) failures[i++] = ErrorMessage.PasswordMustContainLowerCase;
		if (!hasUpper) failures[i++] = ErrorMessage.PasswordMustContainUpperCase;
		if (!hasDigit) failures[i++] = ErrorMessage.PasswordMustContainDigit;
		if (!hasSymbol) failures[i++] = ErrorMessage.PasswordMustContainSymbol;
		if (hasSpace) failures[i] = ErrorMessage.PasswordContainsWhiteSpace;
		
		return failures;
	}
}