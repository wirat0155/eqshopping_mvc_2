using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbModel
{
    public class eqs_usermanagement
    {
        [Key]
        public int cellunlockuserid { get; set; }
        public string plantno { get; set; }
        public string username { get; set; }
        public int cellid { get; set; }
        public bool unlockcellflag { get; set; }
        public bool datamanagementflag { get; set; }
        public bool distributorflag { get; set; }
        public string creuser { get; set; }
        public DateTime credate { get; set; }
        public string lastuser { get; set; }
        public DateTime lastupdate { get; set; }
    }
}
