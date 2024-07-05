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
	  var portStr = Environment.GetEnvironmentVariable("SMTP_PORT");
	  var username = Environment.GetEnvironmentVariable("SMTP_USERNAME");
	  var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
	  var from = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL");

	  // Validate and parse port
	  if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(portStr) ||
		  string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
		  string.IsNullOrEmpty(from) || !int.TryParse(portStr, out var port)) {
		throw new InvalidOperationException("SMTP configuration environment variables are not properly set.");
	  }

	  _smtpClient = new SmtpClient(host, port) {
		Credentials = new NetworkCredential(username, password),
		EnableSsl = true
	  };
	}

	public void SendEmail(string to, string subject, string body) {
	  var from = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL");

	  if (string.IsNullOrEmpty(from)) {
		throw new InvalidOperationException("SMTP_FROM_EMAIL environment variable is not set.");
	  }

	  var message = new MailMessage(from, to, subject, body);
	  _smtpClient.Send(message);
	}
  }
}