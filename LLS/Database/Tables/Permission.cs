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
	Filename: Permission.cs
*/
#endregion 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Database
{
    [Table("permissions")]
    public class Permission
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        public string key { get; set; }
        [Required]
        public string name { get; set; }
        public ICollection<UserPermission> Users { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
    [Table("users_permissions")]
    public class UserPermission
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public int PermRefId { get; set; }
        [ForeignKey("PermRefId")]
        public Permission Perm { get; set; }
        public int UserRefId { get; set; }
        [ForeignKey("UserRefId")]
        public Users User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
