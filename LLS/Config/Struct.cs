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
	Filename: Struct.cs
*/
#endregion 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LLS.Config
{
    public class Struct
    {
        [JsonProperty("mysql_server")]
        public string MysqlServer { get; set; }
        [JsonProperty("mysql_database")]
        public string MysqlDb { get; set; }
        [JsonProperty("mysql_username")]
        public string MysqlUsername { get; set; }
        [JsonProperty("mysql_password")]
        public string MysqlPassword { get; set; }
        [JsonProperty("listen_ip")]
        public string ListenIP { get; set; }
        [JsonProperty("listen_port")]
        public int ListenPort { get; set; }
        [JsonProperty("encryption_key")]
        public string EncryptionKey { get; set; } = "test1";
        [JsonProperty("cert_pass")]
        public string CertPassword { get; set; } = null;
    }
}
