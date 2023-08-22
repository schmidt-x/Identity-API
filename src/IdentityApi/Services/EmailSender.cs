using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using IdentityApi.Options;
using Microsoft.Extensions.Options;

namespace IdentityApi.Services;

public class EmailSender : IEmailSender
{
	private readonly EmailOptions _email;
	
	public EmailSender(IOptions<EmailOptions> options)
	{
		_email = options.Value;
	}
	
	
	public async Task SendAsync(string emailTo, string message)
	{
		using var emailMessage = new MailMessage
		{
			From = new(_email.Address, "IdentityApi"),
			To = { new(emailTo) },
			Subject = "Email verification",
			Body = message,
		};
		
		using var smtpClient = new SmtpClient("smtp.gmail.com", 587)
		{
			EnableSsl = true,
			DeliveryMethod = SmtpDeliveryMethod.Network,
			UseDefaultCredentials = false,
			Credentials = new NetworkCredential(_email.Address, _email.Password)
		};
		
		await smtpClient.SendMailAsync(emailMessage);
	}
}