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
	Filename: Program.cs
*/
#endregion 
using LLS.Handler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LLS
{
    public class Program
    {
        Database.Context LLSDB;
        private CommandHandler _handler;
        private Networking.Listener _listener;
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Exception ex = e.ExceptionObject as Exception;
                Log.WriteLine(ex);
                Console.ReadLine();
                Environment.Exit(0);
            };
            Log.WriteLine("Starting LLS...");
            new Program().runAyy().Wait();
        }
        public async Task runAyy()
        {
            _handler = new CommandHandler(Assembly.GetExecutingAssembly());
            try
            {
                Log.WriteLine("Loading Config...");
                new Config.Loader().Load();
                Log.WriteLine("Loading DB Init...");
                LLSDB = new Database.Context();
                if(LLSDB.Database.CreateIfNotExists()) Log.WriteLine("Loading DB Migration...");
                _listener = new Networking.Listener(_handler);
                _listener.serverCertificate = new X509Certificate2(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LLS_SSL.pfx"), Config.Loader.Config.CertPassword);
            }
            catch (Exception ex)
            {
                if(Debugger.IsAttached)
                {
                    Log.WriteLine(ex);
                } else
                {
                    Log.WriteLine(LogSeverity.Error, "Could not connect to MySQL! Ex: " + ex.ToString());
                }
                Console.ReadLine();
                Environment.Exit(0);
            }
            Log.WriteLine(LogSeverity.Info, "Modules Loaded: {0}", _handler.CommandNames);
            await Task.Delay(-1);
        }
    }
}
