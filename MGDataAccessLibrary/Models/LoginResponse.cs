namespace MGDataAccessLibrary.Models
{
    public class LoginResponse
    {
        public SignedInUserInfo UserInfo { get; set; }
        public PMCInfo PMCInfo { get; set; }
        public LoggedInUserPermissions Permissions { get; set; }
    }
}