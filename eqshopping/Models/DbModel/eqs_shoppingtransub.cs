using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbModel
{
    public class eqs_shoppingtransub
    {
        [Key]
        public long transubid { get; set; }
        public long tranid { get; set; }
        public short seqno { get; set; }
        public string taskcode { get; set; }
        public string equipmentno { get; set; }
        public bool shoppingflag { get; set; }
        public DateTime? shoppingdate { get; set; }
        public string shoppinguser { get; set; }
        public bool checkingflag { get; set; }
        public DateTime? checkingdate { get; set; }
        public string checkinguser { get; set; }
        public bool adjustflag { get; set; }
        public DateTime? adjustdate { get; set; }
        public string adjustuser { get; set; }
    }
}
