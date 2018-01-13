#region Copyright
/*	
	 __      ________ _   _ _____ _____        
	 \ \    / /  ____| \ | |_   _|  __ \ /\    
	  \ \  / /| |__  |  \| | | | | |__) /  \   
	   \ \/ / |  __| | . ` | | | |  ___/ /\ \  
	    \  /  | |____| |\  |_| |_| |  / ____ \ 
	     \/   |______|_| \_|_____|_| /_/    \_\
                                        
	Copyright (c) 2017 - 2018 gitlab.com/Venipa
	Project: LLS
	User: Venipa .
	Filename: Listener.cs
*/
#endregion 
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using LLS.Config;

namespace LLS.Networking
{
    class Listener
    {
        private TcpListener ListenSocket;
        public static IPEndPoint usersocket;
        public Handler.CommandHandler CommandHandler;
        public X509Certificate serverCertificate;
        public static ConcurrentDictionary<Guid, TcpClient> _clients;
        public Listener(Handler.CommandHandler _handler)
        {
            try
            {
                _clients = new ConcurrentDictionary<Guid, TcpClient>();
                this.CommandHandler = _handler;
                IPEndPoint EndPoint = new IPEndPoint(IPAddress.Any, Loader.Config.ListenPort);

                ListenSocket = new TcpListener(EndPoint);
                ListenSocket.Start();
                Log.WriteLine("Listening on {0}", EndPoint.ToString(), Color.Gold);
                ListenSocket.BeginAcceptTcpClient(new AsyncCallback(NewClient), ListenSocket);
                Timer t = new Timer(15000)
                {
                    AutoReset = true,
                };
                t.Elapsed += T_Elapsed;
                t.Start();
            }
            catch(Exception ex) { Log.WriteLine(ex); }
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            _clients.Where(x => !x.Value.Connected).ToList().ForEach(x =>
            {
                if(_clients.TryRemove(x.Key, out TcpClient v))
                {
                    v.Close();
                }
            });
            Log.WriteLine(false, "Clients Connected: {0}", _clients.Count());
        }

        private void NewClient(IAsyncResult Result)
        {
            try
            {
                TcpClient ClientSocket = ListenSocket.EndAcceptTcpClient(Result);
                ListenSocket.BeginAcceptTcpClient(new AsyncCallback(NewClient), ListenSocket);
                if (_clients.Count(x => ((IPEndPoint)x.Value.Client.RemoteEndPoint).Address == (ClientSocket.Client.RemoteEndPoint as IPEndPoint).Address) > 0)
                {
                    ClientSocket.Close();
                    return;
                }
                Guid g = Guid.NewGuid();
                if (!_clients.TryAdd(g, ClientSocket))
                {
                    ClientSocket.Close();
                    return;
                }
                SslStream _stream = new SslStream(ClientSocket.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(CertCallBack));
                try
                {
                    _stream.AuthenticateAsServer(serverCertificate);
                } catch(AuthenticationException) { ClientSocket.Close(); return; }

                ClientSocket.ReceiveTimeout = 60000;
                ClientHandler Handle = new ClientHandler(g, ClientSocket, CommandHandler, serverCertificate, _stream);
                usersocket = ((IPEndPoint)ClientSocket.Client.RemoteEndPoint);
                Log.WriteLine(LogSeverity.Info, "Client Connected - " + usersocket.Address.ToString() +":"+usersocket.Port.ToString());
            }
            catch(Exception ex) { Log.WriteLine(ex); }

        }

        private bool CertCallBack(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
    }
}
