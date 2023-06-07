using System;

namespace IdentityApi.Services;

public interface IUserContext
{
	Guid GetId();
}