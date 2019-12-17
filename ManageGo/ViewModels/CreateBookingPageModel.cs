using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;
using MGDataAccessLibrary.Models.Amenities.Responses;
using PropertyChanged;
using Xamarin.Forms;

namespace ManageGo
{
    internal class CreateBookingPageModel : BaseDetailPage
    {
        private Models.PMCBuilding selectedBuilding;
        private BuildingUnit selectedUnit;
        private Models.BuildingAmenity selectedAmenity { get; set; }
        private DateTime? selectedDate;
        private string totalAmount;

        public double TenantListHeight { get; set; }
        public bool SetFromTime { get; private set; }
        public bool SetToTime { get; private set; }
        public bool SwitchingAmPam { get; private set; }
        public bool ShouldShowClock { get; set; }
        public bool TimerIsForStart { get; private set; }
        public bool RequestPayment { get; set; }
        public bool MarkAsPaid { get; set; }
        public bool NotApplicable { get; set; }
        public bool ApproveBooking { get; set; }
        public bool OtherTextFieldIsVisible { get; set; }
        public string OtherNameText { get; set; }
        public string NoteToTenant { get; set; }
        public bool ListViewIsVisible { get; set; }
        public ObservableCollection<AvailableDaysAndTimes> ListOfAvailableTimes { get; private set; }
        public ObservableCollection<DateTime> ListOfAvailableDays { get; private set; }
        public ObservableCollection<Models.AvailableTimes> FromTimes { get; private set; } = new ObservableCollection<Models.AvailableTimes>();
        public ObservableCollection<Models.AvailableTimes> ToTimes { get; private set; } = new ObservableCollection<Models.AvailableTimes>();

        public DateTime CurrentCalendarMonthYear { get; set; } = DateTime.Now;
        public string ToTime { get; set; }
        public int? ToTimeTotalMinutes { get; private set; }
        public string FromTime { get; set; }
        public int? FromTimeTotalMinutes { get; private set; }

        public string TotalAmount
        {
            get => totalAmount;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    totalAmount = "$0.00";
                else if (!value.StartsWith("$", StringComparison.InvariantCulture))
                    totalAmount = "$" + value;
                else
                    totalAmount = value;
                if (double.TryParse(value?.Replace("$", ""), out double d))
                {
                    TotalAmountNum = d;
                    if (d < 0.01)
                    {
                        NotApplicable = true;
                        RequestPayment = false;
                        MarkAsPaid = false;
                    }
                    else
                    {
                        NotApplicable = false;
                        RequestPayment = true;
                        MarkAsPaid = false;
                    }
                }
            }
        }
        public double TotalAmountNum { get; set; }
        public double SecurityDeposit { get; set; }
        public ObservableCollection<Models.BuildingAmenity> Amenities { get; set; } = new ObservableCollection<Models.BuildingAmenity>();
        public ObservableCollection<Models.PMCBuilding> Buildings { get; set; } = new ObservableCollection<Models.PMCBuilding>();
        public ObservableCollection<BuildingUnit> Units { get; set; } = new ObservableCollection<BuildingUnit>();
        public ObservableCollection<UnitTenant> Tenants { get; set; } = new ObservableCollection<UnitTenant>();

        [AlsoNotifyFor("SelectedDateString")]
        public DateTime? SelectedDate
        {
            get => selectedDate;
            set
            {

                //close the date picker after selection is made
                if (ListOfAvailableDays.Any(t => t.Date == value))
                {
                    selectedDate = value;
                    DatePickerIsVisible = false;
                    PopulateFromTimes();
                }

            }
        }

        private void PopulateFromTimes()
        {
            if (SelectedDate.HasValue && ListOfAvailableTimes != null && ListOfAvailableTimes.Any())
            {
                var times = ListOfAvailableTimes?.FirstOrDefault(t => t.Date == SelectedDate)?.TimeRanges?.Where(t => t.BookedBy is null);
                TimeSpan startTime = new TimeSpan(0, 0, 0);
                int maxTime = 48;
                var now = DateTime.Now.Date;
                if (SelectedDate.Value.Date == now.Date)
                {

                    startTime.Add(TimeSpan.FromHours(now.Hour));
                    if (DateTime.Now.Minute / 30d > 1)
                        startTime.Add(TimeSpan.FromHours(1));
                    var endOfDay = TimeSpan.FromMinutes(1380 + 30);
                    var diff = endOfDay.TotalMinutes - new TimeSpan(now.Hour, now.Minute, 0).TotalMinutes;
                    maxTime = (int)(diff / 30d);

                }
                if (times != null)
                {
                    for (int i = 0; i < maxTime; i++)
                    {
                        var extraMinutes = new TimeSpan(0, 30 * i, 0);
                        var time = startTime.Add(extraMinutes);
                        bool available = false;
                        foreach (var t in times)
                        {
                            available = time.TotalMinutes >= t.From && time.TotalMinutes <= t.To;
                        }
                        FromTimes.Add(new Models.AvailableTimes { Time = time, IsAvailable = available });
                    }
                }

            }
        }


