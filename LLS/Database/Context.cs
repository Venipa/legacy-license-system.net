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
	Filename: Context.cs
*/
#endregion 
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LLS.Config;
using MySql.Data.MySqlClient;

namespace LLS.Database
{
    public class Context : DbContext
    {
        public static string ConnectionString { get { return GetConnectionString(); } }
        public DbSet<Users> Users { get; set; }
        public DbSet<ForgotPassword> ForgotPassword { get; set; }
        public DbSet<Licenses> Licenses { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<ProgramData> UserPrograms { get; set; }

        public Context() : base(ConnectionString)
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        private static string GetConnectionString()
        {
            MySqlConnectionStringBuilder mys = new MySqlConnectionStringBuilder();
            mys.Database = Loader.Config.MysqlDb;
            mys.Server = Loader.Config.MysqlServer;
            mys.UserID = Loader.Config.MysqlUsername;
            mys.Password = Loader.Config.MysqlPassword;
            
            return mys.ToString();
        }
    }
}
