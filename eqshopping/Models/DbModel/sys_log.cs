using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbModel
{
    public class sys_log
    {
        [Key]
        public long id { get; set; }
        public string username { get; set; }
        public string systemno { get; set; }
        public string eventname { get; set; }
        public DateTime? eventdate { get; set; }
        public string? ipaddress { get; set; }
    }
}