        private void PopulateToTimes()
        {
            if (SelectedDate.HasValue && ListOfAvailableTimes != null && ListOfAvailableTimes.Any())
            {
                if (FromTimeTotalMinutes is null)
                    return;
                var times = ListOfAvailableTimes?.FirstOrDefault(t => t.Date == SelectedDate)?.TimeRanges?.Where(t => t.BookedBy is null);
                //to time will be at least 30 minutes after from time
                TimeSpan startTime = new TimeSpan(0, FromTimeTotalMinutes.Value + 30, 0);
                int maxTime = 48;
                if (times != null)
                {
                    for (int i = 0; i < maxTime; i++)
                    {
                        var extraMinutes = new TimeSpan(0, 30 * i, 0);
                        var time = startTime.Add(extraMinutes);
                        bool available = false;
                        foreach (var t in times)
                        {
                            available = time.TotalMinutes >= t.From && time.TotalMinutes <= t.To;
                        }
                        ToTimes.Add(new Models.AvailableTimes { Time = time, IsAvailable = available });
                    }
                }

            }
        }



        public string SelectedDateString => SelectedDate.HasValue ? SelectedDate.Value.ToString("MMM d, yyyy") : "Select date";
        public bool DatePickerIsVisible { get; set; }
        public bool FromTimeListIsVisible { get; private set; }
        public bool ToTimeListIsVisible { get; private set; }

        [AlsoNotifyFor("SelectedAmenityName")]
        public Models.BuildingAmenity SelectedAmenity
        {
            get => selectedAmenity;
            set
            {
                selectedAmenity = value;
                if (value != null)
                    OnAmenitySelected.Execute(value);
            }
        }
        [AlsoNotifyFor("SelectedBuildingName")]
        public Models.PMCBuilding SelectedBuilding
        {
            get => selectedBuilding;
            set
            {
                if (value != null && value.BuildingId == selectedBuilding?.BuildingId)
                    return;
                selectedBuilding = value;
                if (value != null)
                    OnBuildingSelected.Execute(value);

            }
        }
        [AlsoNotifyFor("SelectedUnitName")]
        public BuildingUnit SelectedUnit
        {
            get => selectedUnit;
            set
            {
                selectedUnit = value;
                if (value != null)
                    OnUnitSelected.Execute(value);
                else
                    OtherTextFieldIsVisible = false;

            }
        }
        public string SelectedAmenityName => SelectedAmenity?.Name;
        public string SelectedBuildingName => SelectedBuilding?.BuildingDescription;
        public string SelectedUnitName => SelectedUnit?.Unit;

        public bool BuildingPickerIsVisible { get; set; }

