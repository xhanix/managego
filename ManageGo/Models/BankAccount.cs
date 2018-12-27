using Newtonsoft.Json;
using PropertyChanged;

namespace ManageGo.Models
{
    [AddINotifyPropertyChangedInterface]
    public class BankAccount
    {
        public int BankAccountID { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountNumber { get; set; }

        [JsonIgnore]
        public string Title
        {
            get { return BankAccountName + " (" + BankAccountNumber + ")"; }
        }

        [JsonIgnore, AlsoNotifyFor("CheckBoxImage")]
        public bool IsSelected { get; set; }

        [JsonIgnore]
        public string CheckBoxImage
        {
            get
            {
                return IsSelected ? "checked.png" : "unchecked.png";
            }
        }
    }
}
