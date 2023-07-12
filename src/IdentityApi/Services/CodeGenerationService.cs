using System;
using System.Linq;

namespace IdentityApi.Services;

public class CodeGenerationService : ICodeGenerationService
{
	public string Generate(int length)
	{
		const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz";
		
		return new string(Enumerable
			.Repeat(chars, length)
			.Select(x => x[Random.Shared.Next(x.Length)])
			.ToArray());
	}
}