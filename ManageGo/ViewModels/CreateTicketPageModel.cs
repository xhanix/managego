using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FreshMvvm;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Plugin.Permissions;
using PropertyChanged;
using Xamarin.Forms;

namespace ManageGo
{
    public class CreateTicketPageModel : FreshBasePageModel
    {
        [AlsoNotifyFor("PopUpBackgroundIsVisible")]
        public bool AttachActionSheetIsVisible { get; private set; }
        public bool PopUpBackgroundIsVisible => AttachActionSheetIsVisible;
        public List<Building> Buildings { get; private set; }
        [AlsoNotifyFor("MoreRowsOptionsRowIcon")]
        public bool ExtraDetailRowsAreVisible { get; private set; }
        public List<Unit> Units { get; private set; }
        public List<Tenant> Tenants { get; private set; }
        public List<Categories> Categories { get; private set; }
        public List<Tags> Tags { get; private set; }
        public List<User> Users { get; private set; }
        public string DueDate { get; private set; } = "Select";
        public string Comment { get; set; }
        public string Subject { get; set; }

        public string AttachedFileName => AttachedFile?.Name;
        public bool HasAttachment => AttachedFile != null;
        [AlsoNotifyFor("DueDateRowIcon")]
        public View DueDateCalendarView { get; private set; }
        public string DueDateRowIcon => DueDateCalendarView != null ? "chevron_down.png" : "chevron.png";
        public string MoreRowsOptionsRowIcon => ExtraDetailRowsAreVisible ? "chevron_down.png" : "chevron.png";
        CancellationTokenSource cts = new CancellationTokenSource();
        [AlsoNotifyFor("HasAttachment", "AttachedFileName")]
        private File AttachedFile { get; set; }

        public FreshAwaitCommand OnShowMoreRowsTapped => new FreshAwaitCommand((tcs) =>
                                                                       {
                                                                           ExtraDetailRowsAreVisible = !ExtraDetailRowsAreVisible;
                                                                           tcs?.SetResult(true);
                                                                       });

