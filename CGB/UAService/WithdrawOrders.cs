using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGB.UAService
{
    public class WithdrawOrder
    {
        public string notify_status { get; set; }
        public string source_card_code { get; set; }
        public string notify_url { get; set; }
        public DateTime? updated_at { get; set; }
        public string updated_by { get; set; }
        public DateTime? exact_time { get; set; }
        public DateTime? operating_time { get; set; }
        public DateTime? created_at { get; set; }
        public int? status { get; set; }
        public string notes { get; set; }
        public string remarks { get; set; }
        public decimal fee { get; set; }
        public decimal amount { get; set; }
        public string bank { get; set; }
        public string account_number { get; set; }
        public string account_name { get; set; }
        public string source_bank { get; set; }
        public string source_account_number { get; set; }
        public string source_account_name { get; set; }
        public string player_id { get; set; }
        public string mch_order_no { get; set; }
        public string mch_id { get; set; }
        public long id { get; set; }
        public string issuing_bank { get; set; }
    }

    public class WithdrawOrders
    {
        public List<WithdrawOrder> list { get; set; }
    }
}
