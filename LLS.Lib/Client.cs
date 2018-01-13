using LLS.Lib.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Crypt = BCrypt.Net;

namespace LLS.Lib
{
    public class LClient
    {
        private TcpClient _socket { get; set; }
        private SslStream _stream { get; set; }
        public bool IsConnected { get {
                bool k = _socket != null && _socket.Connected;
                if (k)
                {
                    try
                    {
                        _stream.WriteModel(RequestType.HEARTBEAT, new HeartBeatContext());
                        var hb = _stream.ReadModel<ResponseContext>();
                        return hb.IsSuccess;
                    }
                    catch (SocketException) { return false; }
                }
                return false;
            }
        }
        public LClient(string ip = null, ElapsedEventHandler CheckHeartBeat = null, int checkinterval = 10)
        {
            ip = ip == null ? "127.0.0.1" : ip;
            int port = 7070;
            _socket = new TcpClient();
            _socket.NoDelay = true;
            while(!_socket.Connected)
            {
                try
                {
                    _socket.Connect(IPAddress.Parse(ip), port);
                    Task.Delay(100).Wait();
                } catch { }
            }
            _stream = new SslStream(_socket.GetStream(),
                false, 
                new RemoteCertificateValidationCallback(CertCallBack));
            try
            {
                _stream.AuthenticateAsClient("LLS");
            } catch(AuthenticationException aex) { if (Debugger.IsAttached) Debug.WriteLine(aex); }
            if (_socket.Connected && CheckHeartBeat != null) HeartBeat(CheckHeartBeat, checkinterval);
            if (_socket.Connected)
                _socket.SendTimeout = 4;
            Task.Delay(1000).Wait();
        }

        private bool CertCallBack(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

        public async Task<ResponseContext> Authenticate(string key)
        {
            if (!_socket.Connected) throw new NotConnectedException();
            _stream.WriteModel(RequestType.AUTH, new AuthContext()
            {
                Auth = key
            });
            var r = _stream.ReadModel<ResponseContext>();
            if (r == null) return new ResponseContext(ResponseType.FAIL);
            return r;
        }
        public async Task<ResponseContext> Login(string email, string password)
        {
            if (!_socket.Connected) throw new NotConnectedException();
            _stream.WriteModel(RequestType.LOGIN, new LoginContext()
            {
                Email = email,
                Password = password
            });
            var r = _stream.ReadModel<ResponseContext>();
            if (Debugger.IsAttached) Debug.WriteLine(r.ToJsonString());
            return r;
        }
        public async Task<ResponseContext> Register(string email, string username, string password, string license)
        {
            if (!_socket.Connected) throw new NotConnectedException();
            _stream.WriteModel(RequestType.REGISTER, new RegisterContext()
            {
                Email = email,
                Password = password,
                Username = username,
                License = license
            });
            var r = _stream.ReadModel<ResponseContext>();
            if (r == null) return new ResponseContext(ResponseType.FAIL);
            return r;
        }
        public async Task<ResponseContext> ForgotPasswordRequest(string email)
        {
            if (!_socket.Connected) throw new NotConnectedException();
            _stream.WriteModel(RequestType.REGISTER, new ForgotPasswordGetContext()
            {
                Email = email
            });
            var r = _stream.ReadModel<ResponseContext>();
            if (r == null) return new ResponseContext(ResponseType.FAIL);
            return r;
        }
        public async Task<ResponseContext> ForgotPasswordRequestValidate(string email, string code)
        {
            if (!_socket.Connected) throw new NotConnectedException();
            _stream.WriteModel(RequestType.REGISTER, new ForgotPasswordPostContext()
            {
                Email = email,
                ResetCode = code
            });
            var r = _stream.ReadModel<ResponseContext>();
            if (r == null) return new ResponseContext(ResponseType.FAIL);
            return r;
        }
        public async Task HeartBeat(ElapsedEventHandler v, int checkInterval = 10)
        {
            Timer t = new Timer(checkInterval * 1000);
            t.AutoReset = true;
            t.Elapsed += HeartBeatElapsed;
            t.Elapsed += v;
            t.Start();
        }

        private void HeartBeatElapsed(object sender, ElapsedEventArgs e)
        {
        }
    }
}
