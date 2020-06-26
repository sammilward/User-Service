using Microsoft.AspNetCore.Identity;
using UserService.Models;

namespace UserService.RabbitMQ.Responses
{
    public class CreateUserRabbitResponse
    {
        public IdentityResult IdentityResult { get; set; }
        public User User { get; set; }
    }
}
