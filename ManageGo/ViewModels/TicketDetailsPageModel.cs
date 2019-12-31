using System;
using Xamarin.Forms;
using FreshMvvm;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Plugin.FilePicker.Abstractions;
using Plugin.FilePicker;
using Plugin.Permissions;
using System.Text;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class TicketDetailsPageModel : FreshBasePageModel
    {
        #region Permissions
        public bool CanSendPublicReply { get; private set; }
        public bool CanReplyToComments { get; private set; }
        public bool CanCreateWorkorderAndEvents { get; private set; }
        public bool CanEditTicketDetails { get; private set; }
        public bool IsUploadingFiles { get; private set; }
        public bool IsCreatingEvent { get; private set; }
        #endregion
        #region Properties
        public bool AccessGrantIsExpanded { get; set; } = true;
        [AlsoNotifyFor("ShowAccessGranted")]
        public bool IsAccessGranted { get; private set; }
        public bool ShowAccessGranted => IsAccessGranted && !ShowingTicketDetails;
        public string AccessPetText { get; private set; }
        public string CloseTicketButtonText => TicketStatus == TicketStatus.Closed ? "Open Ticket" : "Close Ticket";
        public string AccessGrantedTimesText { get; private set; }
        public bool SetFromTime { get; private set; }
        public bool SwitchingAmPam { get; private set; }
        public bool OldTimeIsPm { get; private set; }
        public bool IsBusy { get; set; }
        public string TenantName { get; private set; } = String.Empty;
        public TicketDetails TicketDetails { get; private set; }
        public string SecondLineText { get; private set; }
        public ObservableCollection<Comments> Comments { get; private set; }
        public string TicketTitle { get; set; }
        List<byte[]> FilesToUpload { get; set; }
        public string BuildingUnitText { get; private set; } = String.Empty;
        public string Status { get; private set; } = String.Empty;
        public Unit Unit { get; private set; }
        public int TicketTenant { get; private set; }
        public int BuildingId { get; private set; }
        public View PopContentView { get; private set; }
        [AlsoNotifyFor("DueDateRowIcon")]
        public View DueDateCalendarView { get; private set; }
        public DateTime OldTime { get; private set; }
        public bool ShouldShowClock { get; private set; }
        public bool PriorityOptionsVisible { get; private set; }
        public bool CategoryOptionsVisible { get; private set; }
        public bool AssignedOptionsVisible { get; private set; }
        public bool TagOptionsVisible { get; private set; }
        public string CategoryLabelText { get; private set; } = string.Empty;
        public string AssignedLabelText { get; private set; } = string.Empty;
        public string CategoryLabelColor { get; private set; } = "#424242";
        public string TagLabelText { get; private set; } = string.Empty;
        public string TagLabelColor { get; private set; } = "#424242";
        public string DueDate { get; private set; } = string.Empty;
        public List<Categories> Categories { get; private set; }
        public List<Tags> Tags { get; private set; }
        [AlsoNotifyFor("CloseTicketButtonText")]
        TicketStatus TicketStatus { get; set; }
        public double ReplyAttachedFilesListHeight => CurrentReplyAttachments is null || !CurrentReplyAttachments.Any() ? 10 : CurrentReplyAttachments.Count * 28;
        public bool CanSelectTenants { get; set; }
        public string DueDateRowIcon => DueDateCalendarView != null ? "chevron_down.png" : "chevron_right.png";

        public bool SetToTime { get; private set; }
        public bool ListIsEnabled { get; private set; } = true;
        public string TicketAddress { get; private set; }
        public string TicketTitleText { get; private set; }
        public string PriorityLabelTextColor => PriorityLabelText.ToLower() == "low" ? "#949494" : PriorityLabelText.ToLower() == "medium" ? "#e0a031" : "#E13D40";

        [AlsoNotifyFor("PriorityLabelTextColor")]
        public string PriorityLabelText { get; private set; }
        public string LowPriorityRadioIcon => PriorityLabelText.ToLower() == "low" ? "radio_selected.png" : "radio_unselected.png";
        public string MediumPriorityRadioIcon => PriorityLabelText.ToLower() == "medium" ? "radio_selected.png" : "radio_unselected.png";
        public string HighPriorityRadioIcon => PriorityLabelText.ToLower() == "high" ? "radio_selected.png" : "radio_unselected.png";

        public MaintenanceTicket CurrentTicket { get; private set; }
        public bool HasPet { get; private set; }
        public bool HasAccess { get; private set; }
        public bool HasWorkOrder { get; private set; }
        public bool HasEvent { get; private set; }
        [AlsoNotifyFor("ShowAccessGranted")]
        public bool ShowingTicketDetails { get; private set; }
        public bool ReplyButtonIsVisible { get; private set; } = true;
        public bool ReplyBoxIsVisible { get; private set; }
        public bool IsDownloadingFile { get; private set; }
        public bool WorkOrderActionSheetIsVisible { get; private set; }
        public bool EventActionSheetIsVisible { get; private set; }
        public LayoutOptions FromFrameVerticalLayout { get; private set; }
        public LayoutOptions ToFrameVerticalLayout { get; private set; }
        bool IsToTimeSelected { get; set; }
        public List<User> Users { get; private set; }
        public string TicketComment { get; private set; }

        DateTime pickedTime;


        [AlsoNotifyFor("EndTimeTextColor")]
        public string ToTime { get; set; }
        [AlsoNotifyFor("EndTimeTextColor")]
        public string FromTime { get; set; }
        public string PickerTitleText { get; private set; }
        public int BridgeColumn { get; private set; }
        [AlsoNotifyFor("AmButtonImage", "PmButtonImage")]
        public bool PickedTimeIsAM { get; set; }
        public string CurrentTime => ClockTime.ToString("h:mm");
        public string AmButtonImage => PickedTimeIsAM ? "am_active.png" : "am_inactive.png";
        public string PmButtonImage => !PickedTimeIsAM ? "pm_active.png" : "pm_inactive.png";
        readonly string redColor = "#fc3535";
        public string EndTimeTextColor => DateTime.Parse(FromTime) > DateTime.Parse(ToTime) ? redColor : "#9b9b9b";

        [AlsoNotifyFor("CurrentTime")]
        public DateTime ClockTime { get; set; }
        [AlsoNotifyFor("FromTime", "ToTime", "CurrentTime")]
        public DateTime PickedTime
        {
            get { return pickedTime; }
            set
            {
                Console.WriteLine("Picked time SET");
                if (!PickedTimeIsAM)
                    pickedTime = !PickedTimeIsAM && !SwitchingAmPam ? value.AddHours(12) : value;
                else
                    pickedTime = value;
                if (SetToTime)
                    ToTime = pickedTime.ToString("h:mm tt");
                else if (SetFromTime)
                    FromTime = pickedTime.ToString("h:mm tt");

                SwitchingAmPam = false;
            }
        }

        public string WorkOrderSummary { get; set; }
        public string WorkOrderDetail { get; set; }
        public string WorkOrderSendEmail { get; set; }

        public string EventSummary { get; set; }
        [AlsoNotifyFor("SelectedEventDateString")]
        public DateTime SelectedEventDate { get; set; }
        public bool EventCalendarIsVisible { get; private set; }

        public string SelectedEventDateString => SelectedEventDate == DateTime.MinValue ? "Select..." : SelectedEventDate.ToString("MM/dd/yy");

        public List<ExternalContact> ExternalContacts { get; private set; }
        int TicketId { get; set; }
        [AlsoNotifyFor("PopUpBackgroundIsVisible")]
        public bool SendOptionsPupupIsVisible { get; private set; }
        [AlsoNotifyFor("PopUpBackgroundIsVisible")]
        public bool AttachActionSheetIsVisible { get; private set; }
        public bool PopUpBackgroundIsVisible => AttachActionSheetIsVisible || SendOptionsPupupIsVisible;
        public bool ReplyIsInternal { get; private set; }


        Dictionary<string, object> Data { get; set; }
        [AlsoNotifyFor("ReplyAttachedFilesIsVisible", "ReplyAttachedFilesListHeight")]
        public ObservableCollection<File> CurrentReplyAttachments { get; set; }
        public bool ReplyAttachedFilesIsVisible => CurrentReplyAttachments != null && CurrentReplyAttachments.Count > 0;

        public string ReplyTextBody { get; set; }
        #endregion
        public override void Init(object initData)
        {
            base.Init(initData);

            FilesToUpload = new List<byte[]>();
            CurrentReplyAttachments = new ObservableCollection<File>();
            Data = initData as Dictionary<string, object>;
            //// need to disable the master-detail nav so the clock functions properly
            //// App.MasterDetailNav.IsGestureEnabled = false;
            var timeNow = DateTime.Now;
            var normalizedTime = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, timeNow.Hour,
            (timeNow.Minute / 15) * 15, 0);
            FromTime = normalizedTime.ToString("h:mm tt");
            ToTime = normalizedTime.AddHours(1).ToString("h:mm tt");
            PickedTimeIsAM = timeNow.Hour < 12;
            CanSendPublicReply = App.UserPermissions.HasFlag(UserPermissions.CanReplyPublicly);
            CanReplyToComments = App.UserPermissions.HasFlag(UserPermissions.CanReplyInternally);
            CanCreateWorkorderAndEvents = App.UserPermissions.HasFlag(UserPermissions.CanAddWorkordersAndEvents);
            CanEditTicketDetails = App.UserPermissions.HasFlag(UserPermissions.CanEditTicketDetails);
            ClockTime.AddYears(1);

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
                var file = new File { Content = bytes, Name = Path.GetFileName(path) };

                CurrentReplyAttachments.Add(file);
                RaisePropertyChanged("ReplyAttachedFilesIsVisible");
                RaisePropertyChanged("ReplyAttachedFilesListHeight");
            }
        }

        public FreshAwaitCommand OnHideDetailsTapped => new FreshAwaitCommand((tcs) =>
                 {
                     PopContentView = null;
                     ShowingTicketDetails = false;
                     ReplyButtonIsVisible = true;
                     tcs?.SetResult(true);
                 });


        public FreshAwaitCommand OnAccessGrantPanelTapped => new FreshAwaitCommand((tcs) =>
                {
                    AccessGrantIsExpanded = !AccessGrantIsExpanded;
                    tcs?.SetResult(true);
                });

        public FreshAwaitCommand OnCloseTicketButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {

                    var result = await CoreMethods.DisplayActionSheet(
                         TicketStatus == TicketStatus.Closed ? $"Do you want to open {TicketTitle}?" : $"Do you want to close {TicketTitle}?",
                        "Cancel",
                        TicketStatus == TicketStatus.Closed ? "Open Ticket" : "Close Ticket");
                    if (result == "Close Ticket" || result == "Open Ticket")
                    {
                        TicketStatus = TicketStatus == TicketStatus.Closed ? TicketStatus.Open : TicketStatus.Closed;
                        OnSaveEditsTapped.Execute(null);
                        OnHideDetailsTapped.Execute(null);
                        foreach (var user in App.Users)
                        {
                            user.IsSelected = false;
                        }
                        foreach (var cat in App.Categories)
                        {
                            cat.IsSelected = false;
                        }
                        foreach (var tag in App.Tags)
                        {
                            tag.IsSelected = false;
                        }
                        await CoreMethods.PopPageModel(data: true, modal: false);
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnSaveEditsTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    if (Categories is null || !Categories.Any(t => t.IsSelected))
                    {
                        await CoreMethods.DisplayAlert("Cannot edit ticket details", "Please select category", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    OnHideDetailsTapped.Execute(null);
                    var ticketPriority = TicketPriorities.Medium;
                    if (PriorityLabelText.ToLower() == "low")
                        ticketPriority = TicketPriorities.Low;
                    else if (PriorityLabelText.ToLower() == "high")
                        ticketPriority = TicketPriorities.High;
                    var requestItem = new Models.UpdateTicketRequestItem
                    {
                        Priority = ticketPriority,
                        TicketID = TicketId,
                        Categories = Categories?.Where(t => t.IsSelected).Select(t => t.CategoryID),
                        Status = TicketStatus,
                        TenantID = TicketTenant,
                        UnitID = Unit?.UnitId,
                        Tags = Tags?.Where(t => t.IsSelected).Select(t => t.TagID),
                        Assigned = Users?.Where(t => t.IsSelected).Select(t => t.UserID),
                        BuildingID = BuildingId,
                        Comment = TicketComment,
                        DueDate = null
                    };
                    try
                    {
                        if (DateTime.TryParse(DueDate, out DateTime d))
                            requestItem.DueDate = d;
                        await Services.DataAccess.UpdateTicket(requestItem);
                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("Unable to update", ex.Message, "DISMISS");
                    }
                    finally
                    {
                        tcs?.SetResult(true);
                    }
                }
                return new FreshAwaitCommand(execute);
            }
        }


        public FreshAwaitCommand OnFirstLineTextTapped
        {
            get
            {
                async void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    //show tenant details as popup
                    if (!CanSelectTenants)
                        return;
                    if (par is Comments tappedComment && tappedComment.CommentType == CommentTypes.Resident)
                    {
                        CurrentTicket.Tenant.TenantDetails = CurrentTicket.TenantDetails;
                        await CoreMethods.PushPageModel<TenantDetailPageModel>(CurrentTicket.Tenant, modal: true);
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnshowDetailsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    if (ShowingTicketDetails)
                    {
                        PopContentView = null;
                        ReplyButtonIsVisible = false;
                    }
                    else
                    {
                        AccessGrantIsExpanded = false;
                        PriorityOptionsVisible = false;
                        DueDateCalendarView = null;
                        AssignedOptionsVisible = false;
                        CategoryOptionsVisible = false;
                        TagOptionsVisible = false;
                        PopContentView = new Views.EditTicketDetailsView(this);
                        ReplyBoxIsVisible = false;
                        WorkOrderActionSheetIsVisible = false;
                        EventActionSheetIsVisible = false;
                        ReplyButtonIsVisible = true;
                    }
                    ReplyButtonIsVisible = !ReplyButtonIsVisible;
                    ShowingTicketDetails = !ShowingTicketDetails;
                    // App.MasterDetailNav.IsGestureEnabled = false;
                    tcs?.SetResult(true);
                }, () => CanEditTicketDetails);
            }
        }

        public FreshAwaitCommand OnShowPriorityOptionsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    PriorityOptionsVisible = !PriorityOptionsVisible;
                    AccessGrantIsExpanded = false;
                    DueDateCalendarView = null;
                    AssignedOptionsVisible = false;
                    TagOptionsVisible = false;
                    CategoryOptionsVisible = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnShowDueDateCalendarTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    AccessGrantIsExpanded = false;
                    PriorityOptionsVisible = false;
                    AssignedOptionsVisible = false;
                    TagOptionsVisible = false;
                    CategoryOptionsVisible = false;
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
                        //if a due date was already set restore it
                        if (DateTime.TryParse(DueDate, out DateTime result))
                            cal.SelectedDate = result;
                        cal.OnSelectedDatesUpdate += (object sender, EventArgs e) =>
                        {
                            DueDate = cal.SelectedDate.ToString("MM/dd/yy");
                        };
                    }

                    tcs?.SetResult(true);
                });
            }
        }


        public FreshAwaitCommand OnShowCategoryOptionsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    AccessGrantIsExpanded = false;
                    PriorityOptionsVisible = false;
                    DueDateCalendarView = null;
                    AssignedOptionsVisible = false;
                    TagOptionsVisible = false;
                    CategoryOptionsVisible = !CategoryOptionsVisible;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnShowAssignedOptionsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    AccessGrantIsExpanded = false;
                    PriorityOptionsVisible = false;
                    DueDateCalendarView = null;
                    TagOptionsVisible = false;
                    CategoryOptionsVisible = false;
                    AssignedOptionsVisible = !AssignedOptionsVisible;
                    tcs?.SetResult(true);
                });
            }
        }


        public FreshAwaitCommand OnShowTagOptionsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    AccessGrantIsExpanded = false;
                    PriorityOptionsVisible = false;
                    DueDateCalendarView = null;
                    AssignedOptionsVisible = false;
                    CategoryOptionsVisible = false;
                    TagOptionsVisible = !TagOptionsVisible;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnRemoveAttachmentTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    File file = (File)par;
                    CurrentReplyAttachments.Remove(file);

                    tcs?.SetResult(true);
                    RaisePropertyChanged("ReplyAttachedFilesIsVisible");
                });
            }
        }

        public FreshAwaitCommand OnSelectEventDateLabelTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    EventCalendarIsVisible = !EventCalendarIsVisible;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnAttachedFileTapped
        {
            get
            {
                async void execute(object param, TaskCompletionSource<bool> tcs)
                {
                    var file = param as File;

                    //get file                            };
                    var requestItem = new MGDataAccessLibrary.Models.CommentFileRequestItem
                    {
                        FileID = file.ID,
                        TicketID = TicketId
                    };

                    IsDownloadingFile = true;
                    var fileData = await Services.DataAccess.GetCommentFile(requestItem);
                    IsDownloadingFile = false;

                    if (fileData is null || string.IsNullOrWhiteSpace(file.Name))
                    {
                        await CoreMethods.DisplayAlert("Something went wrong", "File type not valid", "DISMISS");
                    }
                    else
                    {
                        //permission effective in Android only
                        var canAccessStorage = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                        if (canAccessStorage != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                        {
                            var needsShowAlert = await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                            if (needsShowAlert)
                                await CoreMethods.DisplayAlert("ManageGo", "Need access to storage", "OK");
                            await CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                            canAccessStorage = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                        }

                        if (canAccessStorage != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                        {
                            await CoreMethods.DisplayAlert("ManageGo", "This app requires access to external storage.", "OK");
                            return;
                        }
                        var documentsPath = DependencyService.Get<IShareFile>().GetPublicExternalFolderPath();
                        string filePath = Path.Combine(documentsPath, file.Name);

                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            int length = fileData.Length;
                            fs.Write(fileData, 0, length);
                        }
                        if (Device.RuntimePlatform == Device.iOS)
                            await CoreMethods.PushPageModel<MGWebViewPageModel>(filePath, modal: true);
                        else if (Device.RuntimePlatform == Device.Android)
                            DependencyService.Get<IShareFile>().ShareLocalFile(filePath, file.Name);
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnInternalTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    ReplyIsInternal = true;
                    await SendComment(commentType: MGDataAccessLibrary.Models.CommentTypes.Internal, tcs: tcs);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnPublicTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    ReplyIsInternal = false;
                    await SendComment(commentType: MGDataAccessLibrary.Models.CommentTypes.Management, tcs: tcs);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        async Task SendComment(MGDataAccessLibrary.Models.CommentTypes commentType, TaskCompletionSource<bool> tcs)
        {
            if (string.IsNullOrWhiteSpace(ReplyTextBody))
                await CoreMethods.DisplayAlert("ManageGo", "Please enter reply text", "OK");
            try
            {
                IsCreatingEvent = true;
                var item = new MGDataAccessLibrary.Models.AddNewCommentRequestItem
                {
                    TicketID = TicketId,
                    CommentType = commentType,
                    Comment = ReplyTextBody,
                    IsCompleted = false
                };
                SendOptionsPupupIsVisible = false;
                AttachActionSheetIsVisible = false;
                ReplyBoxIsVisible = false;
                ReplyButtonIsVisible = true;
                // clear the message box
                ReplyTextBody = null;

                RaisePropertyChanged("ReplyAttachedFilesIsVisible");

                int newId = await Services.DataAccess.SendNewCommentAsync(item);

                // only upload files for this partical comment. Other files belong to other comments
                // that were create before or after this comment
                List<Task> uploadTask = new List<Task>();
                foreach (File file in CurrentReplyAttachments)
                {
                    file.ParentComment = newId;
                    uploadTask.Add(Services.DataAccess.UploadFile(file));
                }

                await Task.WhenAll(uploadTask);
                await Services.DataAccess.UploadCompleted(newId);
                CurrentReplyAttachments.Clear();
                OnListRefreshRequested.Execute(null);
                IsCreatingEvent = false;
                tcs?.SetResult(true); // allow another comment to be sent immidiately
            }
            catch (Exception ex)
            {
                IsCreatingEvent = false;
                await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                tcs?.SetResult(true); // allow another comment to be sent immidiately
            }

        }

        public FreshAwaitCommand OnSendReplyTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    /*
                    if (CurrentTicket.Unit is null)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "This ticket is not assigned to a tenant", "OK");
                        tcs?.SetResult(true);
                        return;
                    }*/
                    //show the send options popup
                    if (string.IsNullOrWhiteSpace(ReplyTextBody))
                        await CoreMethods.DisplayAlert("ManageGo", "Please enter reply text", "OK");
                    else
                        SendOptionsPupupIsVisible = !SendOptionsPupupIsVisible;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute, () => CanReplyToComments);
            }
        }

        public FreshAwaitCommand OnAttachButtonTapped => new FreshAwaitCommand((tcs) =>
        {
            AttachActionSheetIsVisible = true;
            tcs?.SetResult(true);
        });



        public FreshAwaitCommand OnBackgroundTapped => new FreshAwaitCommand((tcs) =>
               {
                   SendOptionsPupupIsVisible = false;
                   AttachActionSheetIsVisible = false;
                   tcs?.SetResult(true);
               });



        public FreshAwaitCommand OnBackButtonTapped
        {
            get
            {

                async void execute(TaskCompletionSource<bool> tcs)
                {
                    if (ShouldShowClock || DueDateCalendarView != null)
                    {
                        tcs?.SetResult(true);
                        return;
                    }


                    if (PopContentView != null)
                    {
                        tcs?.SetResult(true);
                        PopContentView = null;
                        ShowingTicketDetails = false;
                        ReplyButtonIsVisible = true;
                        return;
                    }

                    foreach (var user in App.Users)
                    {
                        user.IsSelected = false;
                    }
                    foreach (var cat in App.Categories)
                    {
                        cat.IsSelected = false;
                    }
                    foreach (var tag in App.Tags)
                    {
                        tag.IsSelected = false;
                    }
                    if (ShouldShowClock)
                    {
                        OnCloseTimePickerTapped.Execute(null);
                    }
                    else if (WorkOrderActionSheetIsVisible || EventActionSheetIsVisible ||
                            ReplyBoxIsVisible)
                    {
                        OnCloseReplyBubbleTapped.Execute(null);
                    }
                    else
                        await CoreMethods.PopPageModel(modal: false);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnCloseReplyBubbleTapped => new FreshAwaitCommand((tcs) =>
                {
                    ReplyButtonIsVisible = true;
                    ReplyBoxIsVisible = false;
                    WorkOrderActionSheetIsVisible = false;
                    EventActionSheetIsVisible = false;
                    tcs?.SetResult(true);
                });



        public FreshAwaitCommand OnSwitchPriorityTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
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
            }
        }

        public FreshAwaitCommand OnSendEventButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    /*
                    if (CurrentTicket.Unit is null)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "This ticket is not assigned to a tenant", "OK");
                        tcs?.SetResult(true);
                        return;
                    }*/

                    var _timeFrom = DateTime.Parse(FromTime);
                    var _timeTo = DateTime.Parse(ToTime);
                    if (string.IsNullOrWhiteSpace(EventSummary) ||
                        SelectedEventDate == DateTime.MinValue)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please fill out the event details before sending", "DISMISS");
                        tcs?.SetResult(true);
                        return;
                    }
                    if (EndTimeTextColor == redColor)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Event end time cannot be earlier than event start", "DISMISS");
                        tcs?.SetResult(true);
                        return;
                    }
                    if (!Users.Any(t => t.IsSelected) && !ExternalContacts.Any(t => t.IsSelected))
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please select users and try again", "DISMISS");
                        tcs?.SetResult(true);
                        return;
                    }
                    if (SelectedEventDate.Date <= DateTime.Now.Date && _timeFrom.TimeOfDay < DateTime.Now.TimeOfDay)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Event start time must be in the future", "DISMISS");
                        tcs?.SetResult(true);
                        return;
                    }
                    IsCreatingEvent = true;
                    var tenants = new List<int>();
                    if (TicketTenant != default)
                    {
                        tenants.Add(TicketTenant);
                    }
                    var newEvent = new MGDataAccessLibrary.Models.EventCreateItem
                    {
                        TicketID = TicketId,
                        Note = EventSummary,
                        Title = EventSummary,
                        EventDate = SelectedEventDate,
                        TimeFrom = _timeFrom.ToString("HHmm"),
                        TimeTo = _timeTo.ToString("HHmm"),
                        SendToUsers = Users.Where(t => t.IsSelected).Select(t => t.UserID),
                        SendToExternalContacts = ExternalContacts.Where(t => t.IsSelected).Select(t => t.ExternalID),
                        SendToEmail = WorkOrderSendEmail,
                        SendToTenant = tenants,
                    };

                    RaisePropertyChanged("Comments");
                    try
                    {
                        await MGDataAccessLibrary.BussinessLogic.TicketsProcessor.CreateEvent(newEvent);
                        //clear the fields used for the workorder
                        EventSummary = null;
                        SelectedEventDate = DateTime.MinValue;
                        WorkOrderSendEmail = null;

                        WorkOrderActionSheetIsVisible = false;
                        EventActionSheetIsVisible = false;
                        SendOptionsPupupIsVisible = false;
                        AttachActionSheetIsVisible = false;
                        ReplyBoxIsVisible = false;
                        ReplyButtonIsVisible = true;
                        OnListRefreshRequested.Execute(null);
                        ((TicketDetailsPage)CurrentPage).RedrawTable();

                        SetTicketAsignedUsers();
                    }
                    catch (Exception ex)
                    {
                        IsCreatingEvent = false;
                        Crashes.TrackError(ex);
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                    }
                    finally
                    {
                        IsCreatingEvent = false;
                        tcs?.SetResult(true);
                    }

                }
                return new FreshAwaitCommand(execute, () => CanCreateWorkorderAndEvents);
            }
        }

        /// <summary>
        /// Sets the selected users to the users whom this ticket is assigned to
        /// </summary>
        private void SetTicketAsignedUsers()
        {
            if (Users is null)
                return;
            foreach (var user in Users)
            {
                if (TicketDetails.Assigned != null && TicketDetails.Assigned.Contains(user.UserID))
                    user.IsSelected = true;
                else
                    user.IsSelected = false;
            }
            if (ExternalContacts != null)
            {
                foreach (var user in ExternalContacts)
                {
                    if (TicketDetails.Assigned.Contains(user.ExternalID))
                        user.IsSelected = true;
                    else
                        user.IsSelected = false;
                }
            }
            AssignedLabelText = Users?.FirstOrDefault(t => t.IsSelected)?.UserFullName;
            if (Users.Count(t => t.IsSelected) > 1)
            {
                AssignedLabelText += $", +{Users.Count(t => t.IsSelected) - 1} more";
            }
        }

        public FreshAwaitCommand OnSendWorkOrderButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    /*
                    if (CurrentTicket.Unit is null)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "This ticket is not assigned to a tenant", "OK");
                        tcs?.SetResult(true);
                        return;
                    }*/
                    //send the created work order
                    if (string.IsNullOrWhiteSpace(WorkOrderSummary) || string.IsNullOrWhiteSpace(WorkOrderDetail))
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please fill out the work order details before sending", "DISMISS");
                        tcs?.SetResult(true);
                        return;
                    }
                    else if (!Users.Where(t => t.IsSelected).Any())
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please select a user", "DISMISS");
                        tcs?.SetResult(true);
                        return;
                    }

                    var item = new Models.CreateWorkorderRequestItem
                    {
                        TicketID = TicketId,
                        SendToUsers = Users.Where(t => t.IsSelected).Select(t => t.UserID),
                        Details = WorkOrderDetail,
                        Summary = WorkOrderSummary,
                        SendToEmail = WorkOrderSendEmail,
                        SendToExternalContacts = ExternalContacts.Where(t => t.IsSelected).Select(t => t.ExternalID)
                    };

                    WorkOrderActionSheetIsVisible = false;
                    SendOptionsPupupIsVisible = false;
                    AttachActionSheetIsVisible = false;
                    ReplyBoxIsVisible = false;
                    ReplyButtonIsVisible = true;
                    Comments.Add(new Comments
                    {
                        CommentType = CommentTypes.WorkOrder,
                        Text = $"{WorkOrderSummary}",
                        CommentCreateTime = DateTime.Now.ToShortDateString(),
                        Name = App.UserName
                    });

                    RaisePropertyChanged("Comments");
                    try
                    {
                        IsCreatingEvent = true;
                        await Services.DataAccess.SendNewWorkOurderAsync(item);
                        SetTicketAsignedUsers();
                        //clear the fields used for the workorder
                        WorkOrderSummary = null;
                        WorkOrderDetail = null;
                        WorkOrderSendEmail = null;
                        OnListRefreshRequested.Execute(null);
                        ((TicketDetailsPage)CurrentPage).RedrawTable();
                        SetTicketAsignedUsers();
                    }
                    catch (Exception ex)
                    {
                        IsCreatingEvent = false;
                        Crashes.TrackError(ex);
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                    }
                    finally
                    {
                        IsCreatingEvent = false;
                        tcs?.SetResult(true);
                    }

                }
                return new FreshAwaitCommand(execute, () => CanCreateWorkorderAndEvents);
            }
        }

        public FreshAwaitCommand OnUploadPhotoTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    AttachActionSheetIsVisible = false;
                    try
                    {
                        var result = await Services.PhotoHelper.PickPhotoAndVideos();
                        if (result.Item1 != null)
                        {
                            var file = new File { Content = result.Item1, Name = result.Item2 };

                            CurrentReplyAttachments.Add(file);
                            RaisePropertyChanged("ReplyAttachedFilesIsVisible");
                            RaisePropertyChanged("ReplyAttachedFilesListHeight");
                        }
                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", ex.Message, "Dismiss");
                    }
                    finally
                    {
                        tcs?.SetResult(true);
                    }

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
                        //string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);
                        var file = new File { Content = fileData.DataArray, Name = fileName };

                        CurrentReplyAttachments.Add(file);
                        RaisePropertyChanged("ReplyAttachedFilesIsVisible");
                        RaisePropertyChanged("ReplyAttachedFilesListHeight");
                    }
                    catch (Exception ex)
                    {
                        Crashes.TrackError(ex);
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

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            App.MasterDetailNav.IsGestureEnabled = false;
            CanSelectTenants = App.UserPermissions.HasFlag(UserPermissions.CanAccessTenants);
            if (Data is null)
                return;
            if (Data.TryGetValue("TicketNumber", out object ticketNumber))
                TicketTitle = $"Ticket T{(string)ticketNumber}";
            if (Data.TryGetValue("Address", out object address))
                TicketAddress = (string)address;
            if (Data.TryGetValue("TicketTitleText", out object subject))
                TicketTitleText = (string)subject;
            Data.TryGetValue("Ticket", out object _ticket);
            if (Data.TryGetValue("TicketDetails", out object ticketDetails))
            {
                TicketDetails = ticketDetails as TicketDetails;
                SetupView(TicketDetails, _ticket as MaintenanceTicket);
            }
            Data = null;
        }

        private void SetupView(TicketDetails ticketDetails, MaintenanceTicket ticket)
        {
            if (ticketDetails is null)
                return;
            Categories = new List<Categories>();
            PriorityLabelText = "Not Available";
            Comments = new ObservableCollection<Comments>();
            CategoryLabelText = "Select";
            AssignedLabelText = "Select";
            TagLabelText = "Select";
            foreach (var cat in App.Categories)
            {
                cat.IsSelected = false;
            }
            foreach (var user in App.Users)
            {
                user.IsSelected = false;
            }
            foreach (var tag in App.Tags)
            {
                tag.IsSelected = false;
            }

            CurrentTicket = ticket;
            HasPet = ticket.HasPet;
            HasAccess = ticket.HasAccess;
            HasWorkOrder = ticket.HasWorkorder;
            HasEvent = ticket.HasEvent;
            TicketId = ticket.TicketId;
            Status = ticket.TicketStatus;
            Unit = ticket.Unit;
            TicketTenant = ticket.Tenant.TenantID;
            BuildingId = ticket.Building is null ? 0 : ticket.Building.BuildingId;
            Users = App.Users;
            TicketStatus = ticket.Status ? TicketStatus.Open : TicketStatus.Closed;

            var tenantName = ticket.Tenant.TenantFirstName + " " + ticket.Tenant.TenantLastName;
            TenantName = string.IsNullOrWhiteSpace(tenantName) ? "Not Available" : tenantName;
            DueDate = ticket.DueDate.HasValue ?
                ticket.DueDate.Value.ToString("MM/dd/yy") : "Not Set";
            if (ticketDetails is null && !string.IsNullOrWhiteSpace(ticket.FirstComment))
            {
                Comments.Insert(0, new Comments
                {
                    CommentType = CommentTypes.Resident,
                    Text = ticket.FirstComment,
                    TopSideLineColor = "#00FFFFFF",
                    IsNotTheLastComment = false,
                    CommentCreateTime = ticket.TicketCreateTime.ToLongDateString(),
                    HasPet = ticket.HasPet,
                    HasAccess = ticket.HasAccess
                });
            }
            else
            {
                IsAccessGranted = ticketDetails.IsAccessGranted;
                if (IsAccessGranted)
                {
                    switch (ticketDetails.AccessGrantedObject.PetInUnit)
                    {
                        case 0:
                            AccessPetText = string.Empty;
                            break;
                        case 1:
                            AccessPetText = "Pets in unit: Small pet";
                            break;
                        case 2:
                            AccessPetText = "Pets in unit: Large pet";
                            break;
                    }

                    if (ticketDetails.AccessGrantedObject.Dates is null || !ticketDetails.AccessGrantedObject.Dates.Any())
                    {
                        if (string.IsNullOrWhiteSpace(ticketDetails.AccessGrantedObject.CustomDescription))
                            AccessGrantedTimesText = $"Access anytime{Environment.NewLine}{AccessPetText}";
                        else
                            AccessGrantedTimesText = ticketDetails.AccessGrantedObject.CustomDescription + $"{Environment.NewLine}{AccessPetText}";
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var date in TicketDetails.AccessGrantedObject.Dates)
                        {
                            var tring = $"{date.DateTimeStart.ToString("MMM dd, yyyy")} - {date.DateTimeStart.ToString("h:mm tt")} - {date.DateTimeEnd.ToString("h:mm tt")}{Environment.NewLine}";
                            sb.Append(tring);
                        }
                        sb.Append(AccessPetText);
                        AccessGrantedTimesText = sb.ToString();
                    }
                }
            }

            if (TicketDetails.Comments != null)
            {
                Comments = new ObservableCollection<Comments>(TicketDetails.Comments);
                if (Comments.Any())
                {
                    Comments.First().TopSideLineColor = "#00FFFFFF";
                    Comments.Last().IsNotTheLastComment = false;
                    for (int i = 1; i < Comments.Count; i++)
                    {
                        Comments[i].TopSideLineColor = Comments[i - 1].SideLineColor;
                    }
                }
                  ((TicketDetailsPage)CurrentPage).RedrawTable();
            }

            PriorityLabelText = TicketDetails.Priority;
            // Categories = TicketDetails.Categories;
            Categories = App.Categories;
            Tags = App.Tags;

            if (TicketDetails.Categories != null && TicketDetails.Categories.Any())
            {
                CategoryLabelText = TicketDetails.Categories.First().CategoryName;
                CategoryLabelColor = "#" + TicketDetails.Categories.First().Color;
                Categories.ForEach(cat => cat.IsSelected = TicketDetails.Categories.Any(c => c.CategoryID == cat.CategoryID));

                if (Categories.Count(t => t.IsSelected) > 1)
                {
                    CategoryLabelText = CategoryLabelText + $", +{Categories.Count(t => t.IsSelected) - 1} more";
                    CategoryLabelColor = "#58595B";
                }
                if (Users != null && Users.Any())
                {
                    DisableAllUsers();
                    var TicketCatIds = TicketDetails.Categories.Select(t => t.CategoryID);
                    //user is enabled if any of their categories matches ticket categories
                    Users.ForEach(user => user.IsEnabled = user.Categories is null || !user.Categories.Any() || user.Categories.Any(userCat => TicketCatIds.Contains(userCat)));
                }
            }


            if (TicketDetails.Tags != null && TicketDetails.Tags.Any())
            {
                TagLabelText = TicketDetails.Tags.First().TagName;
                TagLabelColor = "#" + TicketDetails.Tags.First().Color;
                foreach (var tag in Tags)
                {
                    tag.IsSelected = TicketDetails.Tags.Any(c => c.TagID == tag.TagID);
                }
                if (Tags.Count(t => t.IsSelected) > 1)
                {
                    TagLabelText += $", +{Tags.Count(t => t.IsSelected) - 1} more";
                    TagLabelColor = "#58595B";
                }
            }
            ExternalContacts = App.ExternalContacts;
            SetTicketAsignedUsers();

            IsBusy = false;
        }

        public FreshAwaitCommand OnTagTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    var tag = (Tags)par;
                    tag.IsSelected = !tag.IsSelected;
                    if (Tags.Any(t => t.IsSelected == true))
                    {
                        TagLabelText = Tags.First(t => t.IsSelected).TagName;
                        TagLabelColor = "#" + Tags.First(t => t.IsSelected).Color;
                        if (Tags.Count(t => t.IsSelected) > 1)
                        {
                            TagLabelText += $", +{Tags.Count(t => t.IsSelected) - 1} more";
                            TagLabelColor = "#58595B";
                        }
                    }
                    else
                        TagLabelText = string.Empty;
                    tcs?.SetResult(true);
                });
            }
        }



        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            if (App.MasterDetailNav != null)
                App.MasterDetailNav.IsGestureEnabled = true;
            base.ViewIsDisappearing(sender, e);
        }

        void ClearCategorySelections()
        {
            Categories.ForEach(cat => cat.IsSelected = false);
            CategoryLabelText = "Select";
        }

        void EnableAllCategories() => Categories.ForEach(t => t.IsEnabled = true);

        //this should never happen now. New API version needs at least one category selected.
        void DisableAllCategories() => Categories.ForEach(t => t.IsEnabled = false);


        void ClearUserSelections()
        {
            Users.ForEach(u => u.IsSelected = false);
            AssignedLabelText = "Select";
        }


        void EnableAllUsers() => Users.ForEach(u => u.IsEnabled = true);
        void DisableAllUsers() => Users.ForEach(u => u.IsEnabled = false);

        private bool IsSelectedUserCombinationAllowed()
        {
            if (Categories is null || !Categories.Any(t => t.IsSelected))
                return true;
            bool isAllowed = false;
            var selectedCategoryIds = Categories.Where(cat => cat.IsSelected).Select(cat => cat.CategoryID);
            foreach (var user in Users.Where(user => user.IsSelected))
            {

                if (user.Categories is null || user.Categories.Intersect(selectedCategoryIds).Any())
                {
                    isAllowed = true;
                }
            }
            return isAllowed;
        }



        public FreshAwaitCommand OnCategoryTapped
        {
            get
            {
                async void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    var cat = (Categories)par;
                    cat.IsSelected = !cat.IsSelected;
                    if (Categories.Any(t => t.IsSelected) && Categories.Any(t => t.IsSelected) && Users.Any(user => user.IsSelected) && (Users.Any(user => user.IsSelected && user.Categories != null && !user.Categories.Any()) || !IsSelectedUserCombinationAllowed()))
                    {

                        var result = await CoreMethods.DisplayAlert("ManageGo", $"Selected categories are not available to the assigned users. Clear assigned users to update ticket categories.", $"Clear assigned users", "Cancel");
                        if (!result)
                        {
                            cat.IsSelected = !cat.IsSelected;
                            tcs?.SetResult(true);
                            return;
                        }
                        ClearUserSelections();
                        EnableAllCategories();
                    }
                    SetSelecteCategoriesLabel();
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
                    //first check if user can be selected based on selected categories
                    //user should be already disabled based on category selection
                    if (!u.IsSelected && u.Buildings != null && !u.Buildings.Contains(CurrentTicket.Building.BuildingId))
                    {
                        await CoreMethods.DisplayAlert("ManageGo", $"{u.UserFullName} does not have access to the selected building.", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    if (!u.IsEnabled)
                    {
                        var result = await CoreMethods.DisplayAlert("ManageGo", $"{u.UserFullName} does not have access to the selected categories. Selecting this user will clear the selected categories.", $"Select {u.UserFullName}", "Cancel");
                        if (!result)
                        {
                            tcs?.SetResult(true);
                            return;
                        }
                        ClearCategorySelections();
                        EnableAllUsers();
                    }
                    u.IsSelected = !u.IsSelected;
                    //users with access to all categories will have empty category array
                    //users with categories = null have access to no categories
                    if (Users.Any(user => user.IsSelected) && !IsSelectedUserCombinationAllowed())
                    {
                        u.IsSelected = false;
                        await CoreMethods.DisplayAlert($"ManageGo", "This user does not have access to the selected categories.", "OK");
                        Users.ForEach(t => t.IsSelected = false);
                    }
                    SetAssignedUsersLabel();
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        private void SetSelecteCategoriesLabel()
        {
            CategoryLabelText = Categories.FirstOrDefault(t => t.IsSelected)?.CategoryName ?? "Select";
            if (Categories.Count(t => t.IsSelected) > 1)
            {
                CategoryLabelText += $", +{Categories.Count(t => t.IsSelected) - 1} more";
            }
            CategoryLabelColor = "#" + Categories.FirstOrDefault(t => t.IsSelected)?.Color ?? "58595B";
        }

        private void SetAssignedUsersLabel()
        {
            AssignedLabelText = Users.FirstOrDefault(t => t.IsSelected)?.UserFullName ?? "Select";
            if (Users.Count(t => t.IsSelected) > 1)
            {
                AssignedLabelText += $", +{Users.Count(t => t.IsSelected) - 1} more";
            }
        }

        public FreshAwaitCommand OnListRefreshRequested
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    if (IsBusy)
                        return;
                    IsBusy = true;
                    if (TicketId > 0)
                    {
                        TicketDetails = await Services.DataAccess.GetTicketDetails(TicketId);

                        IsBusy = false;
                        ((TicketDetailsPage)CurrentPage).StopRefresh();
                        SetupView(TicketDetails, CurrentTicket);
                        ((TicketDetailsPage)CurrentPage).RedrawTable();
                    }

                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnTakePhotoTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    AttachActionSheetIsVisible = false;
                    var result = await Plugin.Permissions.CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Camera);
                    result &= await Plugin.Permissions.CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Microphone);
                    result &= await Plugin.Permissions.CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                    if (result == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                        await CoreMethods.PushPageModel<TakeVideoPageModel>(true, true, false);
                    else
                    {
                        var showRational = await Plugin.Permissions.CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Camera);
                        if (showRational)
                            await CoreMethods.DisplayAlert("Camera Access", "ManageGo needs access to camera to capture images", "OK");

                        showRational = await Plugin.Permissions.CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                        if (showRational)
                            await CoreMethods.DisplayAlert("Storage Access", "ManageGo needs access to storage to capture videos", "OK");

                        showRational = await Plugin.Permissions.CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Microphone);
                        if (showRational)
                            await CoreMethods.DisplayAlert("Microphone Access", "ManageGo needs access to microphone to capture videos", "OK");

                        await Plugin.Permissions.CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Camera, Plugin.Permissions.Abstractions.Permission.Microphone, Plugin.Permissions.Abstractions.Permission.Storage);
                        result = await Plugin.Permissions.CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Camera);
                        result &= await Plugin.Permissions.CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Microphone);
                        result &= await Plugin.Permissions.CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                        if (result == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                            await CoreMethods.PushPageModel<TakeVideoPageModel>(true, true, false);
                        else
                            await CoreMethods.DisplayAlert("LabLog", "Unable to access camera. Please try again later.", "Dismiss");
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnSelectPMTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    if (PickedTimeIsAM)
                    {
                        var _newTime = PickedTime.Date + new TimeSpan(ClockTime.Hour, ClockTime.Minute, ClockTime.Second);
                        PickedTime = _newTime;
                        SwitchingAmPam = true;
                        var newTime = PickedTime.AddHours(12);
                        PickedTime = newTime;
                        ClockTime = newTime;
                    }
                    PickedTimeIsAM = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnSelectAMTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    if (!PickedTimeIsAM)
                    {
                        var _newTime = PickedTime.Date + new TimeSpan(ClockTime.Hour, ClockTime.Minute, ClockTime.Second);
                        PickedTime = _newTime;
                        SwitchingAmPam = true;
                        var newTime = PickedTime.AddHours(-12);
                        PickedTime = newTime;
                        ClockTime = newTime;
                    }

                    PickedTimeIsAM = true;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnFromTimeLabelTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    OldTime = DateTime.Parse(FromTime.Replace("PM", ""));
                    ShouldShowClock = true;
                    SetToTime = false;
                    PickerTitleText = "Start time:";
                    if (FromTime.EndsWith("pm", StringComparison.CurrentCultureIgnoreCase))
                    {
                        PickedTimeIsAM = false;
                        OldTimeIsPm = true;
                    }
                    else
                    {
                        PickedTimeIsAM = true;
                        OldTimeIsPm = false;
                    }
                    SetFromTime = true;
                    PickedTime = DateTime.Parse(FromTime.Replace("PM", ""));
                    ClockTime = DateTime.Parse(FromTime.Replace("PM", ""));
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnToTimeLabelTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    OldTime = DateTime.Parse(ToTime.Replace("PM", ""));
                    ShouldShowClock = true;
                    SetFromTime = false;
                    PickerTitleText = "End time:";
                    if (ToTime.EndsWith("pm", StringComparison.CurrentCultureIgnoreCase))
                    {
                        OldTimeIsPm = true;
                        PickedTimeIsAM = false;
                    }
                    else
                    {
                        PickedTimeIsAM = true;
                        OldTimeIsPm = false;
                    }
                    SetToTime = true;
                    PickedTime = DateTime.Parse(ToTime.Replace("PM", ""));
                    ClockTime = DateTime.Parse(FromTime.Replace("PM", ""));
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnCloseTimePickerTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    var _oldTime = OldTime;
                    SwitchingAmPam = false;
                    PickedTimeIsAM = !OldTimeIsPm;

                    if (SetToTime)
                        PickedTime = new DateTime(_oldTime.Ticks);
                    else if (SetFromTime)
                        PickedTime = new DateTime(_oldTime.Ticks);
                    ClockTime = PickedTime;
                    RaisePropertyChanged();
                    ShouldShowClock = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnSetTimeFromPickerTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    var _timeString = ClockTime.ToString("hh:mm");
                    _timeString = _timeString + " " + (PickedTimeIsAM ? "AM" : "PM");
                    if (SetFromTime)
                    {
                        FromTime = _timeString;
                    }
                    else if (SetToTime)
                    {
                        ToTime = _timeString;
                    }
                    ShouldShowClock = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnInternalButtonTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    ReplyIsInternal = !ReplyIsInternal;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnReplyLabelTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    /*
                    if (CurrentTicket.Unit is null)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "This ticket is not assigned to a tenant", "OK");
                        tcs?.SetResult(true);
                        return;
                    }*/
                    ReplyButtonIsVisible = false;
                    ReplyBoxIsVisible = true;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnReplyWorkOrderTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    ReplyButtonIsVisible = false;
                    WorkOrderActionSheetIsVisible = true;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute, () => CanCreateWorkorderAndEvents);
            }
        }

        public FreshAwaitCommand OnReplyEventTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    /*
                    if (CurrentTicket.Unit is null)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "This ticket is not assigned to a tenant", "OK");
                        tcs?.SetResult(true);
                        return;
                    }*/
                    ReplyButtonIsVisible = false;
                    EventActionSheetIsVisible = true;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute, () => CanCreateWorkorderAndEvents);
            }
        }


    }
}

