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
        public bool ShouldShowClock { get; private set; }
        public bool SetToTime { get; private set; }
        public bool ListIsEnabled { get; private set; } = true;
        public string TicketAddress { get; private set; }
        public string TicketTitleText { get; private set; }
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
        string pickedTime;
        public string ToTime { get; set; }
        public string FromTime { get; set; }
        public int BridgeColumn { get; private set; }
        [AlsoNotifyFor("FromTime", "ToTime")]
        public string PickedTime
        {
            get { return pickedTime; }
            set
            {
                pickedTime = value;
                if (SetToTime)
                    ToTime = $"{pickedTime}";
                else if (SetFromTime)
                    FromTime = $"{pickedTime}";
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
            //need to disable the master-detail nav so the clock functions properly
            App.MasterDetailNav.IsGestureEnabled = false;
            var timeNow = DateTime.Now;
            var normalizedTime = new DateTime(timeNow.Year, timeNow.Month, timeNow.Day, timeNow.Hour,
            (timeNow.Minute / 15) * 15, 0);
            FromTime = normalizedTime.ToString("hh:mm tt");
            ToTime = normalizedTime.AddHours(1).ToString("hh:mm tt");
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
                return new FreshAwaitCommand(async (param, tcs) =>
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
                });
            }
        }

        public FreshAwaitCommand OnInternalTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    ReplyIsInternal = true;
                    await SendComment(commentType: CommentTypes.Internal, tcs: tcs);
                });
            }
        }

        public FreshAwaitCommand OnPublicTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    ReplyIsInternal = false;
                    await SendComment(commentType: CommentTypes.Management, tcs: tcs);
                });
            }
        }

        async Task SendComment(CommentTypes commentType, TaskCompletionSource<bool> tcs)
        {
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

                });
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
                return new FreshAwaitCommand(async (tcs) =>
                {
                    await CoreMethods.PopPageModel(false);
                    tcs?.SetResult(true);
                });
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

        public FreshAwaitCommand OnSendWorkOrderButtonTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    //send the created work order
                    if (string.IsNullOrWhiteSpace(WorkOrderSummary) ||
                        string.IsNullOrWhiteSpace(WorkOrderDetail))
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Please fill out the work order details before sending", "DISMISS");
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
                    Comments.Add(new ManageGo.Comments
                    {
                        CommentType = CommentTypes.WorkOrder,
                        Text = $"{WorkOrderSummary}",

                        CommentCreateTime = DateTime.Now.ToShortDateString(),
                        Name = App.UserName
                    });
                    WorkOrderActionSheetIsVisible = false;
                    SendOptionsPupupIsVisible = false;
                    AttachActionSheetIsVisible = false;
                    ReplyBoxIsVisible = false;
                    ReplyButtonIsVisible = true;
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

                });
            }
        }

        public FreshAwaitCommand OnUploadPhotoTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
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
                });
            }
        }

        public FreshAwaitCommand OnUploadFileTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
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
               });
            }
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            if (Data is null)
                return;
            if (Data.TryGetValue("TicketDetails", out object ticketDetails))
            {
                TicketDetails = (TicketDetails)ticketDetails;
                Comments = new ObservableCollection<Comments>(TicketDetails.Comments);
            }
            if (Data.TryGetValue("Ticket", out object _ticket))
            {
                var ticket = (MaintenanceTicket)_ticket;
                HasPet = ticket.HasPet;
                HasAccess = ticket.HasAccess;
                HasWorkOrder = ticket.HasWorkorder;
                HasEvent = ticket.HasEvent;
                TicketId = ticket.TicketId;
            }

            if (Data.TryGetValue("TicketNumber", out object ticketNumber))
                TicketTitle = $"Ticket #{(string)ticketNumber}";
            if (Data.TryGetValue("Address", out object address))
                TicketAddress = (string)address;

            if (Data.TryGetValue("TicketTitleText", out object subject))
                TicketTitleText = (string)subject;

            Users = App.Users;
            ExternalContacts = App.ExternalContacts;
        }



        public FreshAwaitCommand OnTakePhotoTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    AttachActionSheetIsVisible = false;
                    await CoreMethods.PushPageModel<TakeVideoPageModel>(true, true, false);
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
                    ShouldShowClock = !ShouldShowClock;
                    SetToTime = false;
                    ToFrameVerticalLayout = LayoutOptions.Start;
                    if (ShouldShowClock)
                    {
                        SetFromTime = true;
                        FromFrameVerticalLayout = LayoutOptions.FillAndExpand;
                        FromTime = $"{PickedTime}";
                    }
                    else
                    {
                        SetFromTime = false;
                        FromFrameVerticalLayout = LayoutOptions.Start;
                    }
                    BridgeColumn = 0;
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
                    ShouldShowClock = !ShouldShowClock;
                    SetFromTime = false;
                    FromFrameVerticalLayout = LayoutOptions.Start;
                    if (ShouldShowClock)
                    {
                        SetToTime = true;
                        ToFrameVerticalLayout = LayoutOptions.FillAndExpand;
                        ToTime = $"{PickedTime}";
                    }

                    else
                    {
                        SetToTime = false;
                        ToFrameVerticalLayout = LayoutOptions.Start;
                    }
                    BridgeColumn = 1;
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
    }
}

