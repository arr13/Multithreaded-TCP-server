using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    // class that contains information for every user
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool CanUserEncrypt { get; set; }
        public bool CanUserDecrypt { get; set; }

        public User()
        {

        }
        public User(string userName, string password, bool canUserEncrypt, bool canUserDecrypt)
        {
            this.UserName = userName;
            this.Password = password;
            this.CanUserDecrypt = canUserDecrypt;
            this.CanUserEncrypt = canUserEncrypt;
        }
    }
}
