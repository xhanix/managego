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

namespace ManageGo
{

    internal class MaintenanceTicketsPageModel : BaseDetailPage
    {
        DateRange dateRange;
        readonly string filterSelectedColor = "#8ad96b";
        readonly string filterDefaultColor = "#aeb0b3";
        public View PopContentView { get; internal set; }
        [AlsoNotifyFor("AddNewButtonIsVisisble")]
        internal bool CalendarIsShown { get; set; }
        [AlsoNotifyFor("AddNewButtonIsVisisble")]
        public bool FilterSelectViewIsShown { get; set; }
        public bool NothingFetched { get; private set; }
        public List<MaintenanceTicket> FetchedTickets { get; set; }
        public bool IsRefreshingList { get; set; }
        public bool ListIsEnabled { get; set; } = false;
        public bool AddNewButtonIsVisisble
        {
            get
            {
                return !(FilterSelectViewIsShown || CalendarIsShown);
            }
        }
        public List<Building> Buildings { get; private set; }
        [AlsoNotifyFor("SelectedCategoriesString")]
        public List<Categories> Categories { get; private set; }
        [AlsoNotifyFor("SelectedTagsString")]
        public List<Tags> Tags { get; private set; }
        public List<User> Users { get; private set; }
        [AlsoNotifyFor("LowPriorityCheckBoxImage", "SelectedPriorityString", "FilterPriorityTextColor")]
        public bool IsLowPriorityFilterSelected { get; private set; }
        [AlsoNotifyFor("MediumPriorityCheckBoxImage", "SelectedPriorityString", "FilterPriorityTextColor")]
        public bool IsMediumPriorityFilterSelected { get; private set; }
        [AlsoNotifyFor("HighPriorityCheckBoxImage", "SelectedPriorityString", "FilterPriorityTextColor")]
        public bool IsHighPriorityFilterSelected { get; private set; }
        [AlsoNotifyFor("OpenTicketFilterCheckBoxImage", "SelectedStatusFlagsString", "FilterStatusTextColor")]
        public bool SelectedOpenTicketsFilter { get; private set; }
        [AlsoNotifyFor("ClosedTicketFilterCheckBoxImage", "SelectedStatusFlagsString", "FilterStatusTextColor")]
        public bool SelectedClosedTicketsFilter { get; private set; }
        public string FilterKeywords { get; set; }
        public string NumberOfAppliedFilters { get; internal set; } = " ";
        int LastLoadedItemId { get; set; }
        int CurrentListPage { get; set; } = 1;
        bool CanGetMorePages { get; set; }
        readonly int pageSize = 30;

