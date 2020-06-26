using System;

namespace UserService.Models
{
    public class UpdateUserModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthCountry { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string CurrentCountry { get; set; }
        public string CurrentCity { get; set; }
        public DateTime? DOB { get; set; }
        public bool? Male { get; set; }
    }
}
