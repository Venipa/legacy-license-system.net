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
	Filename: Loader.cs
*/
#endregion 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LLS.Config
{
    class Loader
    {
        public static Struct Config;
        static string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        public void Load()
        {
            if (!File.Exists(path))
            {
                Log.WriteLine(LogSeverity.Error, "Config not Found");
                Struct st = new Struct();
                File.WriteAllText(path, JsonConvert.SerializeObject(st, Formatting.Indented));
                Config = st;
            }
            Config = JsonConvert.DeserializeObject<Struct>(File.ReadAllText(path));
        }
    }
}
