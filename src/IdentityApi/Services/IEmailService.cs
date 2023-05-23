namespace IdentityApi.Services;

public interface IEmailService
{
	void Send(string emailTo, string message);
}