using System;

namespace eqshopping.Models.DbView
{
    public class ShoppingHistoryDTO
    {
        public long tranid { get; set; }
        public string plantno { get; set; }
        public string posno { get; set; }
        public string partno { get; set; }
        public string cellno { get; set; }
        public DateTime? checkingfinishdate { get; set; }
        public string StartCheckUser { get; set; }
        public short seqno { get; set; }
        public string equipmentno { get; set; }
        public DateTime? checkingdate { get; set; }
        public string checkinguser { get; set; }
    }
}
