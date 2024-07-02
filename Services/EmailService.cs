// Services/EmailService.cs
using System.Net;
using System.Net.Mail;

namespace Cloud.Services {
  public interface IEmailService {
	void SendEmail(string to, string subject, string body);
  }

  public class EmailService : IEmailService {
	private readonly SmtpClient _smtpClient;

	public EmailService() {
	  var host = Environment.GetEnvironmentVariable("SMTP_HOST");
	  var port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT"));
	  var username = Environment.GetEnvironmentVariable("SMTP_USERNAME");
	  var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

	  _smtpClient = new SmtpClient(host, port) {
		Credentials = new NetworkCredential(username, password),
		EnableSsl = true
	  };
	}

	public void SendEmail(string to, string subject, string body) {
	  var from = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL");
	  var message = new MailMessage(from, to, subject, body);
	  _smtpClient.Send(message);
	}
  }
}