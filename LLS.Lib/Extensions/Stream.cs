using LLS.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Lib.Extensions
{
    public static class Auth
    {
        public static string key = "";
    }
    public static class Stream
    {
        public static void WriteString(this SslStream s, string Message)
        {
            if (s.CanWrite)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(Message + "<EOF>");
                s.Write(buffer, 0, buffer.Length);
                if (Debugger.IsAttached) Debug.WriteLine("WRITE> " + Message);
            }
        }
        public static void WriteAuth<RequestContext>(this SslStream s, RequestContext Message)
        {
            if (s.CanWrite)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(Message.ToJsonString());
                s.Write(buffer, 0, buffer.Length);
            }
        }
        public static void WriteModel<T>(this SslStream s, RequestType rt, T Message)
        {
            WriteString(s, new RequestContext()
            {
                RequestContent = Message.ToJsonString(),
                RequestType = rt
            }.ToJsonString());
        }
        public static string ReadString(this SslStream s)
        {
            int bytecount = 1;
            StringBuilder sb = new StringBuilder();
            string rd = string.Empty;
            while (bytecount > 0 && rd.IndexOf("<EOF>") == -1)
            {
                byte[] buffer = new byte[1024];
                bytecount = s.Read(buffer, 0, buffer.Length);
                Array.Resize(ref buffer, bytecount);
                rd = Encoding.ASCII.GetString(buffer);
                sb.Append(rd);
                if (Debugger.IsAttached) Debug.WriteLine("READ> " + sb.ToString());
            }
            return sb.Replace("<EOF>", "").ToString();
        }
        public static T ReadModel<T>(this SslStream s)
        {
            try
            {
                return ReadString(s).ToModel<T>();
            } catch(Exception ex)
            {
                if (Debugger.IsAttached) Debug.WriteLine(ex);
                return default(T);
            }
        }
    }
}
