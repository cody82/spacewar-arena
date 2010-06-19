using System;
using System.Net.Mail;
using System.IO;

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
        string GenerateMessage(Exception e)
        {
            string message="";

            if(Root.Instance!=null)
            {
                message += "root version: " + Root.Instance.AssemblyVersion + "\n";
                if (Root.Instance.Mod != null)
                {
                    message += "mod version: " + Root.Instance.Mod.AssemblyVersion + "\n";
                }
            }

            try
            {
                StreamReader r = new StreamReader("info" + Path.DirectorySeparatorChar + "hg.txt");
                message += "hg version: " + r.ReadToEnd() + "\n";
            }
            catch(Exception)
            {
            }

            try
            {
                StreamReader r = new StreamReader("info" + Path.DirectorySeparatorChar + "compiler.txt");
                message += "compiler version: " + r.ReadToEnd() + "\n";
            }
            catch (Exception)
            {
            }

            for (Exception e2 = e; e2 != null;e2 = e2.InnerException)
            {
                message += "\n" + e2.Message + "\n\n" + e2.StackTrace;
                if (e2.InnerException != null)
                {
                    message += "\nInner Exception:\n";
                }
            }

            return message;
        }

		public void Send(Exception e)
		{
			Console.WriteLine ("mailing bug report...");
			
			string from2="noreply@spacewar-arena.com";
			string to="cody@spacewar-arena.com";
			
			MailAddress SendFrom = new MailAddress(from2);
			MailAddress SendTo = new MailAddress(to);

			MailMessage MyMessage = new MailMessage(SendFrom, SendTo);

			MyMessage.Subject = "Bug Report";
            MyMessage.Body = GenerateMessage(e);

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