        public FreshAwaitCommand OnCreateButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    if (string.IsNullOrWhiteSpace(Comment) || string.IsNullOrWhiteSpace(Subject))
                    {
                        await CoreMethods.DisplayAlert("Cannot create ticket", "Please type subject and message", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    if (Units is null || !Units.Any(t => t.IsSelected))
                    {
                        await CoreMethods.DisplayAlert("Cannot create ticket", "Please select building and unit", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    var ticketPriority = 2;
                    if (PriorityLabelText.ToLower() == "low")
                        ticketPriority = 0;
                    else if (PriorityLabelText.ToLower() == "medium")
                        ticketPriority = 1;
                    try
                    {
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                           {
                            { "BuildingID", Buildings.First(t=>t.IsSelected).BuildingId },
                            { "UnitID", Units.First(t=>t.IsSelected).UnitId},
                            { "Status", 0 },
                            { "Priority", ticketPriority },
                            { "Subject", Subject },
                            { "Comment", Comment },
                           };
                        if (DateTime.TryParse(DueDate, out DateTime d))
                            parameters.Add("DueDate", d);
                        if (Tenants != null && Tenants.Any(t => t.IsSelected))
                        {
                            parameters.Add("TenantID", Tenants.First(t => t.IsSelected).TenantID);
                        }
                        if (Categories != null && Categories.Any(t => t.IsSelected))
                        {
                            parameters.Add("Categories", Categories.Where(t => t.IsSelected).Select(t => t.CategoryID));
                        }
                        if (Tags != null && Tags.Any(t => t.IsSelected))
                        {
                            parameters.Add("Tags", Tags.Where(t => t.IsSelected).Select(t => t.TagID));
                        }
                        if (Users != null && Users.Any(t => t.IsSelected))
                        {
                            parameters.Add("Assigned", Users.Where(t => t.IsSelected).Select(t => t.UserID));
                        }
                        if (AttachedFile != null)
                        {
                            parameters.Add("FileName", AttachedFile.Name);
                            parameters.Add("File", AttachedFile.Content);
                        }
                        await Services.DataAccess.CreateTicket(parameters);
                        await CoreMethods.PopPageModel(data: true);
                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                    }
                    finally
                    {
                        tcs?.SetResult(true);
                    }
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnCancelTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    await CoreMethods.PopPageModel();
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnShowDueDateCalendarTapped => new FreshAwaitCommand((tcs) =>
                                                                              {
                                                                                  if (DueDateCalendarView != null)
                                                                                      DueDateCalendarView = null;
                                                                                  else
                                                                                  {
                                                                                      var cal = new Controls.CalendarView
                                                                                      {
                                                                                          HeightRequest = 240
                                                                                      };
                                                                                      DueDateCalendarView = cal;
                                                                                      cal.AllowMultipleSelection = false;
                                                                                      cal.OnSelectedDatesUpdate += (object sender, EventArgs e) =>
                                                                                      {
                                                                                          DueDate = cal.SelectedDate.ToString("MM/dd/yy");
                                                                                      };
                                                                                  }
                                                                                  tcs?.SetResult(true);
                                                                              });

        [AlsoNotifyFor("AssignedOptionsRowIcon")]
        public bool AssignedOptionsVisible { get; private set; }
        public string AssignedLabelText { get; private set; } = "Select";
        public string AssignedOptionsRowIcon => AssignedOptionsVisible ? "chevron_down.png" : "chevron.png";

        [AlsoNotifyFor("TagOptionsRowIcon")]
        public bool TagOptionsVisible { get; private set; }
        public string TagLabelText { get; private set; } = "Select";
        public string TagLabelColor { get; private set; } = "#424242";
        public string TagOptionsRowIcon => TagOptionsVisible ? "chevron_down.png" : "chevron.png";
        [AlsoNotifyFor("CategoryOptionsRowIcon")]
        public bool CategoryOptionsVisible { get; private set; }
        public string CategoryLabelText { get; private set; } = "Select";
        public string CategoryLabelColor { get; private set; } = "#424242";
        public string CategoryOptionsRowIcon => CategoryOptionsVisible ? "chevron_down.png" : "chevron.png";

        [AlsoNotifyFor("AddressOptionsRowIcon")]
        public bool AddressOptionsVisible { get; private set; }
        public string AddressLabelText { get; private set; } = "Select";
        public string AddressOptionsRowIcon => AddressOptionsVisible ? "chevron_down.png" : "chevron.png";

        [AlsoNotifyFor("PriorityOptionsRowIcon")]
        public bool PriorityOptionsVisible { get; private set; }
        [AlsoNotifyFor("PriorityLabelTextColor")]
        public string PriorityLabelText { get; private set; } = "Medium";
        public string PriorityOptionsRowIcon => PriorityOptionsVisible ? "chevron_down.png" : "chevron.png";
        public string PriorityLabelTextColor => PriorityLabelText.ToLower() == "low" ? "#949494" :
                    PriorityLabelText.ToLower() == "medium" ? "#e0a031" : "#E13D40";
        public string LowPriorityRadioIcon => PriorityLabelText.ToLower() == "low" ? "radio_selected.png" : "radio_unselected.png";
        public string MediumPriorityRadioIcon => PriorityLabelText.ToLower() == "medium" ? "radio_selected.png" : "radio_unselected.png";
        public string HighPriorityRadioIcon => PriorityLabelText.ToLower() == "high" ? "radio_selected.png" : "radio_unselected.png";


        [AlsoNotifyFor("TenantOptionsRowIcon")]
        public bool TenantOptionsVisible { get; private set; }
        public string TenantLabelText { get; private set; } = "Select";
        public string TenantOptionsRowIcon => TenantOptionsVisible ? "chevron_down.png" : "chevron.png";

        [AlsoNotifyFor("UnitOptionsRowIcon")]
        public bool UnitOptionsVisible { get; private set; }
        public string UnitLabelText { get; private set; } = "Select";
        public string UnitOptionsRowIcon => UnitOptionsVisible ? "chevron_down.png" : "chevron.png";

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            Buildings = App.Buildings;
            Categories = App.Categories;
            Tags = App.Tags;
            Users = App.Users;
        }

        public override void ReverseInit(object returnedData)
        {
            base.ReverseInit(returnedData);
            //this is the path to a photo/video file that was just captured
            if (returnedData is string path)
            {
                var fileName = Path.GetFileName(path);
                var docPath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var fullPath = docPath + "/" + fileName;
                var bytes = System.IO.File.ReadAllBytes(fullPath);
                AttachedFile = new File { Content = bytes, Name = Path.GetFileName(path) };
            }
        }


        public FreshAwaitCommand OnShowAssignedOptionsTapped => new FreshAwaitCommand((tcs) =>
                                                                              {
                                                                                  AssignedOptionsVisible = !AssignedOptionsVisible;
                                                                                  tcs?.SetResult(true);
                                                                              });

        void ClearCategorySelections()
        {
            foreach (var c in Categories)
            {
                c.IsSelected = false;
            }
        }

        void EnableAllCategories()
        {
            foreach (var c in Categories)
            {
                c.IsEnabled = true;
            }
        }

        void DisableAllCategories()
        {
            foreach (var c in Categories)
            {
                c.IsEnabled = false;
            }
        }

        void ClearUserSelections()
        {
            foreach (var u in Users)
            {
                u.IsSelected = false;
            }
        }


        void EnableAllUsers()
        {
            foreach (var u in Users)
            {
                u.IsEnabled = true;
            }
        }

        void DisableAllUsers()
        {
            foreach (var u in Users)
            {
                u.IsEnabled = false;
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

        public FreshAwaitCommand OnUserTapped
        {
            get
            {
                async void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    var u = (User)par;
                    if (!u.IsEnabled)
                    {
                        var result = await CoreMethods.DisplayAlert("ManageGo", $"{u.UserFullName} does not have access to the selected categories. Selecting this user will clear the selected categories.", $"Select {u.UserFullName}", "Cancel");
                        if (!result)
                        {
                            tcs?.SetResult(true);
                            return;
                        }
                        else
                        {
                            ClearCategorySelections();
                            EnableAllUsers();
                        }
                    }
                    u.IsSelected = !u.IsSelected;
                    if (Users.Any(t => t.IsSelected == true))
                    {
                        AssignedLabelText = Users.First(t => t.IsSelected).UserFullName;
                        if (Users.Count(t => t.IsSelected) > 1)
                        {
                            AssignedLabelText = AssignedLabelText + $", +{Users.Count(t => t.IsSelected) - 1} more";
                        }

                        var allowedCategories = Categories.Select(c => c.CategoryID);

                        if (Users.Any(t => t.IsSelected && (t.Categories is null || !t.Categories.Any())))
                        {
                            DisableAllCategories();

                        }
                        else
                        {
                            foreach (var user in Users.Where(user => user.IsSelected))
                            {
                                allowedCategories = allowedCategories.Intersect(user.Categories);
                            }
                            DisableAllCategories();
                            if (allowedCategories.Any())
                            {
                                foreach (var c in Categories)
                                {
                                    if (allowedCategories.Contains(c.CategoryID))
                                    {
                                        c.IsEnabled = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //no users are selected
                        AssignedLabelText = string.Empty;
                        EnableAllCategories();
                        EnableAllUsers();
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }


        public FreshAwaitCommand OnShowTagOptionsTapped => new FreshAwaitCommand((tcs) =>
                                                                         {
                                                                             TagOptionsVisible = !TagOptionsVisible;
                                                                             tcs?.SetResult(true);
                                                                         });
        public FreshAwaitCommand OnTagTapped => new FreshAwaitCommand((par, tcs) =>
                                                              {
                                                                  var tag = (Tags)par;
                                                                  tag.IsSelected = !tag.IsSelected;
                                                                  if (Tags.Any(t => t.IsSelected == true))
                                                                  {
                                                                      TagLabelText = Tags.First(t => t.IsSelected).TagName;
                                                                      TagLabelColor = "#" + Tags.First(t => t.IsSelected).Color;
                                                                      if (Tags.Count(t => t.IsSelected) > 1)
                                                                      {
                                                                          TagLabelText = TagLabelText + $", +{Tags.Count(t => t.IsSelected) - 1} more";
                                                                          TagLabelColor = "#58595B";
                                                                      }
                                                                  }
                                                                  else
                                                                      TagLabelText = string.Empty;
                                                                  tcs?.SetResult(true);
                                                              });

        public FreshAwaitCommand OnAttachButtonTapped => new FreshAwaitCommand((tcs) =>
                                                                       {
                                                                           AttachActionSheetIsVisible = true;
                                                                           tcs?.SetResult(true);
                                                                       });

        public FreshAwaitCommand OnRemoveAttachmentTapped => new FreshAwaitCommand((tcs) =>
                                                                           {
                                                                               AttachedFile = null;
                                                                           });

        public FreshAwaitCommand OnTakePhotoTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    if (await CheckCameraPermissionsAsync())
                    {
                        AttachActionSheetIsVisible = false;
                        await CoreMethods.PushPageModel<TakeVideoPageModel>(true, true, false);
                    }
                    else if (NumberOfTries >= 2)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Unable to take photo or video. You did not allow access to camera.", "DISMISS");
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnUploadPhotoTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    AttachActionSheetIsVisible = false;
                    var result = await Services.PhotoHelper.PickPhotoAndVideos();
                    if (result.Item1 != null)
                    {
                        AttachedFile = new File { Content = result.Item1, Name = result.Item2 };
                        RaisePropertyChanged("ReplyAttachedFilesIsVisible");
                        RaisePropertyChanged("ReplyAttachedFilesListHeight");
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnUploadFileTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    try
                    {
                        AttachActionSheetIsVisible = false;

                        FileData fileData = await CrossFilePicker.Current.PickFile();
                        if (fileData == null)
                            return; // user canceled file picking

                        string fileName = fileData.FileName;
                        AttachedFile = new File { Content = fileData.DataArray, Name = fileName };
                        RaisePropertyChanged("ReplyAttachedFilesIsVisible");
                        RaisePropertyChanged("ReplyAttachedFilesListHeight");
                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                    }
                    finally
                    {
                        tcs?.SetResult(true);
                    }

                }
                return new FreshAwaitCommand(execute);
            }
        }


        public FreshAwaitCommand OnBackgroundTapped => new FreshAwaitCommand((tcs) =>
                                                                     {
                                                                         AttachActionSheetIsVisible = false;
                                                                         tcs?.SetResult(true);
                                                                     });

        public FreshAwaitCommand OnShowCategoryOptionsTapped => new FreshAwaitCommand((tcs) =>
                                                                              {
                                                                                  CategoryOptionsVisible = !CategoryOptionsVisible;
                                                                                  tcs?.SetResult(true);
                                                                              });

        public FreshAwaitCommand OnCategoryTapped
        {
            get
            {
                async void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    var cat = (Categories)par;
                    if (!cat.IsEnabled)
                    {
                        var result = await CoreMethods.DisplayAlert("ManageGo", $"{cat.CategoryName} is not available to the assigned users. Selecting this category will clear the selected users.", $"Select {cat.CategoryName}", "Cancel");
                        if (!result)
                        {
                            tcs?.SetResult(true);
                            return;
                        }
                        else
                        {
                            ClearUserSelections();
                            EnableAllCategories();
                        }
                    }
                    cat.IsSelected = !cat.IsSelected;
                    if (Categories.Any(t => t.IsSelected == true))
                    {
                        CategoryLabelText = Categories.First(t => t.IsSelected).CategoryName;
                        CategoryLabelColor = "#" + Categories.First(t => t.IsSelected).Color;
                        if (Categories.Count(t => t.IsSelected) > 1)
                        {
                            CategoryLabelText = CategoryLabelText + $", +{Categories.Count(t => t.IsSelected) - 1} more";
                            CategoryLabelColor = "#58595B";
                        }
                        //selected categories
                        DisableAllUsers();
                        var selectedCats = Categories.Where(c => c.IsSelected).Select(c => c.CategoryID);
                        foreach (User u in Users.Where(user => user.Categories != null))
                        {
                            if (u.Categories.Intersect(selectedCats).Any())
                                u.IsEnabled = true;
                        }
                    }
                    else
                    {
                        EnableAllUsers();
                        if (!Users.Any(user => user.IsSelected))
                            EnableAllCategories();
                        CategoryLabelText = "Select";
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnShowPriorityOptionsTapped => new FreshAwaitCommand((tcs) =>
                                                                              {
                                                                                  PriorityOptionsVisible = !PriorityOptionsVisible;
                                                                                  tcs?.SetResult(true);
                                                                              });


        public FreshAwaitCommand OnSwitchPriorityTapped => new FreshAwaitCommand((par, tcs) =>
                                                                         {
                                                                             string text = (string)par;
                                                                             if (text == "low")
                                                                                 PriorityLabelText = "Low";
                                                                             else if (text == "medium")
                                                                                 PriorityLabelText = "Medium";
                                                                             else
                                                                                 PriorityLabelText = "High";
                                                                             tcs?.SetResult(true);
                                                                         });


        public FreshAwaitCommand OnShowTenantOptionsTapped
        {
            get
            {
                async void execute(System.Threading.Tasks.TaskCompletionSource<bool> tcs)
                {
                    if (Units is null || !Units.Any(t => t.IsSelected))
                    {
                        await CoreMethods.DisplayAlert("Address not selected", "Select an address to pick a unit", "OK");
                    }
                    else if (Tenants is null || !Tenants.Any())
                    {
                        await CoreMethods.DisplayAlert("Tenants not found", "Unit has no tenants", "OK");
                    }
                    else
                    {
                        TenantOptionsVisible = !TenantOptionsVisible;
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }



        public FreshAwaitCommand OnTenantTapped => new FreshAwaitCommand((par, tcs) =>
                                                                 {
                                                                     var tenant = (Tenant)par;
                                                                     tenant.IsSelected = true;
                                                                     TenantLabelText = tenant.FullName;
                                                                     foreach (var t in Tenants.Where(t => t.IsSelected && t.TenantID != tenant.TenantID))
                                                                     {
                                                                         t.IsSelected = false;
                                                                     }
                                                                     tcs?.SetResult(true);
                                                                 });


        public FreshAwaitCommand OnShowUnitOptionsTapped
        {
            get
            {
                async void execute(System.Threading.Tasks.TaskCompletionSource<bool> tcs)
                {
                    if (!Buildings.Any(t => t.IsSelected))
                    {
                        await CoreMethods.DisplayAlert("Address not selected", "Select an address to pick a unit", "OK");
                    }
                    else if (Units is null || !Units.Any())
                    {
                        await CoreMethods.DisplayAlert("Units not found", "Building has no units", "OK");
                    }
                    else
                    {
                        UnitOptionsVisible = !UnitOptionsVisible;
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }


        public FreshAwaitCommand OnUnitTapped => new FreshAwaitCommand((par, tcs) =>
                                                               {
                                                                   var unit = (Unit)par;
                                                                   unit.IsSelected = true;
                                                                   UnitLabelText = unit.UnitName;
                                                                   Tenants = unit.Tenants;
                                                                   foreach (var u in Units.Where(t => t.IsSelected && t.UnitId != unit.UnitId))
                                                                   {
                                                                       u.IsSelected = false;
                                                                   }
                                                                   tcs?.SetResult(true);
                                                               });

        public FreshAwaitCommand OnShowAddressesOptionsTapped => new FreshAwaitCommand((tcs) =>
                                                                               {
                                                                                   AddressOptionsVisible = !AddressOptionsVisible;
                                                                                   tcs?.SetResult(true);
                                                                               });

        public FreshAwaitCommand OnBuildingTapped
        {
            get
            {
                async void execute(object par, System.Threading.Tasks.TaskCompletionSource<bool> tcs)
                {
                    var building = (Building)par;
                    cts.Cancel();
                    cts = new CancellationTokenSource();
                    building.IsSelected = true;
                    Users = App.Users.Where(user => user.Buildings.Contains(building.BuildingId)).ToList();
                    try
                    {
                        var buildingWithDetails = await Services.DataAccess.GetBuildingDetails(building.BuildingId);
                        if (!cts.Token.IsCancellationRequested)
                        {
                            Units = buildingWithDetails.Units;
                        }
                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                    }
                    AddressLabelText = building.BuildingName;
                    foreach (var b in Buildings.Where(t => t.IsSelected && t.BuildingId != building.BuildingId))
                    {
                        b.IsSelected = false;
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public int NumberOfTries { get; private set; }

        private async Task<bool> CheckCameraPermissionsAsync()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Camera);
            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);
            var audioStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Microphone);
            // user does not have any of the required permissions
            if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted ||
               audioStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted ||
               storageStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                // user does not have any of the required permissions
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Camera))
                {
                    await CoreMethods.DisplayAlert("ManageGo", "Need camera access to take photo/video", "OK");
                }
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Microphone))
                {
                    await CoreMethods.DisplayAlert("ManageGo", "Need audio access to take video", "OK");
                }
                if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Storage))
                {
                    await CoreMethods.DisplayAlert("ManageGo", "Need access to storage to take photo/video", "OK");
                }
                await CrossPermissions.Current.RequestPermissionsAsync(
                        Plugin.Permissions.Abstractions.Permission.Camera,
                        Plugin.Permissions.Abstractions.Permission.Microphone,
                        Plugin.Permissions.Abstractions.Permission.Storage);
                if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted ||
              audioStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted ||
              storageStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                {
                    if (await CoreMethods.DisplayAlert("ManageGo", "Please allow access to continue", "OK", "Cancel"))
                    {
                        NumberOfTries++;
                        return await CheckCameraPermissionsAsync();
                    }
                    return false;
                }
                return true;
            }
            return true;
        }

    }
}

