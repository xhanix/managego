using System;

namespace ManageGo
{
    public class ListEventArgs : EventArgs
    {
        public String MovieUrl { get; set; }
        public String ErrorMessage { get; set; }
        public ListEventArgs(String data, String error)
        {
            MovieUrl = data;
            ErrorMessage = error;
        }
    }

}
