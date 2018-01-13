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
	Filename: Licenses.cs
*/
#endregion 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Database
{
    [Table("licenses")]
    public class Licenses
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
		[MaxLength(30), Required, Index(IsUnique = true)]
        public string key { get; set; }
        [DefaultValue(0)]
        public bool IsUsed { get; set; } = false;
        [DefaultValue(null)]
        public int? UsedBy { get; set; }
        [DefaultValue(false)]
        public bool IsLifetime { get; set; } = false;
        [ForeignKey("UsedBy")]
        public Users UsedByUser { get; set; }
        public int? ProgramRefId { get; set; }
        [ForeignKey("ProgramRefId")]
        public ProgramData Program { get; set; }
        public int Level { get; set; } = 1;
        [DefaultValue(60*60*24)]
        public TimeSpan ExpiresIn { get; set; } = TimeSpan.FromDays(1);
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
