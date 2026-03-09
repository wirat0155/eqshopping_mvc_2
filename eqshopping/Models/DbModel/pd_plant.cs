using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbModel
{
    public class pd_plant
    {
        [Key]
        public string plantno { get; set; }
        public string plantname { get; set; }
    }
}
