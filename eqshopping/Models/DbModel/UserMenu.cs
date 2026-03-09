using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eqshopping.Models.DbModel
{
    [Table("usermenu")]
    public class UserMenu
    {
        [Key]
        public int id { get; set; }
        public string username { get; set; }
        public int mnuid { get; set; }
        public bool id_enabled { get; set; }
        public bool id_visible { get; set; }
        public bool id_addnew { get; set; }
        public bool id_edit { get; set; }
        public bool id_delete { get; set; }
        public bool id_print { get; set; }
        public bool id_export { get; set; }
    }
}
