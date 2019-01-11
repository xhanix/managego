using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo.Models
{
    [AddINotifyPropertyChangedInterface]
    public class PaymentBase
    {
        public int PaymentId { get; set; }
        public string TransactionNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public Tenant Tenant { get; set; }
        public Unit Unit { get; set; }
        public Building Building { get; set; }
        [JsonIgnore]
        public string ShortDescription
        {
            get
            {
                return Tenant.FullName + ", " + Building?.BuildingShortAddress + ", " + Unit.UnitName;
            }
        }
    }

    public class Payment : PaymentBase
    {
        //public int PaymentId { get; set; }
        //public string Number { get; set; }
        //public decimal Amount { get; set; }
        public decimal OtherFee { get; set; }
        public decimal TenantFee { get; set; }
        public decimal Total { get; set; }
        public decimal TotalAmountAndFees { get; set; }
        //public DateTime TransactionDate { get; set; }
        [AlsoNotifyFor("StatusColor")]
        public string TransactionStatus { get; set; }
        public string TransactionType { get; set; }
        public string PaymentType { get; set; }
        public bool IsMarked { get; set; }
        public string PaymentNote { get; set; }
        public string PaymentAcctUsed { get; set; }
        public int? RecurringId { get; set; }
        public List<BankTransaction> ReverseTransactions { get; set; }
        public List<BankTransaction> BankTransactions { get; set; }
        [JsonIgnore]
        public string StatusColor
        {
            get
            {
                return TransactionStatus.ToLower().Contains("submitted") || TransactionStatus.ToLower().Contains("passed")
                        ? "#51bd23" : "#e23b3b";
            }
        }
        [JsonIgnore]
        public string FormattedTenantDetails
        {
            get
            {
                return Tenant is null ? "Unknown" : Tenant.FullName + " " + Tenant.FirstUnitAddress;
            }
        }
        [JsonIgnore]
        public bool DetailsShown { get; set; }
        [JsonIgnore]
        public string BankTransactionNumber
        {
            get
            {
                return BankTransactions is null ||
                      !BankTransactions.Any() ? string.Empty : BankTransactions.First().BankTransactionNumber;
            }
        }
    }
}
