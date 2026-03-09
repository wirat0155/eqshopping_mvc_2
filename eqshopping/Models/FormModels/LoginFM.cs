using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eqshopping.Models.FormModels
{
    public class LoginFM
    {
        [Required(ErrorMessage = "กรุณาเลือก Plant ก่อน Login")]
        public string txt_plantno { get; set; }
        [Required(ErrorMessage = "กรุณากรอก รหัสผ่าน ก่อน Login")]
        public string txt_empno { get; set; }
        [Required(ErrorMessage = "กรุณากรอกข้อมูล รหัสผ่าน ก่อน Login")]
        public string txt_password { get; set; }
    }
}
