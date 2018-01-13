using EASendMail;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Networking
{
    public class Mail
    {
        SmtpMail oMail;
        SmtpClient oClient;
        SmtpServer oServer;
        public Mail(string ToEmail)
        {
            oMail = new SmtpMail("TryIt");
            oClient = new SmtpClient();

            oMail.From = "noreply@venipa.net";
            oMail.To = ToEmail;

            oServer = new SmtpServer("");
        }
        public async Task<bool> Send(string Subject, string Body)
        {
            oMail.Subject = Subject;
            oMail.TextBody = Body;
            try
            {
                if(Debugger.IsAttached)
                {
                    Log.WriteLine(LogSeverity.Debug, "Email({0}), Subject({1}), Message({2})", oMail.To.ToString(), Subject, Body);
                    return true;
                }
                oClient.SendMail(oServer, oMail);
            } catch(Exception ex)
            {
                Log.WriteLine(ex);
                return false;
            }
            return true;
        }
    }
}
