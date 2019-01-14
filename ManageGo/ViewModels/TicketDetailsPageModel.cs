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
        #endregion
        #region Properties
        public bool SetFromTime { get; private set; }
        public bool SwitchingAmPam { get; private set; }
        public bool OldTimeIsPm { get; private set; }
        public bool IsBusy { get; private set; }
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
        [AlsoNotifyFor("PriorityOptionsRowIcon")]
        public bool PriorityOptionsVisible { get; private set; }
        [AlsoNotifyFor("CategoryOptionsRowIcon")]
        public bool CategoryOptionsVisible { get; private set; }
        [AlsoNotifyFor("AssignedOptionsRowIcon")]
        public bool AssignedOptionsVisible { get; private set; }
        [AlsoNotifyFor("TagOptionsRowIcon")]
        public bool TagOptionsVisible { get; private set; }
        public string CategoryLabelText { get; private set; } = String.Empty;
        public string AssignedLabelText { get; private set; } = String.Empty;
        public string CategoryLabelColor { get; private set; } = "#424242";
        public string TagLabelText { get; private set; } = String.Empty;
        public string TagLabelColor { get; private set; } = "#424242";
        public string DueDate { get; private set; } = String.Empty;
        public List<Categories> Categories { get; private set; }
        public List<Tags> Tags { get; private set; }
        int TicketStatus { get; set; }
        public double ReplyAttachedFilesListHeight { get { return CurrentReplyAttachments is null || !CurrentReplyAttachments.Any() ? 0 : CurrentReplyAttachments.Count * 28; } }

        public string PriorityOptionsRowIcon
        {
            get { return PriorityOptionsVisible ? "chevron_down.png" : "chevron_right.png"; }
        }
        public string CategoryOptionsRowIcon
        {
            get { return CategoryOptionsVisible ? "chevron_down.png" : "chevron_right.png"; }
        }
        public string AssignedOptionsRowIcon
        {
            get { return AssignedOptionsVisible ? "chevron_down.png" : "chevron_right.png"; }
        }
        public string TagOptionsRowIcon
        {
            get { return TagOptionsVisible ? "chevron_down.png" : "chevron_right.png"; }
        }
        public string DueDateRowIcon
        {
            get { return DueDateCalendarView != null ? "chevron_down.png" : "chevron_right.png"; }
        }
        public bool SetToTime { get; private set; }
        public bool ListIsEnabled { get; private set; } = true;
        public string TicketAddress { get; private set; }
        public string TicketTitleText { get; private set; }
        public string PriorityLabelTextColor
        {
            get
            {
                return PriorityLabelText.ToLower() == "low" ? "#949494" :
                    PriorityLabelText.ToLower() == "medium" ? "#e0a031" : "#E13D40";
            }
        }
        [AlsoNotifyFor("PriorityLabelTextColor")]
        public string PriorityLabelText { get; private set; }
        public string LowPriorityRadioIcon { get { return PriorityLabelText.ToLower() == "low" ? "radio_selected.png" : "radio_unselected.png"; } }
        public string MediumPriorityRadioIcon { get { return PriorityLabelText.ToLower() == "medium" ? "radio_selected.png" : "radio_unselected.png"; } }
        public string HighPriorityRadioIcon { get { return PriorityLabelText.ToLower() == "high" ? "radio_selected.png" : "radio_unselected.png"; } }

        public MaintenanceTicket CurrentTicket { get; private set; }
        public bool HasPet { get; private set; }
        public bool HasAccess { get; private set; }
        public bool HasWorkOrder { get; private set; }
        public bool HasEvent { get; private set; }
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
        private List<int> AssignedUserIds { get; set; }
        DateTime pickedTime;
        private int numberOfTries;

        [AlsoNotifyFor("EndTimeTextColor")]
        public string ToTime { get; set; }
        [AlsoNotifyFor("EndTimeTextColor")]
        public string FromTime { get; set; }
        public string PickerTitleText { get; private set; }
        public int BridgeColumn { get; private set; }
        [AlsoNotifyFor("AmButtonImage", "PmButtonImage")]
        public bool PickedTimeIsAM { get; set; }
        public string CurrentTime { get; private set; }
        public string AmButtonImage
        {
            get { return PickedTimeIsAM ? "am_active.png" : "am_inactive.png"; }
        }
        public string PmButtonImage
        {
            get { return !PickedTimeIsAM ? "pm_active.png" : "pm_inactive.png"; }
        }
        readonly string redColor = "#fc3535";
        public string EndTimeTextColor
        {
            get
            {
                if (DateTime.Parse(FromTime) > DateTime.Parse(ToTime))
                    return redColor;
                return "#9b9b9b";
            }
        }
        [AlsoNotifyFor("FromTime", "ToTime", "CurrentTime")]
        public DateTime PickedTime
        {
            get { return pickedTime; }
            set
            {
                if (!PickedTimeIsAM)
                    pickedTime = !PickedTimeIsAM && !SwitchingAmPam ? value.AddHours(12) : value;
                else
                    pickedTime = value;
                if (SetToTime)
                    ToTime = pickedTime.ToString("h:mm tt");
                else if (SetFromTime)
                    FromTime = pickedTime.ToString("h:mm tt");
                CurrentTime = pickedTime.ToString("h:mm");
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

        public string SelectedEventDateString
        {
            get
            {
                return SelectedEventDate == DateTime.MinValue ? "Select..." : SelectedEventDate.ToString("MM/dd/yy");
            }
        }
        public List<ExternalContact> ExternalContacts { get; private set; }
        int TicketId { get; set; }
        [AlsoNotifyFor("PopUpBackgroundIsVisible")]
        public bool SendOptionsPupupIsVisible { get; private set; }
        [AlsoNotifyFor("PopUpBackgroundIsVisible")]
        public bool AttachActionSheetIsVisible { get; private set; }
        public bool PopUpBackgroundIsVisible
        {
            get { return AttachActionSheetIsVisible || SendOptionsPupupIsVisible; }
        }
        public bool ReplyIsInternal { get; private set; }

        public List<File> ReplyAttachments { get; set; }
        Dictionary<string, object> Data { get; set; }
        [AlsoNotifyFor("ReplyAttachedFilesIsVisible", "ReplyAttachedFilesListHeight")]
        public ObservableCollection<File> CurrentReplyAttachments { get; set; }
        public bool ReplyAttachedFilesIsVisible
        {
            get
            {
                return CurrentReplyAttachments != null && CurrentReplyAttachments.Count > 0;
            }
        }
        public string ReplyTextBody { get; set; }
        #endregion
        public override void Init(object initData)
        {
            base.Init(initData);
            ReplyAttachments = new List<File>();
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
                ReplyAttachments.Add(file);
                CurrentReplyAttachments.Add(file);
                RaisePropertyChanged("ReplyAttachedFilesIsVisible");
                RaisePropertyChanged("ReplyAttachedFilesListHeight");
            }
        }

        public FreshAwaitCommand OnHideDetailsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    PopContentView = null;
                    ShowingTicketDetails = false;
                    ReplyButtonIsVisible = true;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnCloseTicketButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {

                    var result = await CoreMethods.DisplayActionSheet($"Do you want to close {TicketTitle}?", "Cancel", "Close Ticket");
                    if (result == "Close Ticket")
                    {
                        TicketStatus = 1;
                        OnSaveEditsTapped.Execute(null);
                        OnHideDetailsTapped.Execute(null);
                        await CoreMethods.PopPageModel(data: CurrentTicket, modal: false);
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
                    PopContentView = null;
                    ReplyButtonIsVisible = true;
                    var ticketPriority = 2;
                    if (PriorityLabelText.ToLower() == "low")
                        ticketPriority = 0;
                    else if (PriorityLabelText.ToLower() == "medium")
                        ticketPriority = 1;
                    try
                    {
                        Dictionary<string, object> parameters = new Dictionary<string, object>
                        {
                            { "TicketID", TicketId },
                            { "Categories", Categories.Where(t=>t.IsSelected) },
                            { "Status", TicketStatus },
                            { "Priority", ticketPriority },
                            { "TenantID", TicketTenant},
                            { "UnitID", Unit.UnitId},
                            { "Tags", Tags.Where(t=>t.IsSelected) },
                            { "Assigned", Users.Where(t=>t.IsSelected) },
                            { "BuildingID", BuildingId },
                            { "Comment", TicketComment },

                        };
                        if (DateTime.TryParse(DueDate, out DateTime d))
                            parameters.Add("DueDate", d);
                        await Services.DataAccess.UpdateTicket(parameters);
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
                        PopContentView = new Views.EditTicketDetailsView(this);
                        ReplyBoxIsVisible = false;
                        WorkOrderActionSheetIsVisible = false;
                        EventActionSheetIsVisible = false;
                        ReplyButtonIsVisible = true;
                    }
                    ReplyButtonIsVisible = !ReplyButtonIsVisible;
                    ShowingTicketDetails = !ShowingTicketDetails;
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
                    if (DueDateCalendarView != null)
                        DueDateCalendarView = null;
                    else
                    {
                        var cal = new Controls.CalendarView();
                        cal.HeightRequest = 240;
                        DueDateCalendarView = cal;
                        cal.AllowMultipleSelection = false;
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

                    //get file
                    var dic = new Dictionary<string, object> {
                        {"TicketID", TicketId},
                        {"FileID", file.ID}
                                                                                               };

                    IsDownloadingFile = true;
                    var fileData = await Services.DataAccess.GetCommentFile(dic);
                    IsDownloadingFile = false;

                    if (fileData is null || string.IsNullOrWhiteSpace(file.Name))
                    {
                        await CoreMethods.DisplayAlert("Something went wrong", "File type not valid", "DISMISS");
                    }
                    else
                    {
                        //save the file to Documents folder and pass path to webview
                        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
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
                    await SendComment(commentType: CommentTypes.Internal, tcs: tcs);
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
                    await SendComment(commentType: CommentTypes.Management, tcs: tcs);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        async Task SendComment(CommentTypes commentType, TaskCompletionSource<bool> tcs)
        {
            if (string.IsNullOrWhiteSpace(ReplyTextBody))
                await CoreMethods.DisplayAlert("ManageGo", "Please enter reply text", "OK");
            try
            {
                var dic = new Dictionary<string, object> {
                    {"TicketID", TicketId},
                    {"CommentType", commentType},
                    {"Comment", ReplyTextBody},
                    {"IsCompleted", false}
                };
                SendOptionsPupupIsVisible = false;
                AttachActionSheetIsVisible = false;
                ReplyBoxIsVisible = false;
                ReplyButtonIsVisible = true;
                Comments.Add(new ManageGo.Comments
                {
                    CommentType = commentType,
                    Text = $"{ReplyTextBody}",
                    CommentCreateTime = DateTime.Now.ToShortDateString(),
                    Name = App.UserName,
                    Files = new ObservableCollection<File>(ReplyAttachments.Where(t => t.ParentComment == 0))
                });
                RaisePropertyChanged("Comments");

                // First assing a temp id for later identification. In case multiple comments
                // get created quickly before all files are uploaded
                var tempId = Guid.NewGuid().ToString();
                foreach (File file in ReplyAttachments.Where(t => t.ParentComment == 0))
                {
                    file.ParentCommentTempId = tempId;
                }
                // wait for the correct comment id. Another comment with attachment can be created
                // while waiting for this.
                ReplyTextBody = null; // clear the message box
                CurrentReplyAttachments.Clear();
                RaisePropertyChanged("ReplyAttachedFilesIsVisible");
                tcs?.SetResult(true); // allow another comment to be sent immidiately
                int newId = await Services.DataAccess.SendNewCommentAsync(dic);
                // reassign the the real comment id to the file with the temp Id
                foreach (File file in ReplyAttachments.Where(t => t.ParentCommentTempId == tempId))
                {
                    file.ParentComment = newId;
                }
                // only upload files for this partical comment. Other files belong to other comments
                // that were create before or after this comment
                List<Task> uploadTask = new List<Task>();
                foreach (File file in ReplyAttachments.Where(t => t.ParentComment == newId))
                {
                    uploadTask.Add(Services.DataAccess.UploadFile(file));
                }
                await Task.WhenAll(uploadTask);
                if (ReplyAttachments.Any(t => t.ParentComment == newId))
                    await Services.DataAccess.UploadCompleted(commentId: newId);

            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
            }

        }

        public FreshAwaitCommand OnSendReplyTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
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

        public FreshAwaitCommand OnAttachButtonTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    AttachActionSheetIsVisible = true;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnBackgroundTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    SendOptionsPupupIsVisible = false;
                    AttachActionSheetIsVisible = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnBackButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    if (ShouldShowClock)
                    {
                        OnCloseTimePickerTapped.Execute(null);
                    }
                    else if (WorkOrderActionSheetIsVisible || EventActionSheetIsVisible ||
                            ReplyBoxIsVisible)
                    {
                        OnCloseReplyBubbleTapped.Execute(null);
                    }
                    else if (PopContentView != null)
                    {
                        OnHideDetailsTapped.Execute(null);
                    }
                    else
                        await CoreMethods.PopPageModel(modal: false);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnCloseReplyBubbleTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    ReplyButtonIsVisible = true;
                    ReplyBoxIsVisible = false;
                    WorkOrderActionSheetIsVisible = false;
                    EventActionSheetIsVisible = false;
                    tcs?.SetResult(true);
                });
            }
        }

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

                    var dic = new Dictionary<string, object> {
                        {"TicketID", TicketId},
                        {"Note", EventSummary},
                        {"Title", EventSummary},
                        {"EventDate", SelectedEventDate},
                        {"TimeFrom", _timeFrom.ToString("HHmm")},
                        {"TimeTo", _timeTo.ToString("HHmm")},
                        {"SendToUsers", Users.Where(t=>t.IsSelected).Select(t=>t.UserID)},
                        {"SendToExternalContacts", ExternalContacts.Where(t=>t.IsSelected).Select(t=>t.ExternalID) },
                                                                                                 };
                    WorkOrderActionSheetIsVisible = false;
                    EventActionSheetIsVisible = false;
                    SendOptionsPupupIsVisible = false;
                    AttachActionSheetIsVisible = false;
                    ReplyBoxIsVisible = false;
                    ReplyButtonIsVisible = true;
                    Comments.Add(new Comments
                    {
                        CommentType = CommentTypes.Event,
                        Text = $"{EventSummary}",
                        CommentCreateTime = DateTime.Now.ToShortDateString(),
                        Name = App.UserName
                    });
                    RaisePropertyChanged("Comments");
                    try
                    {
                        await Services.DataAccess.SendNewEventAsync(dic);
                        foreach (var user in Users.Where(t => t.IsSelected))
                        {
                            if (AssignedUserIds.Contains(user.UserID))
                                user.IsSelected = true;
                            else
                                user.IsSelected = false;
                        }
                        foreach (var user in ExternalContacts.Where(t => t.IsSelected))
                        {
                            if (AssignedUserIds.Contains(user.ExternalID))
                                user.IsSelected = true;
                            else
                                user.IsSelected = false;
                        }
                        //clear the fields used for the workorder
                        EventSummary = null;
                        SelectedEventDate = DateTime.MinValue;
                        WorkOrderSendEmail = null;
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
                return new FreshAwaitCommand(execute, () => CanCreateWorkorderAndEvents);
            }
        }

        public FreshAwaitCommand OnSendWorkOrderButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    //send the created work order
                    if (string.IsNullOrWhiteSpace(WorkOrderSummary) || string.IsNullOrWhiteSpace(WorkOrderDetail))
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please fill out the work order details before sending", "DISMISS");
                        tcs?.SetResult(true);
                        return;
                    }

                    var dic = new Dictionary<string, object> {
                        {"TicketID", TicketId},
                        {"Summary", WorkOrderSummary},
                        {"Details", WorkOrderDetail},
                        {"SendToUsers", Users.Where(t=>t.IsSelected).Select(t=>t.UserID)},
                        {"SendToExternalContacts", ExternalContacts.Where(t=>t.IsSelected).Select(t=>t.ExternalID) },
                        {"SendToEmail", WorkOrderSendEmail}
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
                        await Services.DataAccess.SendNewWorkOurderAsync(dic);
                        foreach (var user in Users.Where(t => t.IsSelected))
                        {
                            if (AssignedUserIds.Contains(user.UserID))
                                user.IsSelected = true;
                            else
                                user.IsSelected = false;
                        }
                        foreach (var user in ExternalContacts.Where(t => t.IsSelected))
                        {
                            if (AssignedUserIds.Contains(user.ExternalID))
                                user.IsSelected = true;
                            else
                                user.IsSelected = false;
                        }
                        //clear the fields used for the workorder
                        WorkOrderSummary = null;
                        WorkOrderDetail = null;
                        WorkOrderSendEmail = null;
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
                    var result = await Services.PhotoHelper.PickPhotoAndVideos();
                    if (result.Item1 != null)
                    {
                        var file = new File { Content = result.Item1, Name = result.Item2 };
                        ReplyAttachments.Add(file);
                        CurrentReplyAttachments.Add(file);
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
                        //string contents = System.Text.Encoding.UTF8.GetString(fileData.DataArray);
                        var file = new File { Content = fileData.DataArray, Name = fileName };
                        ReplyAttachments.Add(file);
                        CurrentReplyAttachments.Add(file);
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

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
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
        }

        private void SetupView(TicketDetails ticketDetails, MaintenanceTicket ticket)
        {
            Categories = new List<Categories>();
            PriorityLabelText = "Not Available";
            Comments = new ObservableCollection<Comments>();
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
            if (ticket != null)
            {
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
                TicketComment = ticket.FirstComment;

                AssignedUserIds = ticket.Assigned;
                if (ticket.Assigned != null && ticket.Assigned.Any())
                {
                    foreach (var user in Users)
                    {
                        if (ticket.Assigned.Contains(user.UserID))
                        {
                            user.IsSelected = true;
                        }
                        else
                        {
                            user.IsSelected = false;
                        }
                    }
                }
                var tenantName = ticket.Tenant.TenantFirstName + " " + ticket.Tenant.TenantLastName;
                TenantName = string.IsNullOrWhiteSpace(tenantName) ? "Not Available" : tenantName;
                DueDate = ticket.DueDate.HasValue ?
                    ticket.DueDate.Value.ToString("MM/dd/yy") : "Not Set";
                if (ticketDetails is null)
                {
                    if (!string.IsNullOrWhiteSpace(TicketComment))
                        Comments.Insert(0, new Comments
                        {
                            CommentType = CommentTypes.Resident,
                            Text = TicketComment,
                            TopSideLineColor = "#00FFFFFF",
                            IsNotTheLastComment = false,
                            CommentCreateTime = ticket.TicketCreateTime.ToLongDateString(),
                            HasPet = ticket.HasPet,

                            HasAccess = ticket.HasAccess
                        });
                }
            }
            if (ticketDetails is null)
            {
                return;
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
            }
            PriorityLabelText = TicketDetails.Priority;
            // Categories = TicketDetails.Categories;
            Categories = App.Categories;
            Tags = App.Tags;
            if (TicketDetails.Categories != null && TicketDetails.Categories.Any())
            {
                CategoryLabelText = TicketDetails.Categories.First().CategoryName;
                CategoryLabelColor = "#" + TicketDetails.Categories.First().Color;
                foreach (var cat in Categories)
                {
                    if (TicketDetails.Categories.Any(c => c.CategoryID == cat.CategoryID))
                    {
                        cat.IsSelected = true;
                    }
                }
                if (Categories.Count(t => t.IsSelected) > 1)
                {
                    CategoryLabelText = CategoryLabelText + $", +{Categories.Count(t => t.IsSelected) - 1} more";
                    CategoryLabelColor = "#58595B";
                }
            }
            if (TicketDetails.Tags != null && TicketDetails.Tags.Any())
            {
                TagLabelText = TicketDetails.Tags.First().TagName;
                TagLabelColor = "#" + TicketDetails.Tags.First().Color;
                foreach (var tag in Tags)
                {
                    if (TicketDetails.Tags.Any(c => c.TagID == tag.TagID))
                    {
                        tag.IsSelected = true;
                    }
                }
                if (Tags.Count(t => t.IsSelected) > 1)
                {
                    TagLabelText = TagLabelText + $", +{Tags.Count(t => t.IsSelected) - 1} more";
                    TagLabelColor = "#58595B";
                }
            }

            ExternalContacts = App.ExternalContacts;
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
                            TagLabelText = TagLabelText + $", +{Tags.Count(t => t.IsSelected) - 1} more";
                            TagLabelColor = "#58595B";
                        }
                    }
                    else
                        TagLabelText = string.Empty;
                    tcs?.SetResult(true);
                });
            }
        }

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
                            if (u.Categories.Intersect(selectedCats).Count() == selectedCats.Count())
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
                        if (!Categories.Any(c => c.IsSelected))
                            EnableAllUsers();
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
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
                        SetupView(TicketDetails, null);
                        IsBusy = false;
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
                    if (await CheckCameraPermissionsAsync())
                    {
                        AttachActionSheetIsVisible = false;
                        await CoreMethods.PushPageModel<TakeVideoPageModel>(true, true, false);
                    }
                    else if (numberOfTries >= 2)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Unable to take photo or video. You did not allow access to camera.", "DISMISS");
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

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
                        numberOfTries++;
                        return await CheckCameraPermissionsAsync();
                    }
                    return false;
                }
                return true;
            }
            return true;
        }

        public FreshAwaitCommand OnSelectPMTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    if (PickedTimeIsAM)
                    {
                        SwitchingAmPam = true;
                        PickedTime = PickedTime.AddHours(12);
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
                        SwitchingAmPam = true;
                        PickedTime = PickedTime.AddHours(-12);
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
                    RaisePropertyChanged("PickedTime");
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
                    var _timeString = PickedTime.ToString("hh:mm tt");
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
                return new FreshAwaitCommand((tcs) =>
                {
                    ReplyButtonIsVisible = false;
                    ReplyBoxIsVisible = true;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnReplyWorkOrderTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    ReplyButtonIsVisible = false;
                    WorkOrderActionSheetIsVisible = true;
                    tcs?.SetResult(true);
                }, () => CanCreateWorkorderAndEvents);
            }
        }

        public FreshAwaitCommand OnReplyEventTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    ReplyButtonIsVisible = false;
                    EventActionSheetIsVisible = true;
                    tcs?.SetResult(true);
                }, () => CanCreateWorkorderAndEvents);
            }
        }
    }
}