        public string ClosedTicketFilterCheckBoxImage
        {
            get
            {
                return SelectedClosedTicketsFilter ? "checked.png" : "unchecked.png";
            }
        }
        public string OpenTicketFilterCheckBoxImage
        {
            get
            {
                return SelectedOpenTicketsFilter ? "checked.png" : "unchecked.png";
            }
        }
        public string HighPriorityCheckBoxImage
        {
            get
            {
                return IsHighPriorityFilterSelected ? "checked.png" : "unchecked.png";
            }
        }
        public string MediumPriorityCheckBoxImage
        {
            get
            {
                return IsMediumPriorityFilterSelected ? "checked.png" : "unchecked.png";
            }
        }
        public string LowPriorityCheckBoxImage
        {
            get
            {
                return IsLowPriorityFilterSelected ? "checked.png" : "unchecked.png";
            }
        }
        public string SelectedStatusFlagsString
        {
            get
            {
                return (SelectedOpenTicketsFilter && SelectedClosedTicketsFilter) || (!SelectedOpenTicketsFilter && !SelectedClosedTicketsFilter) ? "All" : SelectedOpenTicketsFilter ? "Open" : "Closed";
            }
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

        public bool BackbuttonIsVisible { get; private set; }

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
        public string FilterBuildingTextColor
        {
            get
            {
                return SelectedBuildingsString == "All" ? filterDefaultColor : filterSelectedColor;
            }
        }
        public string FilterStatusTextColor
        {
            get
            {
                return SelectedStatusFlagsString == "All" ? filterDefaultColor : filterSelectedColor;
            }
        }
        public string FilterCategoryTextColor
        {
            get
            {
                return SelectedCategoriesString == "All" ? filterDefaultColor : filterSelectedColor;
            }
        }
        public string FilterTagsTextColor
        {
            get
            {
                return SelectedTagsString == "All" ? filterDefaultColor : filterSelectedColor;
            }
        }
        public string FilterPriorityTextColor
        {
            get
            {
                return SelectedPriorityString == "All" ? filterDefaultColor : filterSelectedColor;
            }
        }
        bool HasCalendarFilter { get; set; }
        internal Dictionary<string, object> FiltersDictionary { get; set; }
        [AlsoNotifyFor("FilterDueDateString", "FilterDueDateColor")]
        public DateRange FilterDueDate { get; set; }

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

        public string FilterDueDateColor
        {
            get
            {
                return FilterDueDate != null ? filterSelectedColor : filterDefaultColor;
            }
        }
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

        internal override async Task LoadData(bool refreshData = false, bool applyNewFilter = false)
        {
            NothingFetched = false;
            try
            {
                if (HasPreExistingFilter)
                {
                    FetchedTickets = await Services.DataAccess.GetTicketsAsync(FiltersDictionary);
                    NumberOfAppliedFilters = "1";
                    BackbuttonIsVisible = true;
                }
                else if (FetchedTickets is null || refreshData)
                {
                    if (FiltersDictionary is null || refreshData)
                    {
                        FiltersDictionary = new Dictionary<string, object>
                    {
                         { "PageSize", pageSize },
                         { "Page",  1},

                    };
                        if (HasCalendarFilter)
                        {
                            FiltersDictionary.Add("DateFrom", DateRange.StartDate);
                            FiltersDictionary.Add("DateTo", DateRange.EndDate ?? DateRange.StartDate);
                        }
                    }
                    if (FetchedTickets is null || applyNewFilter)
                    {
                        // FetchedTickets is null on view init
                        FetchedTickets = await Services.DataAccess.GetTicketsAsync(FiltersDictionary);
                        if (FetchedTickets.Count >= pageSize)
                        {
                            var lastIdx = FetchedTickets.IndexOf(FetchedTickets.Last());
                            var index = Math.Floor(lastIdx / 2d);
                            var markedItem = FetchedTickets.ElementAt((int)index);
                            LastLoadedItemId = markedItem.TicketId;
                            CanGetMorePages = FetchedTickets.Count == pageSize;
                        }
                        else
                        {
                            CanGetMorePages = false;
                        }

                    }
                    else
                    {
                        var list = FetchedTickets.ToList();
                        var nextPage = await Services.DataAccess.GetTicketsAsync(FiltersDictionary);
                        CanGetMorePages = nextPage.Count == pageSize;
                        list.AddRange(nextPage);
                        FetchedTickets = list;
                    }

                    //RaisePropertyChanged("FetchedTickets");
                    if (FetchedTickets.Count > 0 && CanGetMorePages)
                    {
                        var lastIdx = FetchedTickets.IndexOf(FetchedTickets.Last());
                        var index = Math.Floor(lastIdx / 2d);
                        var markedItem = FetchedTickets.ElementAt((int)index);
                        LastLoadedItemId = markedItem.TicketId;
                    }
                    Buildings = App.Buildings;
                    Categories = App.Categories;
                    Tags = App.Tags;
                    Users = App.Users;
                    ListIsEnabled = true;
                    Console.WriteLine($"Tickets Fetched: {FetchedTickets.Count}");
                    HasLoaded = true;
                }
            }
            catch
            {
                APIhasFailed = true;
                FetchedTickets = null;
                NothingFetched = true;
                if (await CoreMethods.DisplayAlert("Something went wrong", "Unable to get tickets. Connect to network and try again", "Try again", "Dismiss"))
                {
                    if (!this.IsLoading)
                        await this.LoadData();
                }
            }
            finally
            {
                HasLoaded = true;
                if (!FetchedTickets.Any())
                {
                    NothingFetched = true;
                }
                else
                {
                    NothingFetched = false;
                }
            }
        }

        public async void OnItemAppeared(MaintenanceTicket ticket)
        {
            var id = ticket.TicketId;
            if (id == LastLoadedItemId)
            {
                //load more items
                CurrentListPage++;
                if (!HasPreExistingFilter)
                    await LoadData(refreshData: true);
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
                    }
                    else
                    {
                        var _view = new TicketFilterSelectView(bindingContext: this);
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
                    RaisePropertyChanged("FilterBuildingTextColor");
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
                    await LoadData(false, false);
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
                    RaisePropertyChanged("FilterCategoryTextColor");
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
                    RaisePropertyChanged("FilterTagsTextColor");
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
                    await LoadData(true, false);
                    NumberOfAppliedFilters = string.Empty;
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
                    HasCalendarFilter = false;
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
                    var numberOfFilters = 0;
                    FilterSelectViewIsShown = false;
                    Dictionary<string, object> paramDic = new Dictionary<string, object>();

                    if (Buildings != null && Buildings.Any(f => f.IsSelected))
                    {
                        paramDic.Add("Buildings", Buildings.Where(f => f.IsSelected).Select(f => f.BuildingId));
                        numberOfFilters++;
                    }
                    if (Tags != null && Tags.Any(f => f.IsSelected))
                    {
                        paramDic.Add("Tags", Tags.Where(t => t.IsSelected).Select(t => t.TagID));
                        numberOfFilters++;
                    }

                    if (SelectedOpenTicketsFilter && SelectedClosedTicketsFilter)
                    {
                        paramDic.Add("TicketStatus", -1);
                    }
                    else if (SelectedOpenTicketsFilter)
                    {
                        paramDic.Add("TicketStatus", 0);
                        numberOfFilters++;
                    }
                    else if (SelectedClosedTicketsFilter)
                    {
                        paramDic.Add("TicketStatus", 1);
                        numberOfFilters++;
                    }
                    else
                    {
                        paramDic.Add("TicketStatus", -1);
                    }
                    if (IsLowPriorityFilterSelected || IsMediumPriorityFilterSelected || IsHighPriorityFilterSelected)
                    {
                        List<int> priorities = new List<int>();
                        if (IsLowPriorityFilterSelected)
                            priorities.Add(0);
                        if (IsMediumPriorityFilterSelected)
                            priorities.Add(1);
                        if (IsHighPriorityFilterSelected)
                            priorities.Add(2);
                        numberOfFilters++;
                        paramDic.Add("Priorities", priorities);
                    }
                    if (Users != null && Users.Any(t => t.IsSelected))
                    {
                        paramDic.Add("Assigned", Users.Where(t => t.IsSelected).Select(t => t.UserID));
                        numberOfFilters++;
                    }
                    if (!string.IsNullOrWhiteSpace(FilterKeywords))
                    {
                        paramDic.Add("Search", FilterKeywords);
                        numberOfFilters++;
                    }
                    paramDic.Add("DateFrom", this.DateRange.StartDate);
                    if (this.DateRange.EndDate.HasValue)
                        paramDic.Add("DateTo", this.DateRange.EndDate.Value);
                    if (FilterDueDate != null)
                    {
                        paramDic.Add("DueDateFrom", FilterDueDate.StartDate);
                        paramDic.Add("DueDateTo", FilterDueDate.EndDate ?? FilterDueDate.StartDate);
                        numberOfFilters++;
                    }
                    NumberOfAppliedFilters = numberOfFilters > 0 ? $"{numberOfFilters}" : " ";
                    FiltersDictionary = paramDic;
                    NothingFetched = false;
                    HasLoaded = false;
                    FetchedTickets = new List<MaintenanceTicket>();
                    FetchedTickets = await Services.DataAccess.GetTicketsAsync(paramDic);
                    HasLoaded = true;
                    if (!FetchedTickets.Any())
                        NothingFetched = true;
                    else
                        NothingFetched = false;
                    ListIsEnabled = true;
                    tcs?.SetResult(true);
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
                            HasCalendarFilter = true;
                            FiltersDictionary = null;
                            NumberOfAppliedFilters = " ";
                            await LoadData(refreshData: true, applyNewFilter: true);
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

        public bool HasPreExistingFilter { get; private set; }

        protected override async void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            FilterDueDate = null;
            FilterKeywords = string.Empty;
            IsLowPriorityFilterSelected = false;
            IsHighPriorityFilterSelected = false;
            IsMediumPriorityFilterSelected = false;
            Categories?.Select(t => t.IsSelected = false);
            Users?.Select(t => t.IsSelected = false);
            Tags?.Select(t => t.IsSelected = false);
            FiltersDictionary = null;
            try
            {
                List<Task> tasks = new List<Task>();
                if (App.Buildings is null || !App.Buildings.Any())
                    tasks.Add(Services.DataAccess.GetBuildings());
                if (App.Categories is null || !App.Categories.Any())
                {
                    tasks.Add(Services.DataAccess.GetAllCategoriesAndTags());
                    tasks.Add(Services.DataAccess.GetAllUsers());
                }
                await Task.WhenAll(tasks);
            }
            catch
            {
                await ShowNoInternetView();
            }
        }


        public override void Init(object initData)
        {
            base.Init(initData);
            if (initData is int buildingId)
            {
                HasPreExistingFilter = true;
                Buildings = App.Buildings;
                Categories = App.Categories;
                Tags = App.Tags;
                Users = App.Users;

                FiltersDictionary = new Dictionary<string, object>
                {
                    { "Buildings", new List<int> { buildingId }}
                };

            }
        }

        public override async void ReverseInit(object returnedData)
        {
            base.ReverseInit(returnedData);
            if (returnedData is MaintenanceTicket && FetchedTickets != null &&
                    FetchedTickets.Contains((MaintenanceTicket)returnedData))
            {
                FetchedTickets.Remove((MaintenanceTicket)returnedData);
            }
            else if (returnedData is bool && (bool)returnedData && FetchedTickets != null)
            {
                await LoadData(refreshData: true);
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
