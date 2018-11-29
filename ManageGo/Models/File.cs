using System;
using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class File
    {
        public int ID { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public byte[] Content { get; set; }
        [JsonIgnore]
        public int ParentComment { get; set; } = 0;
        [JsonIgnore]
        public string ParentCommentTempId { get; set; }
    }
}
