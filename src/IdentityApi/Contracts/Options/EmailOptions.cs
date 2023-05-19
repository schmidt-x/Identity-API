using System;

namespace IdentityApi.Contracts.Options;

public class EmailOptions
{
	public const string Email = "Email";
	
	public string Address { get; set; } = String.Empty; 
	public string Password { get; set; } = String.Empty;
}