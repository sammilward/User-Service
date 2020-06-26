using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prometheus;
using RabbitMQHelper;
using RabbitMQHelper.Models;
using System.Threading.Tasks;
using UserService.DataAccess;
using UserService.Models;
using UserService.RabbitMQ.Requests;
using UserService.RabbitMQ.Responses;

namespace UserService.RabbitMQ.Handlers
{
    public class GetAllUsersRabbitHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "GetAllUsers";

        private readonly ILogger<GetAllUsersRabbitHandler> _logger;
        private readonly IUserRepository _userRepository;

        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("GetAllUserRabbitMessagesRecieved", "Number of rabbit messages recieved to GetAll user handler");
        private readonly Counter successfullyGetAllUsersRequestsCounter = Metrics.CreateCounter("successfullyGetAllUsers", "Number of successfully GetAll users request");

        public GetAllUsersRabbitHandler(ILogger<GetAllUsersRabbitHandler> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(GetAllUsersRabbitHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<GetAllUsersRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(GetAllUsersRabbitRequest getAllUsersRabbitRequest)
        {
            var getUsersFilters = new GetUsersFilters()
            {
                LocationScope = getAllUsersRabbitRequest.LocationScope,
                Travellers = getAllUsersRabbitRequest.Travellers,
                MaxAge = getAllUsersRabbitRequest.MaxAge,
                MinAge = getAllUsersRabbitRequest.MinAge,
                GenderOption = getAllUsersRabbitRequest.GenderOption
            };

            var users = await _userRepository.GetAllAsync(getAllUsersRabbitRequest.Id, getUsersFilters);

            var getAllUsersRabbitResponse = new GetAllUsersRabbitResponse();

            if (users.Count > 0)
            {
                getAllUsersRabbitResponse.FoundUsers = true;
                getAllUsersRabbitResponse.Users = users;
            }
            else
            {
                getAllUsersRabbitResponse.FoundUsers = false;
                getAllUsersRabbitResponse.Users = null;
            }

            successfullyGetAllUsersRequestsCounter.Inc();

            return getAllUsersRabbitResponse;
        }
    }
}
