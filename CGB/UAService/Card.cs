using System;

namespace CGB.UAService
{
    public class Card
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Code { get; set; }
        public bool? Status { get; set; }
        public string UkeyPassword { get; set; }
        public string PayPassword { get; set; }
        public string LoginPassword { get; set; }
        public string LoginUsername { get; set; }

        // Utility
        public string Reminder { get; set; }
        public bool? Online { get; set; }
        public string Level { get; set; }
        public int? FkAccountTypeId { get; set; }
        public int FkBankId { get; set; }
        public int? FkCompanyId { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime CreateTime { get; set; }
        public string BankName { get; set; }
        public decimal? Balance { get; set; }
        public int Id { get; set; }
        public string UpdatedBy { get; set; }
        public string CreatedBy { get; set; }
    }
}
