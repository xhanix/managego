using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomCalendar;
using FreshMvvm;
using Xamarin.Forms;
using PropertyChanged;
using System.Linq;
using Xamarin.Essentials;
using System.Threading;
using Microsoft.AppCenter.Crashes;
using System.Collections.ObjectModel;

namespace ManageGo
{

    internal class MaintenanceTicketsPageModel : BaseDetailPage
    {
        DateRange dateRange;
        public bool NothingFetched { get; private set; }
        public TicketRequestItem CurrentFilter { get; private set; }
        public TicketRequestItem ParameterItem { get; set; }
        public ObservableCollection<MaintenanceTicket> FetchedTickets { get; set; }
        public List<Building> Buildings { get; private set; }
        [AlsoNotifyFor("SelectedCategoriesString")]
        public List<Categories> Categories { get; private set; }
        [AlsoNotifyFor("SelectedTagsString")]
        public List<Tags> Tags { get; private set; }
        [AlsoNotifyFor("SelectedUsersString")]
        public List<User> Users { get; private set; }
        int LastLoadedItemId { get; set; }
        bool CanGetMorePages { get; set; }
        #region filter view properties
        [AlsoNotifyFor("LowPriorityCheckBoxImage", "SelectedPriorityString")]
        public bool IsLowPriorityFilterSelected { get; private set; }
        [AlsoNotifyFor("MediumPriorityCheckBoxImage", "SelectedPriorityString")]
        public bool IsMediumPriorityFilterSelected { get; private set; }
        [AlsoNotifyFor("HighPriorityCheckBoxImage", "SelectedPriorityString")]
        public bool IsHighPriorityFilterSelected { get; private set; }
        [AlsoNotifyFor("OpenTicketFilterCheckBoxImage", "SelectedStatusFlagsString")]
        public bool SelectedOpenTicketsFilter { get; private set; }
        [AlsoNotifyFor("ClosedTicketFilterCheckBoxImage", "SelectedStatusFlagsString")]
        public bool SelectedClosedTicketsFilter { get; private set; }
        public string FilterKeywords { get; set; }
        public string ClosedTicketFilterCheckBoxImage
        {
            get => SelectedClosedTicketsFilter ? "checked.png" : "unchecked.png";
        }
        public string OpenTicketFilterCheckBoxImage
        {
            get => SelectedOpenTicketsFilter ? "checked.png" : "unchecked.png";
        }
        public string HighPriorityCheckBoxImage
        {
            get => IsHighPriorityFilterSelected ? "checked.png" : "unchecked.png";
        }
        public string MediumPriorityCheckBoxImage
        {
            get => IsMediumPriorityFilterSelected ? "checked.png" : "unchecked.png";
        }
        public string LowPriorityCheckBoxImage
        {
            get => IsLowPriorityFilterSelected ? "checked.png" : "unchecked.png";
        }
        public string SelectedStatusFlagsString
        {
            get => (SelectedOpenTicketsFilter && SelectedClosedTicketsFilter) || (!SelectedOpenTicketsFilter && !SelectedClosedTicketsFilter) ? "All" : SelectedOpenTicketsFilter ? "Open" : "Closed";
        }
        public string SelectedPriorityString
        {
            get
            {
                if ((IsLowPriorityFilterSelected && IsMediumPriorityFilterSelected && IsHighPriorityFilterSelected)
                   || (!IsLowPriorityFilterSelected && !IsMediumPriorityFilterSelected && !IsHighPriorityFilterSelected))
                    return "All";

                List<string> result = new List<string>();
                if (IsHighPriorityFilterSelected)
                    result.Add("High");
                if (IsMediumPriorityFilterSelected)
                    result.Add("Medium");
                if (IsLowPriorityFilterSelected)
                    result.Add("Low");
                return string.Join(", ", result);
            }
        }
        public string SelectedCategoriesString
        {
            get
            {
                return Categories is null || !Categories.Any(t => t.IsSelected)
                                                        || Categories.Count() == Categories.Count(t => t.IsSelected)
                    ? "All"
                    :
                    Categories.Count(t => t.IsSelected) > 1 ?
                                                        $"{Categories.First(t => t.IsSelected).CategoryName}, {Categories.Count(t => t.IsSelected) - 1} more"
                                                            :
                                                        Categories.First(t => t.IsSelected).CategoryName
                                                        ;
            }
        }
        public string SelectedUsersString
        {
            get
            {
                return Users is null || !Users.Any(t => t.IsSelected)
                                              || Users.Count() == Users.Count(t => t.IsSelected)
                    ? "All"
                                                  : string.Join(", ", Users.Where(t => t.IsSelected).Select(t => t.UserFullName));
            }
        }
        public string SelectedTagsString
        {
            get
            {
                return Tags is null || !Tags.Any(t => t.IsSelected)
                                            || Tags.Count() == Tags.Count(t => t.IsSelected)
                    ? "All"
                    : Tags.Count(t => t.IsSelected) > 1 ?
                                            $"{Tags.First(t => t.IsSelected).TagName}, {Tags.Count(t => t.IsSelected) - 1} more"
                                                :
                                            Tags.First(t => t.IsSelected).TagName;
            }
        }
        public string SelectedBuildingsString
        {
            get
            {
                return Buildings is null || !Buildings.Any(t => t.IsSelected) ||
                                                      Buildings.Count() == Buildings.Count(t => t.IsSelected) ?
                                                      "All" :
                                                      Buildings.Count(t => t.IsSelected) > 1 ?
                                                      $"{Buildings.First(t => t.IsSelected).BuildingName}, {Buildings.Count(t => t.IsSelected) - 1} more"
                                                          : $"{Buildings.First(t => t.IsSelected).BuildingName}";
            }
        }
        public bool FilterBuildingsExpanded { get; private set; }
        public bool FilterPrioritiesExpanded { get; private set; }
        public bool FilterCategoriesExpanded { get; private set; }
        public bool FilterTagsExpanded { get; private set; }
        public bool FilterStatusExpanded { get; private set; }
        public bool FilterDueDateExpanded { get; private set; }
        public bool FilterUsersExpanded { get; private set; }
        public string FilterDueDateString
        {
            get
            {
                return FilterDueDate != null ?
                    FilterDueDate.EndDate.HasValue ? FilterDueDate.StartDate.ToShortDateString() + "-" + FilterDueDate.EndDate.Value.ToShortDateString()
                                     :
                    FilterDueDate.StartDate.ToShortDateString() : "All";
            }
        }

