using System.ComponentModel.DataAnnotations;

namespace IdentityLibrary.Contracts.DTOs;

public class VerificationRequest
{
	[Required(ErrorMessage = "Verificaiton code is required")]
	public string Code { get; set; }
}