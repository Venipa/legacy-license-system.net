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
	Filename: ClientHandler.cs
*/
#endregion 
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;
using LLS.Lib;
using LLS.Lib.Extensions;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;

namespace LLS.Networking
{
    public class ClientHandler
    {
        private Guid _id;
        private TcpClient ClientSocket;
        private IPEndPoint ClientEndPoint;
        private Byte[] ClientBuffer;
        private Handler.CommandHandler ClientCommantHandler;
        private string EncKey { get { return Crypto.MD5(Config.Loader.Config.EncryptionKey); } }
        public bool IsAuthed { get; set; } = false;
        public IPEndPoint IPAddress { get { return ClientEndPoint; } }
        public string AuthKey { get; set; }
        private SslStream _stream { get; set; }
        private StringBuilder readData { get; set; }

        public ClientHandler(Guid id, TcpClient HandleSocket, Handler.CommandHandler commandHandler, X509Certificate cert, SslStream s)
        {
            try
            {
                _id = id;
                readData = new StringBuilder();
                this._stream = s;
                this.ClientCommantHandler = commandHandler;
                ClientSocket = HandleSocket;
                ClientEndPoint = (IPEndPoint)HandleSocket.Client.RemoteEndPoint;

                ClientBuffer = new Byte[1024];
                _stream.BeginRead(ClientBuffer, 0, ClientBuffer.Length, new AsyncCallback(ReceiveSSL), _stream);
                //ClientSocket.Client.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, new AsyncCallback(Receive), ClientSocket);
            }
            catch(Exception ex) { Log.WriteLine(ex); this.Disconnect(); }
        }
        private void RemoveClient()
        {
            if(Listener._clients.TryRemove(_id, out TcpClient _client))
            { 
                Log.WriteLine("Client {0} - ({1}) disconnected!", IPAddress.ToString(), _id.ToString());
            }
        }
        private void ReceiveSSL(IAsyncResult ar)
        {
            if (!ClientSocket.Connected)
            {
                Disconnect();
            }
            // Read the  message sent by the server.
            // The end of the message is signaled using the
            // "<EOF>" marker.
            SslStream stream = (SslStream)ar.AsyncState;
            int byteCount = -1;
            try
            {
                try
                {
                    byteCount = stream.EndRead(ar);
                } catch(IOException)
                {
                    clean();
                }
                Array.Resize(ref ClientBuffer, byteCount);
                readData.Append(Encoding.UTF8.GetString(ClientBuffer));
                // Check for EOF or an empty message.
                if (readData.ToString().IndexOf("<EOF>") == -1 && byteCount > 0)
                {
                    if (Debugger.IsAttached) Debug.WriteLine("BEGIN:" + readData.ToString());
                    // We are not finished reading.
                    // Asynchronously read more message data from  the server.
                    stream.BeginRead(ClientBuffer, 0, ClientBuffer.Length,
                        new AsyncCallback(ReceiveSSL),
                        stream);
                }
                else
                {
                    if (Debugger.IsAttached) Debug.WriteLine("END:"+readData.ToString());
                    Receive(readData.ToString().Replace("<EOF>", ""));
                    readData = new StringBuilder();
                }
            }
            catch(SocketException)
            {
                Disconnect();
            }
            catch
            {
                Disconnect();
            }
        }
        private void Receive(string data)
        {
            try
            {
                if (data.Length != 0)
                {
                    string DecryptedMessage = data;
                    var IncomingObj = DecryptedMessage.ToModel<RequestContext>();
                    if(IncomingObj == null)
                    {
                        Send((new ResponseContext() { ResponseType = ResponseType.FAIL }).ToJsonString());
                        this.Disconnect();
                        return;
                    }
                    if(!this.IsAuthed && IncomingObj.RequestType != RequestType.AUTH)
                    {
                        Send((new ResponseContext() { ResponseType = ResponseType.NOT_AUTHED }).ToJsonString());
                        this.Disconnect();
                        return;
                    }
                    if(ClientCommantHandler.TryGetAttribute(IncomingObj.RequestType, out Handler.CommandAttribute CO))
                    {
                        var ob = IncomingObj.RequestContent.ToModel(CO.Context);
                        if(ob == null)
                        {
                            Send((new ResponseContext() { ResponseType = ResponseType.INVALID_REQUEST }).ToJsonString());
                            return;
                        }
                        var rc = ClientCommantHandler.ExecuteAsync(IncomingObj.RequestType, ob, this).GetAwaiter().GetResult();
                        if (!ClientSocket.Connected) Shutdown();
                        if (rc != null)
                        {
                            Send(rc.ToJsonString());
                        } else
                        {
                            Send((new ResponseContext() { ResponseType = ResponseType.FAIL }).ToJsonString());
                        }
                    }
                }
                else { this.Disconnect(); }
            }
            catch(JsonSerializationException)
            {
                if(ClientSocket.Connected)
                {
                    Send((new ResponseContext() { ResponseType = ResponseType.FAIL }).ToJsonString());
                }
            }
            catch(Exception ex) { Log.WriteLine(ex); this.Disconnect(); }
        }
        void clean()
        {

            ClientBuffer = new Byte[1024];
            _stream.BeginRead(ClientBuffer, 0, ClientBuffer.Length, new AsyncCallback(ReceiveSSL), _stream);
            //ClientSocket.Client.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, new AsyncCallback(Receive), ClientSocket);
        }
        private Boolean CheckString(String Value)
        {
            foreach (Char Character in Value)
            {
                if (!Char.IsLetterOrDigit(Character)) { return false; }
            }

            return true;
        }

        private void Send(string Message)
        {
            if (!ClientSocket.Connected) { this.Disconnect(); return; }
            
            try
            {
                byte[] Buffer = Encoding.ASCII.GetBytes(Message + "<EOF>");
                if (Debugger.IsAttached) Log.WriteLine(LogSeverity.Debug, "SEND> {0}", false, Message.ToString());

                _stream.BeginWrite(Buffer, 0, Buffer.Length, new AsyncCallback(MessageSent), _stream);
                
            }
            catch(Exception ex) { Log.WriteLine(ex); }
            clean();
        }

        private void MessageSent(IAsyncResult ar)
        {
            try { _stream.EndWrite(ar); }
            catch (Exception ex) { Log.WriteLine(ex); }
        }
        private void Disconnect() => Shutdown();
        private void Shutdown()
        {
            try
            {
                RemoveClient();
                ClientSocket.Close();
                _stream.Dispose();
                if (ClientSocket.Client != null) ClientSocket.Client.Dispose();
            }
            catch(Exception ex) { Log.WriteLine(ex); }
        }
    }
}
