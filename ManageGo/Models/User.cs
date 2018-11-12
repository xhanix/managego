using System.Collections.Generic;

namespace ManageGo
{
    public class User
    {
        public int UserID { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }

        public string UserFullName => $"{UserFirstName} {UserLastName}".Trim();

        public string UserEmailAddress { get; set; }

        // This was done because apparently writing consist API's is difficult
        // (even after asking about a million times - and I don't have the time to ask yet again)
        public List<int> Categories { get; set; }
    }
}