        #endregion
        #region view properties
        [AlsoNotifyFor("AddNewButtonIsVisisble")]
        internal bool CalendarIsShown { get; set; }
        [AlsoNotifyFor("AddNewButtonIsVisisble")]
        public bool FilterSelectViewIsShown { get; set; }
        public bool BackbuttonIsVisible { get; private set; }

        public string NumberOfAppliedFilters { get; internal set; } = " ";
        public bool ListIsEnabled { get; set; } = false;
        public bool IsRefreshingList { get; set; }
        public bool AddNewButtonIsVisisble
        {
            get => !(FilterSelectViewIsShown || CalendarIsShown);
        }
        public View PopContentView { get; internal set; }
        #endregion

        [AlsoNotifyFor("FilterDueDateString")]
        public DateRange FilterDueDate { get; set; }




        [AlsoNotifyFor("CalendarButtonText")]
        internal DateRange DateRange
        {
            get
            {
                return dateRange is null ? new DateRange(DateTime.Today, DateTime.Today.AddDays(-30)) : dateRange;
            }
            set
            {
                dateRange = value;
            }
        }
        public string CalendarButtonText
        {
            get
            {
                if (DateRange.EndDate.HasValue)
                {
                    if (DateRange.StartDate == DateRange.EndDate)
                        return DateRange.StartDate.ToString("MMM-dd");
                    else
                        return DateRange.StartDate.ToString("MMM dd") + " - " + DateRange.EndDate.Value.ToString("MMM dd");
                }
                else
                {
                    return DateRange.StartDate.ToString("MMM-dd");
                }

            }
        }


