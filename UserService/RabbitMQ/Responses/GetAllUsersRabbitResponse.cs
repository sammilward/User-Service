using System.Collections.Generic;
using UserService.Models;

namespace UserService.RabbitMQ.Responses
{
    public class GetAllUsersRabbitResponse
    {
        public bool FoundUsers { get; set; }
        public List<User> Users { get; set; }
    }
}