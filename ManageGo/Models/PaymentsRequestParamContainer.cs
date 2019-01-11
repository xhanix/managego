using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace ManageGo.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore), Serializable]
    public class PaymentsRequestParamContainer
    {
        public int PageSize { get; set; } = 50;
        public int Page { get; set; } = 1;

        public IList<int> Buildings { get; set; }
        public IList<int> Tenants { get; set; }
        public IList<int> Units { get; set; }
        public int? AmountFrom { get; set; }
        public int? AmountTo { get; set; }
        public string Search { get; set; }
        public DateTime DateFrom { get; set; } = DateTime.Today;
        public DateTime? DateTo { get; set; }

        public PaymentsRequestParamContainer Clone()
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;
                return (PaymentsRequestParamContainer)formatter.Deserialize(ms);
            }
        }

    }
}
