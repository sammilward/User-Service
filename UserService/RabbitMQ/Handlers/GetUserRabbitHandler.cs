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
    public class GetUserRabbitHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "GetUser";

        private readonly ILogger<GetUserRabbitHandler> _logger;
        private readonly IUserRepository _userRepository;

        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("GetUserRabbitMessagesRecieved", "Number of rabbit messages recieved to Get user handler");
        private readonly Counter successfullyGetUsersRequestsCounter = Metrics.CreateCounter("successfullyGetUsers", "Number of successfully Get users request");
        private readonly Counter unsucccessfulGetUsersRequestsCounter = Metrics.CreateCounter("unsucccessfulGetUsers", "Number of unsuccessfull Get users requests");

        public GetUserRabbitHandler(ILogger<GetUserRabbitHandler> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(GetUserRabbitHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<GetUserRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(GetUserRabbitRequest getUserRabbitRequest)
        {
            var user = await _userRepository.GetAsync(getUserRabbitRequest.Id);

            var getUserRabbitResponse = new GetUserRabbitResponse();

            if (user != null)
            {
                _logger.LogInformation($"{nameof(CreateUserRabbitHandler)}.{nameof(HandleMessageAsync)}: User retrieved with id: {user.Id}.");
                successfullyGetUsersRequestsCounter.Inc();
                getUserRabbitResponse.User = user;
            }
            else
            {
                _logger.LogInformation($"{nameof(CreateUserRabbitHandler)}.{nameof(HandleMessageAsync)}: User not found with id: {getUserRabbitRequest.Id}.");
                unsucccessfulGetUsersRequestsCounter.Inc();
                getUserRabbitResponse.FoundUser = false;
            }

            return getUserRabbitResponse;
        }
    }
}
