using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.DbView
{
    public class vw_emp_general
    {
        [Key]
        public string empno { get; set; }
        public string empname { get; set; }
        public string empstatusno { get; set; }
    }
}
