using System;
namespace MGDataAccessLibrary.Models
{
    public class AddNewCommentRequestItem
    {
        public int TicketID { get; set; }
        public CommentTypes CommentType { get; set; }
        public string Comment { get; set; }
        public bool IsCompleted { get; set; }
    }
}
