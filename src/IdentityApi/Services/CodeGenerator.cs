using System;
using System.Linq;
using IdentityApi.Options;
using Microsoft.Extensions.Options;

namespace IdentityApi.Services;

public class CodeGenerator : ICodeGenerator
{
	private readonly VerificationCodeOptions _code;

	public CodeGenerator(IOptions<VerificationCodeOptions> codeOptions)
	{
		_code = codeOptions.Value;
	}
	
	public string Generate()
	{
		return new string(Enumerable
			.Repeat(_code.Text, _code.Length)
			.Select(x => x[Random.Shared.Next(x.Length)])
			.ToArray());
	}
}