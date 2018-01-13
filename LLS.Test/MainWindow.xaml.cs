using LLS.Lib;
using LLS.Lib.Extensions;
using LLS.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LLS.Test
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string AuthenticationKey = "mavis.123";
        public string EncKey = "test1";
        LClient _client;
        UserData _user;
        IPInfo _ip;
        public MainWindow()
        {
            InitializeComponent();
            _ip = new IPInfo();
            _ip = _ip.getHost();
            Task.Run((Action)(() =>
            {
                AsyncMain();
            }));
        }
        public async Task AsyncMain()
        {
            await Task.Delay(500);
            _client = new LClient();
            if(_ip.IsValid)
            {
                Console.WriteClient($"Current User: {_ip.IPAddress.ToString()}, {(_ip.ISP ?? "-")}");
            }
            await _client.Authenticate(AuthenticationKey).ContinueWith(async x =>
            {
                if (x.IsCompleted && x.Result.IsSuccess)
                {
                    Console.WriteClient("Successfully connected to Server!");
                    await LoginOrRegister();
                } else if(x.IsCanceled || x.IsFaulted)
                {
                    Console.WriteClient("Could not Authenticate :/");
                } else
                {
                    Console.WriteClient("Invalid Auth Key");
                }
            });
        }
        public async Task LoginOrRegister()
        {

            try
            {
                var data = await _client.Login("admin@venipa.net", "admin");
                if (!data.IsSuccess) Console.WriteServer(data.Message ?? "Fail on Login");
                if (data.ResponseType == ResponseType.USER_NOT_FOUND || data.ResponseType == ResponseType.LICENSE_NOT_FOUND)
                {
                    Console.WriteServer(data.Message);
                    var r = await _client.Register("admin@venipa.net", "venipa", "admin", "test");
                    if (r.IsSuccess)
                    {
                        _user = (await _client.Login("admin@venipa.net", "admin")).RequestContent.ToModel<UserData>();
                    }
                }
                else if (data.IsSuccess)
                {
                    _user = data.RequestContent.ToModel<UserData>();
                    Console.WriteClient($"License is Active: {_user.IsActive.ToString()}");
                } else
                {
                    Console.WriteServer(data.Message ?? "Failed on Login");
                }
            }
            catch { Console.WriteClient("Invalid Response."); }
        }
    }
    public static class Ext
    {
        public static void WriteServer(this TextBox t, string msg)
        {
            Write(t, "Server> ", msg);
        }
        public static void WriteClient(this TextBox t, string msg)
        {
            Write(t, "Client> ", msg);
        }
        static void Write(this TextBox t, string prefix,string msg)
        {
            t.Dispatcher.BeginInvoke((Action)(() =>
            {
                t.Text += (prefix + msg) + Environment.NewLine;
            }));
        }
    }
}
