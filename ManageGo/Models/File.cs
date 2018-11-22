using System;
using PropertyChanged;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class File
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