        public override void Init(object initData)
        {
            base.Init(initData);
            TotalAmount = $"0.00";
            FromTime = "Select";
            ToTime = "Select";
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            var pmcInfo = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetPMCInfo();
            var pmcBuildings = pmcInfo.Item1.BuildingsAccess;
            if (pmcBuildings != null && pmcBuildings.Any(b => b.Amenities != null && b.Amenities.Any()))
                Buildings = new ObservableCollection<Models.PMCBuilding>(pmcBuildings.Where(t => t.Amenities != null && t.Amenities.Any()).Select(t => new Models.PMCBuilding
                {
                    Amenities = t.Amenities,
                    BuildingDescription = t.BuildingDescription,
                    BuildingId = t.BuildingId,
                    IsSelected = false
                }));
            SelectedBuilding = Buildings?.FirstOrDefault(t => t.Amenities != null && t.Amenities.Any());
            if (SelectedBuilding != null)
                SelectedBuilding.IsSelected = true;
        }
        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            return;
        }

        public FreshAwaitCommand OnBuildingButtonTapped => new FreshAwaitCommand((tcs) =>
        {
            BuildingPickerIsVisible = true;
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnAmenitySelected => new FreshAwaitCommand(async (par, tcs) =>
        {
            var amenity = (Amenity)par;
            var amenityWithDetails = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetAmenity(amenity.Id, SelectedBuilding.BuildingId);
            amenity.Rules = amenityWithDetails.Rules;
            SecurityDeposit = amenity.Rules.SecurityDepositAmount;
            var from = new DateTime(CurrentCalendarMonthYear.Year, CurrentCalendarMonthYear.Month, 1);
            var availableTimes = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetAvailableDays(new MGDataAccessLibrary.Models.Amenities.Requests.AvailableDays
            {
                BuildingId = SelectedBuilding.BuildingId,
                //  UnitId = selectedUnit.Id,
                From = from.ToString(),
                To = from.AddDays(31).ToString(),
            }, SelectedAmenity.Id);
            if (availableTimes.AvailableDaysAndTimes.Any())
            {
                ListOfAvailableTimes = new ObservableCollection<AvailableDaysAndTimes>(availableTimes.AvailableDaysAndTimes);
                ListOfAvailableDays = new ObservableCollection<DateTime>(availableTimes.AvailableDaysAndTimes.Where(t => t.TimeRanges.Any(t => t.BookedBy is null)).Select(t => t.Date));
                PopulateFromTimes();
                tcs?.SetResult(true);
            }

        });

        public FreshAwaitCommand HandleCalendarNavigation
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var date = (DateTime)par;

                    CurrentCalendarMonthYear = date;
                    if (SelectedAmenity != null && SelectedBuilding != null)
                    {
                        //get available times if an amenity is already selected
                        var availableTimes = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetAvailableDays(new MGDataAccessLibrary.Models.Amenities.Requests.AvailableDays
                        {
                            BuildingId = SelectedBuilding.BuildingId,
                            //  UnitId = selectedUnit.Id,
                            From = new DateTime(date.Year, date.Month, 1).ToString(),
                            To = date.AddDays(31).ToString(),
                        }, SelectedAmenity.Id);
                        if (availableTimes.AvailableDaysAndTimes.Any())
                        {
                            ListOfAvailableDays = new ObservableCollection<DateTime>(availableTimes.AvailableDaysAndTimes.Where(t => t.TimeRanges.Any()).Select(t => t.Date));
                            ListOfAvailableTimes = new ObservableCollection<AvailableDaysAndTimes>(availableTimes.AvailableDaysAndTimes);
                            PopulateFromTimes();

                        }

                    }

                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnBuildingSelected
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var building = (Models.PMCBuilding)par;
                    SelectedUnit = null;

                    int selectedAmenityId = default;
                    if (SelectedAmenity != null && SelectedBuilding.Amenities.Select(t => t.Id).Contains(SelectedAmenity.Id))
                    {
                        selectedAmenityId = SelectedBuilding.Amenities.First(t => t.Id == SelectedAmenity.Id).Id;
                        SelectedAmenity.IsSelected = true;
                    }
                    else
                        SelectedAmenity = null;
                    if (building.Amenities != null && building.Amenities.Any())
                        Amenities = new ObservableCollection<Models.BuildingAmenity>(building.Amenities.Select(t => new Models.BuildingAmenity
                        {
                            Id = t.Id,
                            IsSelected = false,
                            Status = t.Status,
                            Rules = t.Rules,
                            Name = t.Name
                        }));
                    if (SelectedBuilding is null)
                        return;
                    if (SelectedBuilding != null)
                    {
                        var units = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetBuildingUnits(SelectedBuilding.BuildingId);
                        if (units != null && units.Any())
                            Units = new ObservableCollection<BuildingUnit>(units);
                    }
                    Tenants.Clear();
                    SelectedAmenity = Amenities?.FirstOrDefault(t => t.Id == selectedAmenityId);
                    foreach (var a in Buildings)
                    {
                        a.IsSelected = false;
                    }
                    building.IsSelected = !building.IsSelected;

                    BuildingPickerIsVisible = false;
                    if (SelectedAmenity != null && SelectedBuilding != null)
                    {
                        //get available times if an amenity is already selected
                        var from = new DateTime(CurrentCalendarMonthYear.Year, CurrentCalendarMonthYear.Month, 1);
                        var availableTimes = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetAvailableDays(new MGDataAccessLibrary.Models.Amenities.Requests.AvailableDays
                        {
                            BuildingId = SelectedBuilding.BuildingId,
                            //  UnitId = selectedUnit.Id,
                            From = from.ToString(),
                            To = from.AddDays(31).ToString(),
                        }, SelectedAmenity.Id);
                        if (availableTimes.AvailableDaysAndTimes.Any())
                        {
                            ListOfAvailableDays = new ObservableCollection<DateTime>(availableTimes.AvailableDaysAndTimes.Where(t => t.TimeRanges.Any(t => t.BookedBy is null)).Select(t => t.Date));
                            ListOfAvailableTimes = new ObservableCollection<AvailableDaysAndTimes>(availableTimes.AvailableDaysAndTimes);
                            PopulateFromTimes();
                        }

                    }
                    tcs?.SetResult(true);
                });
            }
        }



        public FreshAwaitCommand OnUnitSelected
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var unit = (BuildingUnit)par;
                    Tenants = new ObservableCollection<UnitTenant>();
                    try
                    {
                        var tenants = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetUnitTenants(unit.Id);

                        foreach (var tenant in tenants)
                            Tenants.Add(tenant);

                        ListViewIsVisible = true;
                        tcs?.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        //For some odd reason the API returns 404 if unit is found but no tenants in it (instead of empty tenants array).
                        //maybe same 404 error is retuned if unit is not found either.
                        //But for tenants the error is 404 with message "Tenants not found"
                        //May change later
                        if (ex.Message.ToLower() != "tenants not found")
                            await CoreMethods.DisplayAlert("ManageGo", ex.Message, "Dismiss");
                    }
                    Tenants.Add(new UnitTenant { FirstName = "Other", Id = default });
                    if (Tenants.Count == 1)
                    {
                        Tenants.First().IsSelected = true;
                        OtherTextFieldIsVisible = true;
                    }

                    var fontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
                    TenantListHeight = Tenants.Count * (12 + Math.Max(fontSize, 25));
                });
            }
        }

        public FreshAwaitCommand OnTenantTapped
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var tenant = (UnitTenant)par;
                    OtherTextFieldIsVisible = tenant.Id == default;
                    foreach (var _tenant in Tenants)
                        _tenant.IsSelected = false;
                    tenant.IsSelected = true;
                    NotApplicable = false;
                    MarkAsPaid = false;
                    RequestPayment = false;
                    if (tenant.Id == default && !MarkAsPaid)
                    {
                        NotApplicable = true;
                    }
                    else if (!MarkAsPaid)
                        RequestPayment = true;
                    FromTimeListIsVisible = false;
                    ToTimeListIsVisible = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnSelectDateTapped => new FreshAwaitCommand(async (par, tcs) =>
              {
                  if (SelectedAmenity is null || SelectedBuilding is null || SelectedUnit is null)
                  {
                      await CoreMethods.DisplayAlert("Unable to select a date", "Please select a building, unit and amenity first", "OK");
                      tcs?.SetResult(true);
                      DatePickerIsVisible = false;
                      App.MasterDetailNav.IsGestureEnabled = true;
                      return;
                  }
                  DatePickerIsVisible = !DatePickerIsVisible;
                  FromTimeListIsVisible = false;
                  ToTimeListIsVisible = false;
                  App.MasterDetailNav.IsGestureEnabled = !DatePickerIsVisible;
                  tcs?.SetResult(true);
              });

        public FreshAwaitCommand OnRequestPaymentTapped => new FreshAwaitCommand((par, tcs) =>
                {
                    if (TotalAmountNum < 0.001)
                        return;
                    NotApplicable = false;
                    MarkAsPaid = false;
                    RequestPayment = true;
                    tcs?.SetResult(true);
                });

        public FreshAwaitCommand OnMarkAsPaidTapped => new FreshAwaitCommand((par, tcs) =>
        {
            if (TotalAmountNum < 0.001)
                return;
            NotApplicable = false;
            RequestPayment = false;
            MarkAsPaid = true;
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnNotApplicableTapped => new FreshAwaitCommand((par, tcs) =>
        {
            MarkAsPaid = false;
            RequestPayment = false;
            NotApplicable = true;
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnApproveBookingTapped => new FreshAwaitCommand((par, tcs) =>
        {

            ApproveBooking = !ApproveBooking;
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnToTimeLabelTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    FromTimeListIsVisible = false;
                    if (!FromTimeTotalMinutes.HasValue)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Select booking start time first", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    ToTimeListIsVisible = !ToTimeListIsVisible;
                    tcs?.SetResult(true);
                });
            }
        }


        public FreshAwaitCommand OnFromTimeLabelTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    ToTimeListIsVisible = false;
                    if (SelectedAmenity is null || SelectedDate is null)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please select amenity and date of booking first", "OK");
                        tcs?.SetResult(true);
                        return;
                    }

                    FromTimeListIsVisible = !FromTimeListIsVisible;
                    tcs?.SetResult(true);
                });
            }
        }


        public FreshAwaitCommand OnFromTimeSelected
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    FromTimeListIsVisible = false;
                    var time = (Models.AvailableTimes)par;
                    var dateTime = new DateTime(time.Time.Ticks);
                    FromTime = dateTime.ToString("h:mm tt", CultureInfo.InvariantCulture);
                    FromTimeTotalMinutes = (int)time.Time.TotalMinutes;
                    PopulateToTimes();
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnToTimeSelected
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    ToTimeListIsVisible = false;
                    var time = (Models.AvailableTimes)par;
                    var dateTime = new DateTime(time.Time.Ticks);
                    ToTime = dateTime.ToString("h:mm tt", CultureInfo.InvariantCulture);
                    ToTimeTotalMinutes = (int)time.Time.TotalMinutes;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnSaveBookingTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {

                    var _timeFrom = DateTime.Parse(FromTime);
                    var _timeTo = DateTime.Parse(ToTime);
                    int? tenantId = Tenants?.FirstOrDefault(t => t.IsSelected)?.Id;
                    if (tenantId == 0)
                    {
                        tenantId = null;
                    }
                    else if (tenantId.HasValue)
                        OtherNameText = null;
                    if (!double.TryParse(TotalAmount.Replace("$", ""), out double d))
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please enter valid booking fee amount.", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    if (_timeFrom >= _timeTo)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Booking end time must be later that start time.", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    if (!SelectedDate.HasValue)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please select a date.", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    if (selectedBuilding is null)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please select a building.", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    if (SelectedUnit is null)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please select a unit.", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    var paymentStatus = MGDataAccessLibrary.Models.Amenities.Requests.PaymentStatus.NotApplicable;
                    if (MarkAsPaid)
                        paymentStatus = MGDataAccessLibrary.Models.Amenities.Requests.PaymentStatus.Paid;
                    else if (RequestPayment)
                        paymentStatus = MGDataAccessLibrary.Models.Amenities.Requests.PaymentStatus.Requested;
                    var request = new MGDataAccessLibrary.Models.Amenities.Requests.CreateBooking
                    {
                        AmenityId = selectedAmenity.Id,
                        BookingFee = d,
                        BuildingId = SelectedBuilding.BuildingId,
                        FromDate = new DateTime(SelectedDate.Value.Year, SelectedDate.Value.Month, SelectedDate.Value.Day, _timeFrom.Hour, _timeFrom.Minute, 0, DateTimeKind.Unspecified),
                        ToDate = new DateTime(SelectedDate.Value.Year, SelectedDate.Value.Month, SelectedDate.Value.Day, _timeTo.Hour, _timeTo.Minute, 0, DateTimeKind.Unspecified),
                        Note = NoteToTenant,
                        Status = ApproveBooking,
                        ProvidedTenantName = OtherNameText,
                        TenantId = tenantId,
                        UnitId = SelectedUnit.Id,
                        PaymentStatus = paymentStatus
                    };
                    try
                    {
                        await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.CreateBooking(request);
                        await CoreMethods.DisplayAlert("Success!", $"{SelectedAmenity.Name} booked for {SelectedDateString} from {FromTime} until {ToTime}", "OK");
                        await CoreMethods.PopPageModel();
                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", ex.Message, "Dismiss");
                    }

                    tcs?.SetResult(true);
                }, () => SelectedDate != null && FromTime != "Select" && ToTime != "Select" && SelectedAmenity != null && SelectedBuilding != null);
            }
        }




        public FreshAwaitCommand OnBackButtonTapped
        {
            get
            {

                async void execute(TaskCompletionSource<bool> tcs)
                {
                    await CoreMethods.PopPageModel(modal: false);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }


    }
}
