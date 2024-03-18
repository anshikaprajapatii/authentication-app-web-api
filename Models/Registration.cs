using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationApp.Models
{
    public class Registration
    {
        public string secretKey { get; set; }
        public int userId { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public int age { get; set; }
        public string gender { get; set; }
        public string password { get; set; }
        public int isActive { get; set; }
    }
}
