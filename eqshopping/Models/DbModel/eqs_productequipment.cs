using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbModel
{
    public class eqs_productequipment
    {
        [Key]
        public int productequipmentid { get; set; }
        public string plantno { get; set; }
        public int? processid { get; set; }
        public string partno { get; set; }
        public string taskcode { get; set; }
        public int equipmentid { get; set; }
        public short seqno { get; set; }
        public string creuser { get; set; }
        public DateTime credate { get; set; }
        public string lastuser { get; set; }
        public DateTime lastupdate { get; set; }
    }
}
