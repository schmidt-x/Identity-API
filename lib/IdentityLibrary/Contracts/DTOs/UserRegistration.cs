﻿namespace IdentityLibrary.Contracts.DTOs;

public class UserRegistration
{
	public string Username { get; set; }
	public string Password { get; set; }
	public string ConfirmPassword { get; set; }
}