using System;

namespace UserService.RabbitMQ.Requests
{
    public class CreateUserRabbitRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthCountry { get; set; }
        public DateTime DOB { get; set; }
        public bool Male { get; set; }
    }
}
