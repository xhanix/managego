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

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class TicketDetailsPageModel : FreshBasePageModel
    {
        public TicketDetails TicketDetails { get; private set; }
        public ObservableCollection<Comments> Comments { get; private set; }
        public string TicketTitle { get; set; }
        List<byte[]> FilesToUpload { get; set; }
        public string BuildingUnitText { get; private set; }
        public View PopContentView { get; private set; }
        public DateTime OldTime { get; private set; }
        public bool ShouldShowClock { get; private set; }
        [AlsoNotifyFor("PriorityOptionsRowIcon")]
        public bool PriorityOptionsVisible { get; private set; }
        [AlsoNotifyFor("CategoryOptionsRowIcon")]
        public bool CategoryOptionsVisible { get; private set; }
        [AlsoNotifyFor("TagOptionsRowIcon")]
        public bool TagOptionsVisible { get; private set; }
        public string CategoryLabelText { get; private set; }
        public string CategoryLabelColor { get; private set; }
        public string TagLabelText { get; private set; }
        public string TagLabelColor { get; private set; }
        public string DueDate { get; private set; }
        public List<Categories> Categories { get; private set; }
        public List<Tags> Tags { get; private set; }

        public string PriorityOptionsRowIcon
        {
            get { return PriorityOptionsVisible ? "chevron_down.png" : "chevron_right.png"; }
        }
        public string CategoryOptionsRowIcon
        {
            get { return CategoryOptionsVisible ? "chevron_down.png" : "chevron_right.png"; }
        }
        public string TagOptionsRowIcon
        {
            get { return TagOptionsVisible ? "chevron_down.png" : "chevron_right.png"; }
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
        public bool HasPet { get; private set; }
        public bool HasAccess { get; private set; }
        public bool HasWorkOrder { get; private set; }
        public bool HasEvent { get; private set; }
        public bool ReplyButtonIsVisible { get; private set; } = true;
        public bool ReplyBoxIsVisible { get; private set; }
        public bool IsDownloadingFile { get; private set; }
        public bool WorkOrderActionSheetIsVisible { get; private set; }
        public bool EventActionSheetIsVisible { get; private set; }
        public LayoutOptions FromFrameVerticalLayout { get; private set; }
        public LayoutOptions ToFrameVerticalLayout { get; private set; }
        bool IsToTimeSelected { get; set; }
        public List<User> Users { get; private set; }
        DateTime pickedTime;
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
        public ObservableCollection<File> CurrentReplyAttachments { get; set; }
        public string ReplyTextBody { get; set; }
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
            }
        }
        public FreshAwaitCommand OnHideDetailsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    PopContentView = null;
                    ReplyButtonIsVisible = false;
                    tcs?.SetResult(true);
                });
            }
        }
        public FreshAwaitCommand OnCloseTicketButtonTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    //todo Close ticket
                });
            }
        }
        public FreshAwaitCommand OnSaveEditsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    //todo save ticket detail edits
                    PopContentView = null;
                    ReplyButtonIsVisible = true;
                });
            }
        }
        public FreshAwaitCommand OnshowDetailsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    PopContentView = new Views.EditTicketDetailsView(this);
                    ReplyButtonIsVisible = false;
                    OnCloseReplyBubbleTapped.Execute(null);
                    tcs?.SetResult(true);
                });
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
                return new FreshAwaitCommand((tcs) =>
                {
                    //show the send options popup
                    SendOptionsPupupIsVisible = !SendOptionsPupupIsVisible;
                    tcs?.SetResult(true);

                }, () => !string.IsNullOrWhiteSpace(ReplyTextBody));
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
                        await CoreMethods.PopPageModel(false);
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
                            user.IsSelected = false;
                        }
                        foreach (var user in ExternalContacts.Where(t => t.IsSelected))
                        {
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
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnSendWorkOrderButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    //send the created work order
                    if (string.IsNullOrWhiteSpace(WorkOrderSummary) ||
                                                                                                         string.IsNullOrWhiteSpace(WorkOrderDetail))
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
                            user.IsSelected = false;
                        }
                        foreach (var user in ExternalContacts.Where(t => t.IsSelected))
                        {
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
                    var result = await Services.PhotoHelper.AddNewPhoto(fromAlbum: true);
                    if (result.Item1 != null)
                    {
                        var file = new File { Content = result.Item1, Name = result.Item2 };
                        ReplyAttachments.Add(file);
                        CurrentReplyAttachments.Add(file);
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

                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            if (Data is null)
                return;
            if (Data.TryGetValue("TicketDetails", out object ticketDetails))
            {
                TicketDetails = ticketDetails as TicketDetails;
                if (TicketDetails != null)
                {
                    Comments = new ObservableCollection<Comments>(TicketDetails.Comments);
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
                            if (TicketDetails.Categories.Any(c => c.CategoryName == cat.CategoryName))
                            {
                                cat.IsSelectedForFiltering = true;
                            }
                            else
                            {
                                cat.IsSelectedForFiltering = false;
                            }
                        }
                    }
                    if (TicketDetails.Tags != null && TicketDetails.Tags.Any())
                    {
                        TagLabelText = TicketDetails.Tags.First().TagName;
                        TagLabelColor = "#" + TicketDetails.Tags.First().Color;
                        foreach (var tag in Tags)
                        {
                            if (TicketDetails.Tags.Any(c => c.TagName == tag.TagName))
                            {
                                tag.IsSelectedForFiltering = true;
                            }
                            else
                            {
                                tag.IsSelectedForFiltering = false;
                            }
                        }
                    }
                }
                else
                {
                    Categories = new List<Categories>();
                    PriorityLabelText = "Not Available";
                    Comments = new ObservableCollection<Comments>();
                }

            }
            if (Data.TryGetValue("Ticket", out object _ticket))
            {
                var ticket = (MaintenanceTicket)_ticket;
                HasPet = ticket.HasPet;
                HasAccess = ticket.HasAccess;
                HasWorkOrder = ticket.HasWorkorder;
                HasEvent = ticket.HasEvent;
                TicketId = ticket.TicketId;
                Users = App.Users;
                if (ticket.Assigned.Any())
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
                DueDate = ticket.DueDate;

            }

            if (Data.TryGetValue("TicketNumber", out object ticketNumber))
                TicketTitle = $"Ticket #{(string)ticketNumber}";
            if (Data.TryGetValue("Address", out object address))
                TicketAddress = (string)address;

            if (Data.TryGetValue("TicketTitleText", out object subject))
                TicketTitleText = (string)subject;


            ExternalContacts = App.ExternalContacts;
        }



        public FreshAwaitCommand OnTakePhotoTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    AttachActionSheetIsVisible = false;
                    await CoreMethods.PushPageModel<TakeVideoPageModel>(true, true, false);
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
                });
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
                });
            }
        }

        public bool SetFromTime { get; private set; }
        public bool SwitchingAmPam { get; private set; }
        public bool OldTimeIsPm { get; private set; }
        public string TenantName { get; private set; }
    }
}

