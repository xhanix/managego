using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using PropertyChanged;
using Xamarin.Forms;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public class Comments
    {
        private string name;
        readonly static string orange = "#fda639";
        readonly static string blue = "#318bfa";
        readonly static string red = "#e53935";
        readonly static string green = "#378ef7";
        [AlsoNotifyFor("SecondLineText")]
        public string Text { get; set; }

        [AlsoNotifyFor("SideLineColor", "Name", "BottomSeparatorIsVisible", "BubbleBackgroundColor")]
        public CommentTypes CommentType { get; set; }
        public bool IsCompleted { get; set; }
        public string AccessNote { get; set; }
        [AlsoNotifyFor("FirstLineText")]
        public string Name
        {
            get
            {
                return CommentType == CommentTypes.Access ? "Access Granted"
                    : CommentType == CommentTypes.WorkOrder ? "Work order"
                    : CommentType == CommentTypes.Event ? "Event" : name;
            }
            set
            {
                name = value;
            }
        }
        [AlsoNotifyFor("FirstLineText")]
        public string CommentCreateTime { get; set; }
        [AlsoNotifyFor("HasFiles", "AttachedFilesListHeight")]
        public ObservableCollection<File> Files { get; set; }
        public List<int> WorkOrders { get; set; }
        public List<int> Events { get; set; }
        public bool HasAccess { get; set; }
        public bool HasPet { get; set; }

        [JsonIgnore]
        public Thickness BubblePadding
        {
            get
            {
                return CommentType == CommentTypes.Management || CommentType == CommentTypes.Internal ||
                                                  CommentType == CommentTypes.Resident
                                   ? new Thickness(0, 2, 0, 2) : new Thickness(22, 2, 22, 2);

            }
        }

        [JsonIgnore]
        public FormattedString FirstLineText
        {
            get
            {
                var date = DateTime.Now;
                var dateString = " ";
                if (!string.IsNullOrWhiteSpace(CommentCreateTime))
                {
                    date = DateTime.Parse(CommentCreateTime);
                    dateString = date.ToString("MMM dd - h:mm tt");
                }
                return new FormattedStringBuilder()
                    .Span(Name ?? " ", "My-TitleFirstPart-Style")
                    .Span("•", "My-TitleSecondPart-Style")
                    .Span(dateString, "My-TitleSecondPart-Style")
                    .Build();
            }
        }

        [JsonIgnore]
        public string SecondLineText
        {
            get
            {
                return Text ?? string.Empty;
            }
        }

        [JsonIgnore]
        public bool BottomSeparatorIsVisible
        {
            get
            {
                return CommentType == CommentTypes.Management || CommentType == CommentTypes.Internal ||
                                                  CommentType == CommentTypes.Resident;
            }
        }


        [JsonIgnore]
        public bool InternalLabelIsVisible
        {
            get
            {
                return CommentType == CommentTypes.Internal;
            }
        }


        [JsonIgnore]
        public string BubbleBackgroundColor
        {
            get
            {
                return CommentType == CommentTypes.Management || CommentType == CommentTypes.Internal ||
                                                  CommentType == CommentTypes.Resident
                    ? "transparent" : "#fff2e0";
            }
        }

        [JsonIgnore, AlsoNotifyFor("AttachedFilesListHeight")]
        public bool HasFiles { get { return Files != null && Files.Count > 0; } }

        [JsonIgnore]
        public double AttachedFilesListHeight
        {
            get
            {
                return !HasFiles || Files is null ? 0 : Files.Count * 55;
            }
        }

        [JsonIgnore]
        public string TopSideLineColor { get; set; } = blue;
        [JsonIgnore]
        public bool IsNotTheLastComment { get; set; } = true;
        [JsonIgnore]
        public string SideLineColor
        {
            get
            {
                switch (CommentType)
                {
                    case CommentTypes.Resident:
                        return green;
                    case CommentTypes.Management:
                        return blue;
                    case CommentTypes.Internal:
                        return red;
                    case CommentTypes.WorkOrder:
                        return orange;
                    case CommentTypes.Event:
                        return orange;
                    case CommentTypes.Access:
                        return orange;
                }
                return "White";
            }
        }
    }
}
