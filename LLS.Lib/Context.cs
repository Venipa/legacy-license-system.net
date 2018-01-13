using LLS.Lib.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Lib
{
    public class HeartBeatContext
    {
        [JsonProperty("ts")]
        public string ts { get; set; } = DateTime.Now.Ticks.ToString();
    }
    public class AuthContext
    {
        [JsonProperty("auth"), JsonRequired]
        public string Auth { get; set; }
    }
    public class ForgotPasswordGetContext
    {
        [JsonProperty("email"), JsonRequired]
        public string Email { get; set; }
    }
    public class ForgotPasswordPostContext
    {
        [JsonProperty("email"), JsonRequired]
        public string Email { get; set; }
        [JsonProperty("reset"), JsonRequired]
        public string ResetCode { get; set; }
    }
    public class LoginContext
    {
        [JsonProperty("email")]
        public string Email { get; set; } = null;
        [JsonProperty("username")]
        public string Username { get; set; } = null;
        [JsonProperty("password"), JsonRequired]
        public string Password { get; set; } = null;
    }
    public class RegisterContext
    {
        [JsonProperty("email"), JsonRequired]
        public string Email { get; set; }
        [JsonProperty("username"), JsonRequired]
        public string Username { get; set; }
        [JsonProperty("password"), JsonRequired]
        public string Password { get; set; }
        [JsonProperty("license"), JsonRequired]
        public string License { get; set; }
    }
    public class ResponseContext
    {
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResponseType ResponseType { get; set; }

        public bool IsSuccess { get { return ResponseType == ResponseType.OK; } }
        public string Message { get
            {
                try
                {
                    var t = RequestContent.ToModel<MessageContext>();
                    if (t == null) return null;
                    return t.Message;
                } catch { return null; }
            } }

        [JsonProperty("content")]
        public string RequestContent { get; set; } = null;
        public ResponseContext() { }
        public ResponseContext(ResponseType t)
        {
            this.ResponseType = t;
        }
        public ResponseContext(ResponseType t, object msg)
        {
            this.ResponseType = t;
            this.RequestContent = msg.GetType() == typeof(string) ? new MessageContext(msg.ToString()).ToJsonString() : msg.ToJsonString();
        }
    }
    public class RequestContext
    {
        [JsonProperty("type"), JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        public RequestType RequestType { get; set; }

        [JsonProperty("content"), JsonRequired]
        public string RequestContent { get; set; } = null;
    }
    public class MessageContext
    {
        [JsonProperty("message"), JsonRequired]
        public string Message { get; set; } = null;

        public MessageContext(string msg) { this.Message = msg; }
    }
    public enum ResponseType
    {
        OK,
        FAIL,
        INVALID_REQUEST,
        NOT_AUTHED,
        PASS_WRONG,
        USERNAME_WRONG,
        USER_NOT_FOUND,
        USER_ALREADY_EXISTS,
        USER_BANNED,
        USER_LOCKED,
        USER_LOWER_PERM,
        LICENSE_ALREADY_USED,
        LICENSE_WRONG,
        LICENSE_NOT_FOUND,
        LICENSE_OK,
        LICENSE_EXPIRED,
        FORGOTPASS_CODE_NOT_FOUND,
        FORGOTPASS_CODE_INVALID,
        FORGOTPASS_CODE_USED,
        FORGOTPASS_OK,
        FORGOTPASS_LOCK
    }
    public enum RequestType
    {
        AUTH,
        LOGIN,
        REGISTER,
        HEARTBEAT,
        FORGOTPASS_REQUEST,
        FORGOTPASS_VALIDATE
    }
}
