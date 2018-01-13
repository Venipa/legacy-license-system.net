using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Lib
{
    /// <summary>
    /// Throws if not Connected to Server
    /// Example: Heartbeat => Not Connected => NotConnectedException
    /// </summary>
    public class NotConnectedException : Exception { }
    /// <summary>
    /// Throws if the Response was an unexpected Type/Class
    /// </summary>
    public class InvalidResponseException : Exception { }
}
