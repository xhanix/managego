using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo.Models
{
    [AddINotifyPropertyChangedInterface]
    public class BankTransaction
    {
        private string bankAccountInfo;
        public int Id { get; set; }
        //what is Number? depricated API field? 
        //confusion with field names from API (check docs after reviewed)
        public string Number { get; set; } //1
        public string BankTransactionNumber { get; set; } //2
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public bool IsMarked { get; set; }
        [AlsoNotifyFor("BuildingsCount", "Subtitle")]
        public string BankAccountInfo
        {
            get
            {
                string regex = "(\".*\")|('.*')|(\\(.*\\))";
                return Regex.Replace(bankAccountInfo, regex, "").Replace("****", "");
            }
            set
            {
                bankAccountInfo = value;
            }
        }
        public int TenantTransactionsCount { get; set; }
        public List<PaymentBase> Payments { get; set; }

        public string BuildingsCount
        {
            get
            {
                var m = Regex.Match(bankAccountInfo, "\\d* buildings");
                if (m.Success)
                {
                    return m.Value.Replace(" buildings", "");
                }
                return "1";
            }
        }
        [JsonIgnore]
        public string Subtitle
        {
            get
            {
                return BuildingsCount == "1" && Payments != null && Payments.Any()
                     && Payments.FirstOrDefault(t => t.Building != null) != null ?
                    Payments.First(t => t.Building != null).Building.BuildingName : BankAccountInfo;
            }
        }
        [JsonIgnore]
        public string SubtitleIcon
        {
            get
            {
                return Subtitle == BankAccountInfo ? "bank_grey.png" : "building_grey.png";
            }
        }
        [JsonIgnore]
        public bool DetailsShown { get; set; }
        [JsonIgnore]
        public string StatusColor
        {
            get
            {
                return Amount >= 0 ? "#51bd23" : "#e23b3b";
            }
        }
        [JsonIgnore]
        public double PaymentsListHeight
        {
            get
            {
                return Payments != null ? Payments.Count * 50 : 0;
            }
        }
    }
}
