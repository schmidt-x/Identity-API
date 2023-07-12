using System;
using System.Linq;
using IdentityApi.Contracts.Options;
using Microsoft.Extensions.Options;

namespace IdentityApi.Services;

public class CodeGenerationService : ICodeGenerationService
{
	private readonly VerificationCodeOptions _codeOptions;

	public CodeGenerationService(IOptions<VerificationCodeOptions> codeOptions)
	{
		_codeOptions = codeOptions.Value;
	}
	
	public string Generate()
	{
		return new string(Enumerable
			.Repeat(_codeOptions.Text, _codeOptions.Length)
			.Select(x => x[Random.Shared.Next(x.Length)])
			.ToArray());
	}
}