using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbModel
{
    public class Product
    {
        public string PartNo { get; set; }
        public bool? eqs_scanposflag { get; set; }
        public bool? eqs_scanassyflag { get; set; }
    }
}
