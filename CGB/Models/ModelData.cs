using System;
using System.Collections.Generic;
using System.Linq;
using CGB.Models.ConfigSettings;

namespace CGB.Models
{
    public static class ModelData
    {
        public static CGB.UAService.Card Card { get; set; }
        public static WebSettings WebSetting { get; set; }
        public static Transactions Transactions { get; set; } = new Transactions();

        public static Orders Orders { get; set; } = new Orders();
        public static UAService.CardLoginHistory CardLoginHistory { get; set; } = new UAService.CardLoginHistory();
    }
    
    public enum ReloginMode
    {
        AUTOMATIC,
        MANUAL
    }

    public class Transactions
    {
        //public string WebURL { get; set; }
        //public int ColCurrentPage { get; set; }
        //public int ColTotalPage { get; set; }
        //public DateTime LastTransDate { get; set; }
        //public List<BankCardLib.Models.Transaction> WebTrans = new List<BankCardLib.Models.Transaction>();
        public bool AutopayFinished { get; set; } 
        public bool CollectionFinished { get; set; }
        public bool BalanceFinished { get; set; }

        public int ProcessedWithdrawal { get; set; }

        public void Clear()
        {
            //WebURL = "";
            //ColCurrentPage = 1;
            //ColTotalPage = -1;
            //WebTrans.Clear();
            //LastTransDate = DateTime.Now.Date;
            AutopayFinished = false;
            CollectionFinished = false;
            BalanceFinished = false;
            ProcessedWithdrawal = 0;
        }
    }

    public class Orders
    {
        //public List<WithdrawOrder> Withdrawals = new List<WithdrawOrder>();
        public List<UAService.WithdrawOrder> Withdrawals = new List<UAService.WithdrawOrder>();
        //public int ProcessedWithdrawal { get; set; }

        public Boolean CanProcessOrders()
        {
            // Check if have orders
            // Check if Processed orders count exceeds' config no. of processed withdrawal
            return Withdrawals.Count > 0 &&
                   ModelData.Transactions.ProcessedWithdrawal < 2;
        }

        public UAService.WithdrawOrder FirstOrder { get { return Withdrawals.FirstOrDefault(); } }

        public string FirstOrderCode
        {
            get
            {
                if (FirstOrder == null) return "";

                return FirstOrder.id.ToString();
            }
        }

        public void SetOrderStatus(OrderStatus orderStatus, string notes)
        {
            FirstOrder.status = (sbyte)orderStatus;
            FirstOrder.notes = notes;
        }
    }

    public enum OrderStatus
    {
        INVALID = 4,
        AUTOCLOSE = 2
    }

    public enum InputPaymentAction
    {
        account_name,
        account_number,
        bank,
        amount
    }
}