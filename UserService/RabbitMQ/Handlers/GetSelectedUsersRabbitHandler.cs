using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prometheus;
using RabbitMQHelper;
using RabbitMQHelper.Models;
using System.Threading.Tasks;
using UserService.DataAccess;
using UserService.RabbitMQ.Requests;
using UserService.RabbitMQ.Responses;

namespace UserService.RabbitMQ.Handlers
{
    public class GetSelectedUsersRabbitHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "GetSelectedUsers";

        private readonly ILogger<GetSelectedUsersRabbitHandler> _logger;
        private readonly IUserRepository _userRepository;

        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("GetSelectedUsersRabbitMessagesRecieved", "Number of rabbit messages recieved to GetSelectedUsers handler");
        //private readonly Counter successfullyGetAllUsersRequestsCounter = Metrics.CreateCounter("successfullyGetAllUsers", "Number of successfully GetAll users request");

        public GetSelectedUsersRabbitHandler(ILogger<GetSelectedUsersRabbitHandler> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(GetSelectedUsersRabbitHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<GetSelectedUsersRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(GetSelectedUsersRabbitRequest getSelectedUsersRabbitRequest)
        {
            var users = _userRepository.GetSelectedAsync(getSelectedUsersRabbitRequest.UserIds);

            var getSelectedUsersRabbitResponse = new GetSelectedUsersRabbitResponse();

            if (users.Count > 0)
            {
                getSelectedUsersRabbitResponse.FoundUsers = true;
                getSelectedUsersRabbitResponse.Users = users;
            }
            else
            {
                getSelectedUsersRabbitResponse.FoundUsers = false;
                getSelectedUsersRabbitResponse.Users = null;
            }

            //successfullyGetAllUsersRequestsCounter.Inc();

            return getSelectedUsersRabbitResponse;
        }
    }
}
