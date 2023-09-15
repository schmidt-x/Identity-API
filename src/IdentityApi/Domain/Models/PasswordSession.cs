using System;

namespace IdentityApi.Domain.Models;

public class PasswordSession : Session
{
	public Guid UserId { get; set; }
}