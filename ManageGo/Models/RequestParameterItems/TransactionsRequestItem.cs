using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using CustomCalendar;
using Newtonsoft.Json;

namespace ManageGo.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore), Serializable]
    public class TransactionsRequestItem
    {
        public int PageSize { get; set; } = 50;
        public int Page { get; set; } = 1;
        public IList<int> BankAccounts { get; set; }
        public int? AmountFrom { get; set; }
        public int? AmountTo { get; set; }
        public string Search { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        [JsonIgnore, IgnoreDataMember]
        DateRange DefaultDateRange => new DateRange(DateTime.Today, DateTime.Today.AddDays(-30));

        [JsonIgnore]
        public int NumberOfAppliedFilters
        {
            get
            {
                var n = 0;
                if (BankAccounts != null && BankAccounts.Count > 0)
                    n++;
                if ((DateFrom != null && DateFrom.Value.Date != DefaultStartDate.Date) || (DateTo != null && DateTo.Value.Date != DefaultToDate.Date))
                    n++;
                if ((AmountFrom != null && AmountFrom > 0) || (AmountTo != null && AmountTo < 5000))
                    n++;
                if (!string.IsNullOrWhiteSpace(Search))
                    n = 1;
                return n;
            }
        }
        [JsonIgnore, IgnoreDataMember]
        public DateTime DefaultStartDate => DefaultDateRange.StartDate;

        [JsonIgnore, IgnoreDataMember]
        public DateTime DefaultToDate => DefaultDateRange.EndDate.Value;

        public TransactionsRequestItem Clone()
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;
                return (TransactionsRequestItem)formatter.Deserialize(ms);
            }
        }
    }
}