        public FreshAwaitCommand OnBackbuttonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    await CoreMethods.PopPageModel(modal: CurrentPage.Navigation.ModalStack.Contains(CurrentPage), animate: false);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            if (ShowingTicketDetails && !RequestedTicketDetails)
            {
                ShowingTicketDetails = false;
                return;
            }
            NothingFetched = false;
            try
            {
                if (FetchNextPage)
                {
                    if (ParameterItem is null)
                    {
                        ParameterItem = new TicketRequestItem
                        {
                            TicketStatus = TicketStatus.Open,
                            DateFrom = DateRange.StartDate,
                            DateTo = DateRange.EndDate
                        };
                    }
                    ParameterItem.Page++;
                    var nextPage = await Services.DataAccess.GetTicketsAsync(ParameterItem);
                    if (nextPage != null)
                    {
                        foreach (var item in nextPage)
                        {
                            if (FetchedTickets is null)
                                FetchedTickets = new ObservableCollection<MaintenanceTicket>();
                            FetchedTickets.Add(item);
                        }
                    }
                    CanGetMorePages = nextPage != null && nextPage.Count() == ParameterItem.PageSize;
                    var lastIdx = FetchedTickets.IndexOf(FetchedTickets.Last());
                    var index = Math.Floor(lastIdx / 2d);
                    var markedItem = FetchedTickets.ElementAt((int)index);
                    LastLoadedItemId = markedItem.TicketId;
                }
                else
                {
                    HasLoaded = false;
                    if (ParameterItem is null)
                    {
                        ParameterItem = new TicketRequestItem
                        {
                            TicketStatus = TicketStatus.Open,
                            DateFrom = DateRange.StartDate,
                            DateTo = DateRange.EndDate,
                        };
                    }
                    if (refreshData)
                        ParameterItem.Page = 1;

                    // FetchedTickets is null on view init
                    var fetchedTickets = await Services.DataAccess.GetTicketsAsync(ParameterItem);
                    if (fetchedTickets != null)
                        FetchedTickets = new ObservableCollection<MaintenanceTicket>(fetchedTickets);
                    else
                        FetchedTickets = new ObservableCollection<MaintenanceTicket>();

                    CanGetMorePages = FetchedTickets != null && FetchedTickets.Count == ParameterItem?.PageSize;
                    if (FetchedTickets != null && FetchedTickets.Any() && CanGetMorePages)
                    {

                        var lastIdx = FetchedTickets.IndexOf(FetchedTickets.Last());
                        var index = Math.Floor(lastIdx / 2d);
                        var markedItem = FetchedTickets.ElementAt((int)index);
                        LastLoadedItemId = markedItem.TicketId;
                    }
                    ListIsEnabled = true;
                    HasLoaded = true;

                }
                 ((MaintenanceTicketsPage)CurrentPage).DataLoaded();
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string> { { "Class", GetType().FullName } });
                APIhasFailed = true;
                FetchedTickets = new ObservableCollection<MaintenanceTicket>();
                NothingFetched = true;
                if (await CoreMethods.DisplayAlert("Something went wrong", $"Unable to get tickets. Error Message: {ex.Message}", "Try again", "Dismiss"))
                {
                    if (!IsLoading)
                        await this.LoadData();
                }
            }
            finally
            {
                HasLoaded = true;
                if (FetchedTickets is null || !FetchedTickets.Any())
                {
                    NothingFetched = true;
                }
                else
                {
                    NothingFetched = false;
                }
            }
        }



        public FreshAwaitCommand SetFilterStatus
        {
            get
            {
                return new FreshAwaitCommand((parameter, tcs) =>
                {
                    var status = (string)parameter;
                    switch (status)
                    {
                        case "Open":
                            SelectedOpenTicketsFilter = !SelectedOpenTicketsFilter;
                            break;
                        case "Closed":
                            SelectedClosedTicketsFilter = !SelectedClosedTicketsFilter;
                            break;
                        default:
                            break;
                    }
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand ShowDetails
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    var ticket = (MaintenanceTicket)par;
                    var alreadyExpandedTicket = FetchedTickets.FirstOrDefault(t => t.FirstCommentShown && t.TicketId != ticket.TicketId);
                    if (alreadyExpandedTicket != null)
                    {
                        alreadyExpandedTicket.FirstCommentShown = false;
                    }
                    ticket.FirstCommentShown = !ticket.FirstCommentShown;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnAddButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    await CoreMethods.PushPageModel<CreateTicketPageModel>();
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand SetFilterPriority
        {
            get
            {
                return new FreshAwaitCommand((parameter, tcs) =>
                {
                    var level = (string)parameter;
                    switch (level)
                    {
                        case "Low":
                            IsLowPriorityFilterSelected = !IsLowPriorityFilterSelected;
                            break;
                        case "Medium":
                            IsMediumPriorityFilterSelected = !IsMediumPriorityFilterSelected;
                            break;
                        case "High":
                            IsHighPriorityFilterSelected = !IsHighPriorityFilterSelected;
                            break;
                        default:
                            break;
                    }

                });
            }
        }

        public FreshAwaitCommand OnFilterTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    if (FilterSelectViewIsShown)
                    {
                        PopContentView = null;
                        ListIsEnabled = true;
                        App.MasterDetailNav.IsGestureEnabled = true;
                    }
                    else
                    {
                        App.MasterDetailNav.IsGestureEnabled = false;
                        var _view = new TicketFilterSelectView(bindingContext: this);
                        CurrentFilter = ParameterItem?.Clone();
                        PopContentView = _view.Content;
                        ListIsEnabled = false;
                    }
                    FilterSelectViewIsShown = !FilterSelectViewIsShown;
                    tcs?.SetResult(true);
                }, () => !CalendarIsShown);
            }
        }

        public FreshAwaitCommand OnBuildingTapped
        {
            get
            {
                return new FreshAwaitCommand((parameter, tcs) =>
                {
                    var building = parameter as Building;
                    building.IsSelected = !building.IsSelected;
                    RaisePropertyChanged("SelectedBuildingsString");

                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnPulledToRefresh
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    IsRefreshingList = true;
                    await LoadData(refreshData: true, FetchNextPage: false);
                    IsRefreshingList = false;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnCategoryTapped
        {
            get
            {
                return new FreshAwaitCommand((parameter, tcs) =>
                {
                    var category = parameter as Categories;
                    category.IsSelected = !category.IsSelected;
                    RaisePropertyChanged("SelectedCategoriesString");
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnUserTapped
        {
            get
            {
                return new FreshAwaitCommand((parameter, tcs) =>
                {
                    var user = parameter as User;
                    user.IsSelected = !user.IsSelected;
                    RaisePropertyChanged("SelectedUsersString");
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnTagTapped
        {
            get
            {
                return new FreshAwaitCommand((parameter, tcs) =>
                {
                    var tag = parameter as Tags;
                    tag.IsSelected = !tag.IsSelected;
                    RaisePropertyChanged("SelectedTagsString");
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnCloseFliterViewTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    PopContentView = null;
                    FilterSelectViewIsShown = false;
                    //set previous filter values if exists
                    if (CurrentFilter != null)
                    {
                        if (Buildings != null && CurrentFilter.Buildings != null)
                        {
                            foreach (var b in Buildings)
                            {
                                if (CurrentFilter.Buildings.Contains(b.BuildingId))
                                    b.IsSelected = true;
                                else
                                    b.IsSelected = false;
                            }
                        }
                        if (Users != null && CurrentFilter.Assigned != null)
                        {
                            foreach (var u in Users)
                            {
                                if (CurrentFilter.Assigned.Contains(u.UserID))
                                    u.IsSelected = true;
                                else
                                    u.IsSelected = false;
                            }
                        }
                        if (Tags != null && CurrentFilter.Tags != null)
                        {
                            foreach (var t in Tags)
                            {
                                if (CurrentFilter.Tags.Contains(t.TagID))
                                    t.IsSelected = true;
                                else
                                    t.IsSelected = false;
                            }
                        }
                        if (Categories != null && CurrentFilter.Categories != null)
                        {
                            foreach (var c in Categories)
                            {
                                if (CurrentFilter.Categories.Contains(c.CategoryID))
                                    c.IsSelected = true;
                                else
                                    c.IsSelected = false;
                            }
                        }

                        if (CurrentFilter.DueDateFrom.HasValue && CurrentFilter.DueDateTo.HasValue)
                            FilterDueDate = new DateRange(CurrentFilter.DueDateFrom.Value, CurrentFilter.DueDateTo.Value);
                        if (CurrentFilter.Priorities != null)
                        {
                            IsLowPriorityFilterSelected = CurrentFilter.Priorities.Contains(TicketPriorities.Low);
                            IsMediumPriorityFilterSelected = CurrentFilter.Priorities.Contains(TicketPriorities.Medium);
                            IsHighPriorityFilterSelected = CurrentFilter.Priorities.Contains(TicketPriorities.High);
                        }

                        if (CurrentFilter.DateFrom.HasValue && CurrentFilter.DateTo.HasValue)
                            DateRange = new DateRange(CurrentFilter.DateFrom.Value, CurrentFilter.DateTo.Value);
                        else if (CurrentFilter.DateFrom.HasValue && !CurrentFilter.DateTo.HasValue)
                            DateRange = new DateRange(CurrentFilter.DateFrom.Value);
                        if (CurrentFilter.TicketStatus.HasValue)
                        {
                            SelectedOpenTicketsFilter = CurrentFilter.TicketStatus.Value == TicketStatus.Open;
                            SelectedClosedTicketsFilter = CurrentFilter.TicketStatus.Value == TicketStatus.Closed;
                        }

                        FilterKeywords = CurrentFilter.Search;
                    }
                    ListIsEnabled = true;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnResetFiltersButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    PopContentView = null;
                    FilterSelectViewIsShown = false;
                    ParameterItem = null;
                    CurrentFilter = null;
                    NumberOfAppliedFilters = string.Empty;
                    FetchedTickets = null;
                    await LoadData(true, false);

                    if (Buildings != null)
                    {
                        foreach (var b in Buildings)
                        {
                            b.IsSelected = false;
                        }
                    }
                    if (Tags != null)
                    {
                        foreach (var b in Tags)
                        {
                            b.IsSelected = false;
                        }
                    }
                    if (Users != null)
                    {
                        foreach (var b in Users)
                        {
                            b.IsSelected = false;
                        }
                    }
                    IsLowPriorityFilterSelected = false;
                    IsMediumPriorityFilterSelected = false;
                    IsHighPriorityFilterSelected = false;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnApplyFiltersTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    PopContentView = null;
                    FilterSelectViewIsShown = false;
                    ParameterItem = new TicketRequestItem();
                    if (!string.IsNullOrWhiteSpace(FilterKeywords))
                        ParameterItem.Search = FilterKeywords;
                    else
                    {
                        if (Buildings != null && Buildings.Any(f => f.IsSelected))
                            ParameterItem.Buildings = Buildings.Where(f => f.IsSelected).Select(f => f.BuildingId).ToList();
                        if (Tags != null && Tags.Any(f => f.IsSelected))
                            ParameterItem.Tags = Tags.Where(t => t.IsSelected).Select(t => t.TagID).ToList();
                        if (SelectedOpenTicketsFilter && SelectedClosedTicketsFilter)
                            ParameterItem.TicketStatus = TicketStatus.All;
                        else if (SelectedOpenTicketsFilter)
                            ParameterItem.TicketStatus = TicketStatus.Open;
                        else if (SelectedClosedTicketsFilter)
                            ParameterItem.TicketStatus = TicketStatus.Closed;
                        else
                            ParameterItem.TicketStatus = TicketStatus.Open;
                        if (IsLowPriorityFilterSelected || IsMediumPriorityFilterSelected || IsHighPriorityFilterSelected)
                        {
                            List<TicketPriorities> priorities = new List<TicketPriorities>();
                            if (IsLowPriorityFilterSelected)
                                priorities.Add(TicketPriorities.Low);
                            if (IsMediumPriorityFilterSelected)
                                priorities.Add(TicketPriorities.Medium);
                            if (IsHighPriorityFilterSelected)
                                priorities.Add(TicketPriorities.High);
                            ParameterItem.Priorities = priorities;
                        }
                        if (Users != null && Users.Any(t => t.IsSelected))
                            ParameterItem.Assigned = Users.Where(t => t.IsSelected).Select(t => t.UserID).ToList();
                        if (Categories != null && Categories.Any(t => t.IsSelected))
                            ParameterItem.Categories = Categories.Where(t => t.IsSelected).Select(t => t.CategoryID).ToList();
                        ParameterItem.DateFrom = this.DateRange.StartDate;
                        if (this.DateRange.EndDate.HasValue)
                            ParameterItem.DateTo = this.DateRange.EndDate.Value;
                        if (FilterDueDate != null)
                        {
                            ParameterItem.DateFrom = FilterDueDate.StartDate;
                            ParameterItem.DateTo = FilterDueDate.EndDate ?? FilterDueDate.StartDate;
                        }
                    }
                    NumberOfAppliedFilters = ParameterItem.NumberOfAppliedFilters == 0 ? "" : $"{ParameterItem.NumberOfAppliedFilters}";
                    NothingFetched = false;
                    HasLoaded = false;
                    FetchedTickets = new ObservableCollection<MaintenanceTicket>();
                    try
                    {
                        FetchedTickets = new ObservableCollection<MaintenanceTicket>(
                             await Services.DataAccess.GetTicketsAsync(ParameterItem));
                    }
                    catch (Exception ex)
                    {
                        Crashes.TrackError(ex);
                    }
                    finally
                    {
                        HasLoaded = true;
                        if (!FetchedTickets.Any())
                            NothingFetched = true;
                        else
                            NothingFetched = false;
                        ListIsEnabled = true;
                        ((MaintenanceTicketsPage)CurrentPage).DataLoaded();
                        tcs?.SetResult(true);
                    }
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnItemTapped
        {
            get
            {
                async void execute(object item, TaskCompletionSource<bool> tcs)
                {
                    var ticket = (MaintenanceTicket)item;
                    RequestedTicketDetails = true;
                    //get ticket details
                    try
                    {
                        var ticketDetails = await Services.DataAccess.GetTicketDetails(ticket.TicketId);
                        var dic = new Dictionary<string, object>
                            {
                                {"TicketDetails", ticketDetails},
                                {"TicketNumber", ticket.TicketNumber},
                                {"Address", ticket.Building?.BuildingName + " #" + ticket.Unit?.UnitName},
                                {"TicketTitleText", ticket.TicketSubject},
                                {"Ticket", ticket}
                            };
                        ShowingTicketDetails = true;
                        await CoreMethods.PushPageModel<TicketDetailsPageModel>(dic, false, false);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        tcs?.SetResult(true);
                    }
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnCalendarButtonTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    if (!CalendarIsShown)
                    {
                        App.MasterDetailNav.IsGestureEnabled = false;
                        StackLayout container = new StackLayout { Spacing = 0 };
                        Grid buttonContainer = new Grid { Padding = new Thickness(8, 8, 8, 12) };
                        var cancelButton = new Button
                        {
                            Text = "CANCEL",
                            BackgroundColor = Color.Transparent,
                            HorizontalOptions = LayoutOptions.Start,
                            TextColor = Color.Gray
                        };
                        cancelButton.Clicked += (sender, e) => OnCalendarButtonTapped.Execute(null);
                        var applyButton = new Button
                        {
                            Text = "APPLY",
                            BackgroundColor = Color.Transparent,
                            HorizontalOptions = LayoutOptions.End,
                            TextColor = Color.Red
                        };

                        var cal = new Controls.CalendarView
                        {
                            SelectedDates = DateRange,
                            HeightRequest = 245,
                            WidthRequest = 400,
                            HorizontalOptions = LayoutOptions.Center,
                            AllowMultipleSelection = true
                        };
                        async void p(object sender, EventArgs e)
                        {
                            this.DateRange = cal.SelectedDates;
                            OnCalendarButtonTapped.Execute(null);
                            ParameterItem = new TicketRequestItem
                            {
                                DateFrom = DateRange.StartDate,
                                DateTo = DateRange.EndDate
                            };
                            NumberOfAppliedFilters = " ";
                            await LoadData(refreshData: true, FetchNextPage: false);
                        }
                        applyButton.Clicked += p;
                        buttonContainer.Children.Add(cancelButton);
                        buttonContainer.Children.Add(applyButton);
                        container.Children.Add(cal);
                        container.Children.Add(buttonContainer);
                        PopContentView = container;
                        ListIsEnabled = false;
                    }
                    else
                    {
                        App.MasterDetailNav.IsGestureEnabled = true;
                        PopContentView = null;
                        ListIsEnabled = true;
                    }
                    CalendarIsShown = !CalendarIsShown;
                    tcs?.SetResult(true);
                }, () => !FilterSelectViewIsShown);
            }
        }

        public FreshAwaitCommand OnExpandFilterTapped
        {
            get
            {
                return new FreshAwaitCommand((parameter, tcs) =>
                {
                    var par = (string)parameter;
                    switch (par)
                    {
                        case "DueDates":
                            FilterDueDateExpanded = !FilterDueDateExpanded;
                            FilterUsersExpanded = false;
                            FilterTagsExpanded = false;
                            FilterCategoriesExpanded = false;
                            FilterStatusExpanded = false;
                            FilterPrioritiesExpanded = false;
                            FilterBuildingsExpanded = false;
                            break;
                        case "Users":
                            FilterUsersExpanded = !FilterUsersExpanded;
                            FilterDueDateExpanded = false;
                            FilterTagsExpanded = false;
                            FilterCategoriesExpanded = false;
                            FilterStatusExpanded = false;
                            FilterPrioritiesExpanded = false;
                            FilterBuildingsExpanded = false;
                            break;
                        case "Tags":
                            FilterTagsExpanded = !FilterTagsExpanded;
                            FilterUsersExpanded = false;
                            FilterDueDateExpanded = false;
                            FilterCategoriesExpanded = false;
                            FilterStatusExpanded = false;
                            FilterPrioritiesExpanded = false;
                            FilterBuildingsExpanded = false;
                            break;
                        case "Categories":
                            FilterCategoriesExpanded = !FilterCategoriesExpanded;
                            FilterTagsExpanded = false;
                            FilterUsersExpanded = false;
                            FilterDueDateExpanded = false;
                            FilterStatusExpanded = false;
                            FilterPrioritiesExpanded = false;
                            FilterBuildingsExpanded = false;
                            break;
                        case "Status":
                            FilterStatusExpanded = !FilterStatusExpanded;
                            FilterCategoriesExpanded = false;
                            FilterTagsExpanded = false;
                            FilterUsersExpanded = false;
                            FilterDueDateExpanded = false;
                            FilterPrioritiesExpanded = false;
                            FilterBuildingsExpanded = false;
                            break;
                        case "Priority":
                            FilterPrioritiesExpanded = !FilterPrioritiesExpanded;
                            FilterStatusExpanded = false;
                            FilterCategoriesExpanded = false;
                            FilterTagsExpanded = false;
                            FilterUsersExpanded = false;
                            FilterDueDateExpanded = false;
                            FilterBuildingsExpanded = false;
                            break;
                        case "Buildings":
                            FilterBuildingsExpanded = !FilterBuildingsExpanded;
                            FilterPrioritiesExpanded = false;
                            FilterStatusExpanded = false;
                            FilterCategoriesExpanded = false;
                            FilterTagsExpanded = false;
                            FilterUsersExpanded = false;
                            FilterDueDateExpanded = false;
                            break;
                        default:
                            break;
                    }
                });
            }
        }

        public bool ShowingTicketDetails { get; private set; }
        public bool RequestedTicketDetails { get; private set; }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            if (App.MasterDetailNav != null)
                App.MasterDetailNav.IsGestureEnabled = true;
            if (!ShowingTicketDetails)
            {
                FilterDueDate = null;
                FilterKeywords = string.Empty;
                IsLowPriorityFilterSelected = false;
                IsHighPriorityFilterSelected = false;
                IsMediumPriorityFilterSelected = false;
                Categories?.Select(t => t.IsSelected = false);
                Users?.Select(t => t.IsSelected = false);
                Tags?.Select(t => t.IsSelected = false);
                ParameterItem = null;
                CurrentFilter = null;
            }
        }


        public override void Init(object initData)
        {
            base.Init(initData);
            Buildings = App.Buildings;
            Categories = App.Categories;
            Tags = App.Tags;
            Users = App.Users;
            if (initData is int buildingId)
            {
                //view requested from Buildings view
                Buildings = App.Buildings;
                Categories = App.Categories;
                Tags = App.Tags;
                Users = App.Users;
                ParameterItem = new TicketRequestItem
                {
                    Buildings = new List<int> { buildingId },
                    TicketStatus = TicketStatus.Open
                };
                NumberOfAppliedFilters = ParameterItem.NumberOfAppliedFilters.ToString();
                var selectedBuildings = Buildings.Where(b => b.BuildingId == buildingId);
                foreach (var b in selectedBuildings)
                {
                    b.IsSelected = true;
                }
                BackbuttonIsVisible = true;
                App.MasterDetailNav.IsGestureEnabled = false;
            }
            async void p(object sender, MaintenanceTicket e)
            {
                var id = e.TicketId;
                if (id == LastLoadedItemId && CanGetMorePages)
                {
                    LastLoadedItemId = 0;
                    await LoadData(refreshData: false, FetchNextPage: true);
                }
            }
            ((MaintenanceTicketsPage)this.CurrentPage).OnTicketAppeared += p;
        }

        public override void ReverseInit(object returnedData)
        {
            base.ReverseInit(returnedData);
            if (returnedData is MaintenanceTicket && FetchedTickets != null &&
                    FetchedTickets.Contains((MaintenanceTicket)returnedData))
            {
                FetchedTickets.Remove((MaintenanceTicket)returnedData);
            }
            else if (returnedData is bool && (bool)returnedData && FetchedTickets != null)
            {
                OnPulledToRefresh.Execute(null);

            }
        }
        private async Task ShowNoInternetView()
        {
            APIhasFailed = true;
            HasLoaded = false;
            ErrorText = Connectivity.NetworkAccess != NetworkAccess.Internet ?
                                    "No Internet Connection" : "Host Unreachable";
            await Task.Run(() =>
            {
                while (Connectivity.NetworkAccess != NetworkAccess.Internet
                       && !cancellationTokenSource.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                }
            });
            await LoadData();
        }
    }
}
