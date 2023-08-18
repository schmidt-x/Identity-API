using System.Threading.Tasks;

namespace IdentityApi.Services;

public interface IEmailSender
{
	Task SendAsync(string emailTo, string message);
}