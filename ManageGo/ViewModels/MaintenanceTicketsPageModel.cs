using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomCalendar;
using FreshMvvm;
using Xamarin.Forms;
using PropertyChanged;
using System.Linq;
using Newtonsoft.Json;

namespace ManageGo
{
    internal class MaintenanceTicketsPageModel : BaseDetailPage
    {
        DateRange dateRange;
        readonly string filterSelectedColor = "#8ad96b";
        readonly string filterDefaultColor = "#aeb0b3";
        public View PopContentView { get; private set; }
        bool CalendarIsShown { get; set; }
        bool FilterSelectViewIsShown { get; set; }
        public List<MaintenanceTicket> FetchedTickets { get; private set; }
        public bool ListIsEnabled { get; set; } = false;
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
        public string NumberOfAppliedFilters { get; private set; } = " ";
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
                return Categories is null || !Categories.Any(t => t.IsSelectedForFiltering)
                                                        || Categories.Count() == Categories.Count(t => t.IsSelectedForFiltering)
                    ? "All"
                    :
                    Categories.Count(t => t.IsSelectedForFiltering) > 1 ?
                                                        $"{Categories.First(t => t.IsSelectedForFiltering).CategoryName}, {Categories.Count(t => t.IsSelectedForFiltering) - 1} more"
                                                            :
                                                        Categories.First(t => t.IsSelectedForFiltering).CategoryName
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
                return Tags is null || !Tags.Any(t => t.IsSelectedForFiltering)
                                            || Tags.Count() == Tags.Count(t => t.IsSelectedForFiltering)
                    ? "All"
                    : Tags.Count(t => t.IsSelectedForFiltering) > 1 ?
                                            $"{Tags.First(t => t.IsSelectedForFiltering).TagName}, {Tags.Count(t => t.IsSelectedForFiltering) - 1} more"
                                                :
                                            Tags.First(t => t.IsSelectedForFiltering).TagName;
            }
        }
        public bool IsSearching { get; private set; }
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
        Dictionary<string, object> FiltersDictionary { get; set; }
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

        DateRange DateRange
        {
            get
            {
                return dateRange is null ? new DateRange(DateTime.Today, DateTime.Today.AddDays(-7)) : dateRange;
            }
            set
            {
                dateRange = value;
            }
        }

        internal override async Task LoadData(bool refreshData = false)
        {
            //throw new NotImplementedException();
            if (FetchedTickets is null || refreshData)
            {
                if (FiltersDictionary is null || refreshData)
                {
                    FiltersDictionary = new Dictionary<string, object>
                    {
                        { "DateFrom", this.DateRange.StartDate },
                        { "DateTo", this.DateRange.EndDate ?? this.DateRange.StartDate },
                    };
                }

                FetchedTickets = await Services.DataAccess.GetTicketsAsync(FiltersDictionary);
                Console.WriteLine($"Tickets Fetched: {FetchedTickets.Count}");
                Buildings = App.Buildings;
                Categories = App.Categories;
                Tags = App.Tags;
                Users = App.Users;
                ListIsEnabled = true;
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

        public FreshAwaitCommand OnCategoryTapped
        {
            get
            {
                return new FreshAwaitCommand((parameter, tcs) =>
                {
                    var category = parameter as Categories;
                    category.IsSelectedForFiltering = !category.IsSelectedForFiltering;
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
                    tag.IsSelectedForFiltering = !tag.IsSelectedForFiltering;
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

        public FreshAwaitCommand OnApplyFiltersTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
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
                    if (Tags != null && Tags.Any(f => f.IsSelectedForFiltering))
                    {
                        paramDic.Add("Tags", Tags.Where(t => t.IsSelectedForFiltering).Select(t => t.TagID));
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
                    IsSearching = true;
                    var results = await Services.DataAccess.GetTicketsAsync(paramDic);
                    FetchedTickets = results;
                    ListIsEnabled = true;
                    IsSearching = false;
                    tcs?.SetResult(true);
                });
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
                        applyButton.Clicked += async (sender, e) =>
                        {
                            this.DateRange = cal.SelectedDates;
                            OnCalendarButtonTapped.Execute(null);
                            IsSearching = true;
                            await LoadData(true);
                            IsSearching = false;
                        };
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
                            break;
                        case "Users":
                            FilterUsersExpanded = !FilterUsersExpanded;
                            break;
                        case "Tags":
                            FilterTagsExpanded = !FilterTagsExpanded;
                            break;
                        case "Categories":
                            FilterCategoriesExpanded = !FilterCategoriesExpanded;
                            break;
                        case "Status":
                            FilterStatusExpanded = !FilterStatusExpanded;
                            break;
                        case "Priority":
                            FilterPrioritiesExpanded = !FilterPrioritiesExpanded;
                            break;
                        case "Buildings":
                            FilterBuildingsExpanded = !FilterBuildingsExpanded;
                            break;
                        default:
                            break;
                    }
                });
            }
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            FilterDueDate = null;
            FilterKeywords = string.Empty;
            IsLowPriorityFilterSelected = false;
            IsHighPriorityFilterSelected = false;
            IsMediumPriorityFilterSelected = false;
            Categories?.Select(t => t.IsSelectedForFiltering = false);
            Users?.Select(t => t.IsSelected = false);
            Tags?.Select(t => t.IsSelectedForFiltering = false);
            FiltersDictionary = null;
        }
    }
}
