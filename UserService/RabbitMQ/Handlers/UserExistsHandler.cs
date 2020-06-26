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
    public class UserExistsHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "UserExists";

        private readonly ILogger<UserExistsHandler> _logger;
        private readonly IUserRepository _userRepository;

        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("UserExistsRabbitMessagesRecieved", "Number of rabbit messages recieved to User Exists handler");
        private readonly Counter userExistsCounter = Metrics.CreateCounter("UserExists", "Number of successful user exists requests");
        private readonly Counter userDoesNotExistRequestsCounter = Metrics.CreateCounter("UserDoesNotExist", "Number of user exists requests when user does not exists");

        public UserExistsHandler(ILogger<UserExistsHandler> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(UserExistsHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<UserExistsRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(UserExistsRabbitRequest userExistsRabbitRequest)
        {
            var user = await _userRepository.GetAsync(userExistsRabbitRequest.Id);

            var userExistsRabbitResponse = new UserExistsRabbitResponse();

            if (user != null)
            {
                _logger.LogInformation($"{nameof(UserExistsHandler)}.{nameof(HandleMessageAsync)}: User exists with id: {user.Id}.");
                userExistsCounter.Inc();
                userExistsRabbitResponse.Exists = true;
            }
            else
            {
                _logger.LogInformation($"{nameof(UserExistsHandler)}.{nameof(HandleMessageAsync)}: User does not exist with id: {userExistsRabbitRequest.Id}.");
                userDoesNotExistRequestsCounter.Inc();
                userExistsRabbitResponse.Exists = false;
            }

            return userExistsRabbitResponse;
        }
    }
}
