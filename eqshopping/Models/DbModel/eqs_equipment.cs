using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbModel
{
    public class eqs_equipment
    {
        [Key]
        public int equipmentid { get; set; }
        public string equipmentno { get; set; }
        public string plantno { get; set; }
        public string equipmenttypeno { get; set; }
        public string creuser { get; set; }
        public DateTime credate { get; set; }
        public string lastuser { get; set; }
        public DateTime lastupdate { get; set; }
    }
}
