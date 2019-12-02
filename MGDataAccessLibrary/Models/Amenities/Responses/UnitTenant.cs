using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;


namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class UnitTenant : INotifyPropertyChanged
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;
        public string Email { get; set; }
        public string HomePhone { get; set; }
        public string CellPhone { get; set; }
        public string Address { get; set; }
        public string Unit { get; set; }
        public int UnitId { get; set; }
        public int BuildingId { get; set; }
        public int Id { get; set; }

        private bool isSelected;
        [JsonIgnore]
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (IsSelected != value)
                {
                    isSelected = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
