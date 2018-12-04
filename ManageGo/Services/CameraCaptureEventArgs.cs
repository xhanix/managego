using System;
namespace ManageGo.Services
{
    public class CameraCaptureEventArgs : EventArgs
    {
        public string Location { get; set; }
        public CameraCaptureEventArgs(string location)
        {
            Location = location;
        }
    }
}
