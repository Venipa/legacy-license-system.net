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
	Filename: CommandAttribute.cs
*/
#endregion 
using LLS.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Handler
{
    public class CommandAttribute : Attribute
    {
        public RequestType Command { get; set; }
        public Type Context { get; set; }
        public CommandAttribute(RequestType Command, Type ContextType)
        {
            this.Command = Command;
            this.Context = ContextType;
        }
        public CommandAttribute(RequestType Command)
        {
            this.Command = Command;
        }
    }
}
