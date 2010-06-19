using System;
using System.Net.Mail;

namespace Cheetah.Bugs
{
	public interface IBugReport
	{
		void Send(Exception e);
	}

	public class BugReport
	{
        public static IBugReport Instance = new MailBugReport();
	}

	public class MailBugReport : IBugReport
	{
		public void Send(Exception e)
		{
			Console.WriteLine ("mailing bug report...");
			
			string from2="noreply@spacewar-arena.com";
			string to="cody@spacewar-arena.com";
			
			MailAddress SendFrom = new MailAddress(from2);
			MailAddress SendTo = new MailAddress(to);

			MailMessage MyMessage = new MailMessage(SendFrom, SendTo);

			MyMessage.Subject = "Bug Report";
			MyMessage.Body = e.Message + "\r\n\r\n" + e.StackTrace;

			//Attachment attachFile = new Attachment("");
			//MyMessage.Attachments.Add(attachFile);

			SmtpClient emailClient = new SmtpClient("spacewar-arena.com");
			emailClient.Send(MyMessage);
		}
	}

	public class HttpBugReport
	{
		public void Send(Exception e)
		{
			throw new Exception("NYI");
		}
	}
}

