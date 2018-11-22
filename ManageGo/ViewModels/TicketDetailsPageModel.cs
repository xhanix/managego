using System;
using Xamarin.Forms;
using FreshMvvm;
using PropertyChanged;
using System.Collections.Generic;

namespace ManageGo
{
    public class TicketDetailsPageModel : FreshBasePageModel
    {

        public TicketDetails TicketDetails { get; private set; }
        public List<Comments> Comments { get; private set; }
        public string TicketTitle { get; set; }
        public string BuildingUnitText { get; private set; }
        public View PopContentView { get; private set; }
        public bool ListIsEnabled { get; private set; } = true;
        public string TicketAddress { get; private set; }
        public string TicketTitleText { get; private set; }
        public bool HasPet { get; private set; }
        public bool HasAccess { get; private set; }
        public bool HasWorkOrder { get; private set; }
        public bool HasEvent { get; private set; }
        public bool ReplyButtonIsVisible { get; private set; } = true;
        public bool ReplyBoxIsVisible { get; private set; }
        [AlsoNotifyFor("InternalButtonImage", "SendButtonColor", "SendButtonText")]
        public bool ReplyIsInternal { get; private set; }
        public List<File> ReplyAttachments { get; set; }
        public string UploadButtonImage { get; set; } = "upload_blue.png";
        public string ReplyTextBody { get; set; }
        public string InternalButtonImage
        {
            get
            {
                return ReplyIsInternal ? "internal_active.png" : "internal_inactive";
            }
        }
        public string SendButtonColor
        {
            get
            {
                return ReplyIsInternal ? "Red" : "#318bfa";
            }
        }
        public string SendButtonText
        {
            get
            {
                return ReplyIsInternal ? "Reply to staff only" : "Reply to all";
            }
        }
        public bool AttachActionSheetIsVisible { get; private set; }
        public override void Init(object initData)
        {
            base.Init(initData);
            ReplyAttachments = new List<File>();
#if DEBUG
            ReplyAttachments.Add(new File { Name = "File 1.jpg", ID = 0 });
            ReplyAttachments.Add(new File { Name = "File 2.jpg", ID = 1 });
#endif
            var data = initData as Dictionary<string, object>;
            if (data.TryGetValue("TicketDetails", out object ticketDetails))
            {
                TicketDetails = (TicketDetails)ticketDetails;
                Comments = TicketDetails.Comments;
            }
            if (data.TryGetValue("Ticket", out object _ticket))
            {
                var ticket = (MaintenanceTicket)_ticket;
                HasPet = ticket.HasPet;
                HasAccess = ticket.HasAccess;
                HasWorkOrder = ticket.HasWorkorder;
                HasEvent = ticket.HasEvent;
            }

            if (data.TryGetValue("TicketNumber", out object ticketNumber))
                TicketTitle = $"Ticket #{(string)ticketNumber}";
            if (data.TryGetValue("Address", out object address))
                TicketAddress = (string)address;

            if (data.TryGetValue("TicketTitleText", out object subject))
                TicketTitleText = (string)subject;
        }

        public FreshAwaitCommand OnAttachButtonTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    AttachActionSheetIsVisible = !AttachActionSheetIsVisible;
                    if (AttachActionSheetIsVisible)
                        UploadButtonImage = "upload_white.png";
                    else
                        UploadButtonImage = "upload_blue.png";
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
                    await CoreMethods.PopPageModel(false, false);
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
                    tcs?.SetResult(true);
                });
            }
        }


        public FreshAwaitCommand OnUploadPhotoTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    var result = await Services.PhotoHelper.AddNewPhoto(true);
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




    }
}

