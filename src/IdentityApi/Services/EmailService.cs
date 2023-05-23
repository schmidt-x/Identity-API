using System.Net;
using System.Net.Mail;
using IdentityApi.Contracts.Options;
using Microsoft.Extensions.Options;

namespace IdentityApi.Services;

public class EmailService : IEmailService
{
	private readonly EmailOptions _email;
	
	public EmailService(IOptions<EmailOptions> options)
	{
		_email = options.Value;
	}
	
	
	public void Send(string emailTo, string message)
	{
		using var emailMessage = new MailMessage
		{
			From = new(_email.Address, "IdentityApi"),
			To = { new(emailTo) },
			Subject = "Email verification",
			Body = message,
		};
		
		var smtpClient = new SmtpClient("smtp.gmail.com", 587)
		{
			EnableSsl = true,
			DeliveryMethod = SmtpDeliveryMethod.Network,
			UseDefaultCredentials = false,
			Credentials = new NetworkCredential(emailMessage.From.Address, _email.Password)
		};
		
		smtpClient.Send(emailMessage);
	}
}