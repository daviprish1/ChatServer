using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserClassLibrary
{
    public class User
    {
        public string UserName { get; set; }
        public string Pass { get; set; }

        public User(string name, string pass)
        {
            UserName = name;
            Pass = pass;
        }
    }
}
