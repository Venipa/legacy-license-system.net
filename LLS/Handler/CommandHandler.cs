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
	Filename: CommandHandler.cs
*/
#endregion 
using LLS.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Handler
{
    public class CommandHandler
    {
        public Dictionary<RequestType, MethodInfo> CommandList = new Dictionary<RequestType, MethodInfo>();
        public string CommandNames { get
            {
                return string.Join(", ", CommandList.Keys);
            } }
        public CommandHandler(Assembly ass)
        {
            var typs = ass.GetTypes().Where(x => x.GetMethods().Where(y => y.GetCustomAttributes(typeof(CommandAttribute), true).Count() > 0).Count() > 0).Select(a => a.GetMethods().Where(ayy => ayy.GetCustomAttributes(typeof(CommandAttribute), true).Count() > 0));
            foreach(var typ in typs)
            {
                foreach(var me in typ)
                {
                    CommandAttribute ca = (CommandAttribute)me.GetCustomAttributes(typeof(CommandAttribute), true).First();
                    CommandList.Add(ca.Command, me);
                }
            }
        }
        public async Task<ResponseContext> ExecuteAsync(RequestType command, params object[] args)
        {
            var me = CommandList.FirstOrDefault(x => x.Key == command).Value;
            Log.WriteLine(LogSeverity.Debug, "Executed Module: {0}", command.ToString());
            if (me != null)
            {
                var cmdAttr = (CommandAttribute)me.GetCustomAttribute(typeof(CommandAttribute));
                ResponseContext r = (ResponseContext)me.Invoke(null, args);
                return r;
            }
            return new ResponseContext() { ResponseType = ResponseType.FAIL };
        }
        public async Task<T> ExecuteAsync<T>(RequestType command, params object[] args)
        {
            var me = CommandList.FirstOrDefault(x => x.Key == command).Value;
            if(me != null)
            {
                var cmdAttr = me.GetCustomAttribute(typeof(CommandAttribute));
                T r = (T)Convert.ChangeType(me.Invoke(this, args), typeof(T));
                return r;
            }
            return default(T);
        }
        public bool TryGetAttribute(RequestType command, out CommandAttribute CO)
        {
            CO = null;
            var me = CommandList.FirstOrDefault(x => x.Key == command);
            if(me.Value != null)
            {
                CO = (CommandAttribute)me.Value.GetCustomAttributes().Where(x => x != null).FirstOrDefault();
                return CO != null;
            } else
            {
                return false;
            }
        }
    }
}
