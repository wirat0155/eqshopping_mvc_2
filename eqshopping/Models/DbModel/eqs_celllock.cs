using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbModel
{
    public class eqs_celllock
    {
        [Key]
        public long locktranid { get; set; }
        public string plantno { get; set; }
        public int cellid { get; set; }
        public int tranid { get; set; }
        public string lockreason { get; set; }
        public bool lockflag { get; set; }
        public DateTime lockdate { get; set; }
        public DateTime? unlockdate { get; set; }
        public string unlockreason { get; set; }
        public string creuser { get; set; }
        public DateTime credate { get; set; }
        public string lastuser { get; set; }
        public DateTime lastupdate { get; set; }
    }
}
