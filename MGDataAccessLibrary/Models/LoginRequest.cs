using System;
namespace MGDataAccessLibrary.Models
{
    public class LoginRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string AccessToken { get; set; }
    }
}
