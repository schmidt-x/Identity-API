namespace IdentityApi.Validation.Helpers;

public static class ValidationHelper
{
	public static bool ContainsRestrictedCharacters(string username)
	{
		if (string.IsNullOrWhiteSpace(username)) return false;
		
		foreach(var l in username)
			if (!char.IsLetter(l) && !char.IsNumber(l) && l != '_' && l != '.')
				return true;
		
		return false;
	}
}