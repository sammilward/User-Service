using System.Collections.Generic;

namespace UserService.RabbitMQ.Requests
{
    public class GetSelectedUsersRabbitRequest
    {
        public List<string> UserIds { get; set; }
    }
}
