using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbModel
{
    public class eqs_shoppingtran
    {
        [Key]
        public long tranid { get; set; }
        public string plantno { get; set; }
        public string posno { get; set; }
        public string partno { get; set; }
        public int cellid { get; set; }
        public bool shoppingflag { get; set; }
        public DateTime? shoppingdate { get; set; }
        public bool samelastpartnoflag { get; set; }
        public bool discheckingfinishflag { get; set; }
        public bool checkingfinishflag { get; set; }
        public DateTime? checkingfinishdate { get; set; }
        public string remarks { get; set; }
        public string creuser { get; set; }
        public DateTime credate { get; set; }
        public string lastuser { get; set; }
        public DateTime lastupdate { get; set; }
        public string system { get; set; }
    }
}
