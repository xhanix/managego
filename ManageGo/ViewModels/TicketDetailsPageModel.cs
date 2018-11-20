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
        public override void Init(object initData)
        {
            base.Init(initData);
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

        public FreshAwaitCommand OnReplyLabelTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {

                    tcs?.SetResult(true);
                });
            }
        }


    }
}

