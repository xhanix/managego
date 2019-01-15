using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace ManageGo.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore), Serializable]
    public class TenantRequestItem
    {
        public int PageSize { get; set; } = 50;
        public int Page { get; set; } = 1;
        public IList<int> Buildings { get; set; }
        public IList<int> Units { get; set; }
        public string Search { get; set; }
        /// <summary>
        /// Default: Active.
        /// </summary>
        /// <value>The status.</value>
        public TenantStatus Status { get; set; } = TenantStatus.Active;


        [JsonIgnore]
        public int NumberOfAppliedFilters
        {
            get
            {
                var n = 0;
                if (Buildings != null && Buildings.Any())
                    n++;
                if (Units != null && Units.Any())
                    n++;
                if (Status != TenantStatus.Active)
                    n++;
                if (!string.IsNullOrWhiteSpace(Search))
                    n++;
                return n;
            }
        }


        public TenantRequestItem Clone()
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;
                return (TenantRequestItem)formatter.Deserialize(ms);
            }
        }
    }
}
