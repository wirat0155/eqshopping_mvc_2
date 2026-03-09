using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbModel
{
    public class pd_cell
    {
        [Key]
        public int cellid { get; set; }
        public string plantno { get; set; }
        public string cellno { get; set; }
        public string statusno { get; set; }
        public bool eqslockflag { get; set; }
        public bool eqs_scanposflag { get; set; }
        public bool eqs_scanassyflag { get; set; }
    }
}
