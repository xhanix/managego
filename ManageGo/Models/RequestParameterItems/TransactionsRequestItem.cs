using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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


        [JsonIgnore]
        public int NumberOfAppliedFilters
        {
            get
            {
                var n = 0;
                if (BankAccounts != null)
                    n++;
                if (DateFrom != null)
                    n++;
                if (DateTo != null)
                    n++;
                if (AmountFrom != null)
                    n++;
                if (AmountTo != null)
                    n++;
                if (!string.IsNullOrWhiteSpace(Search))
                    n++;
                return n;
            }
        }

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
