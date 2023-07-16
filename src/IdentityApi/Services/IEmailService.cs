using System.Threading.Tasks;

namespace IdentityApi.Services;

public interface IEmailService
{
	Task SendAsync(string emailTo, string message);
}