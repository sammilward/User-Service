using Microsoft.AspNetCore.Identity;
using System;

namespace UserService.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthCountry { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CurrentCountry { get; set; }
        public string CurrentCity { get; set; }
        public DateTime DOB { get; set; }
        public bool Male { get; set; }
    }
}
