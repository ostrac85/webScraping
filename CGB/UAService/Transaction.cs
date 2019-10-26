using System.Collections.Generic;

namespace CGB.UAService
{
    public partial class Transaction
    {
        public string code { get; set; }
        public string balance { get; set; }
        public string time { get; set; }
        public string sign { get; set; }
        public List<TransactionRecord> records { get; set; }
    }

    public partial class TransactionRecord
    {
        public string payer { get; set; }
        public string detail { get; set; }
        public string transfer_from { get; set; }
        public string transfer_to { get; set; }
        public string trading_channel { get; set; }
        public decimal amount { get; set; }
        public decimal balance { get; set; }
        public string remarks { get; set; }
        public string notes { get; set; }
        public string transaction_time { get; set; }
        public int type { get; set; }
        public long? order_id { get; set; }
    }

    public partial class TempTransRecord
    {
        public long? order_id { get; set; }
        public decimal amount { get; set; }
        public string account_name { get; set; }
        public string account_number { get; set; }
        public string date_hour_mins { get; set; }
    }

    public class TransactionRecordEqualityComparer : IEqualityComparer<TransactionRecord>
    {
        public bool Equals(TransactionRecord x, TransactionRecord y)
        {
            // Two items are equal if their keys are equal.
            return x.detail == y.detail;
        }

        public int GetHashCode(TransactionRecord obj)
        {
            return obj.detail.GetHashCode();
        }
    }
}
