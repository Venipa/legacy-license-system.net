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
	Filename: Log.cs
*/
#endregion 
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Console = Colorful.Console;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LLS
{
    public enum LogSeverity
    {
        Critical = 0,
        Error = 1,
        Warning = 2,
        General = 3,
        Info = 4,
        Verbose = 5,
        Debug = 6
    }
    public static class Log
    {
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
        private static ReaderWriterLockSlim xlock = new ReaderWriterLockSlim();
        private static string path = AppDomain.CurrentDomain.BaseDirectory;
        public static async Task WriteLine(Exception ex, params object[] args)
        {
            // Get stack trace for the exception with source file information
            var st = new StackTrace(ex, true);
            // Get the top stack frame
            var frame = st.GetFrame(0);
            // Get the line number from the stack frame
            var line = frame.GetFileLineNumber();
            var filename = frame.GetFileName();

            WriteLine(LogSeverity.Error, ex.ToString(), Color.Red);
            return;
            if (Debugger.IsAttached)
            {
                WriteLine(LogSeverity.Error, ex.ToString(), Color.Red);
            }
            else
            {
                WriteLine(LogSeverity.Error, "Exception: {0}, Line: {1}:{2}", ex.Message, filename ?? "Unknown", line, Color.Red, args);
            }
        }
        public static async Task WriteLine(bool writeToLog = true, string str = "", params object[] args) => WriteLine(LogSeverity.Info, str.ToString(), writeToLog, args);
        public static async Task WriteLine(string str = "", params object[] args) => WriteLine(LogSeverity.Info, str.ToString(), args);
        public static async Task WriteLine(LogSeverity type, string str = "", params object[] args) => WriteLine(type, str.ToString(), true, args);
        public static async Task WriteLine(LogSeverity type, string str = "", bool writeToLog = true, params object[] args)
        {
            if (!Debugger.IsAttached && type >= LogSeverity.Verbose) return;
            if (Debugger.IsAttached) Debug.WriteLine(str.ToString(), args);
            DateTime dt = DateTime.UtcNow;
            if(writeToLog) xlock.EnterWriteLock();
            try
            {
                Color clr = Color.LightGray;
                foreach (object va in args)
                {
                    if (va != null)
                    {
                        if (va.GetType() == typeof(Color))
                        {
                            clr = (Color)va;
                        }
                    }
                }
                string arg = string.Format(str, args);
                string write = string.Format("[{0}][{1}]: {2}", dt.ToString("HH:mm:ss"), type.ToString().ToUpper(), arg);
                if(IsRunningOnMono())
                {
                    System.Console.WriteLine(write);
                } else
                {
                    Console.WriteLine(write, Color.FromArgb(clr.R, clr.G, clr.B));
                }
                if(writeToLog) File.AppendAllText(Path.Combine(path, $"{dt.ToString("dd-MM")}.log"), $"{write}\r\n");
            }
            finally
            {
                if(writeToLog) xlock.ExitWriteLock();
            }
        }
        public static async Task ToFile(LogSeverity type, string filename = "general", string str = "", params object[] args)
        {
            xlock.EnterWriteLock();
            DateTime dt = DateTime.UtcNow;
            try
            {
                string arg = string.Format(str, args);
                string write = string.Format("[{0}][{1}]: {2}", dt.ToString("HH:mm:ss"), type.ToString().ToUpper(), arg);
                File.AppendAllText(Path.Combine(path, $"{filename}.log"), $"{write}\r\n");
            }
            finally
            {
                xlock.ExitWriteLock();
            }
        }
    }
}
