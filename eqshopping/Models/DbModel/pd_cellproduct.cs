using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbModel
{
    public class pd_cellproduct
    {
        [Key]
        public int cellproductid { get; set; }
        public string plantno { get; set; }
        public string partno { get; set; }
        public int cellid { get; set; }
    }
}
