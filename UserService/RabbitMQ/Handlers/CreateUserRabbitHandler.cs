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
    public class CreateUserRabbitHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "CreateUser";

        private readonly ILogger<CreateUserRabbitHandler> _logger;
        private readonly IUserRepository _userRepository;

        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("CreateUserRabbitMessagesRecieved", "Number of rabbit messages recieved to create user handler");
        private readonly Counter successfullyCreatedUsersCounter = Metrics.CreateCounter("successfullyCreatedUsers", "Number of successfully created users");
        private readonly Counter unsucccessfulCreatedUsersCounter = Metrics.CreateCounter("unsucccessfulCreatedUsers", "Number of unsuccessfull created users");

        public CreateUserRabbitHandler(ILogger<CreateUserRabbitHandler> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(CreateUserRabbitHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<CreateUserRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(CreateUserRabbitRequest createUserRabbitRequest)
        {
            var user = new User()
            {
                UserName = createUserRabbitRequest.Username,
                Email = createUserRabbitRequest.Email,
                FirstName = createUserRabbitRequest.FirstName,
                LastName = createUserRabbitRequest.LastName,
                BirthCountry = createUserRabbitRequest.BirthCountry,
                DOB = createUserRabbitRequest.DOB,
                Male = createUserRabbitRequest.Male
            };

            var identityResult = await _userRepository.AddAsync(user, createUserRabbitRequest.Password);

            var createUserResponse = new CreateUserRabbitResponse()
            {
                IdentityResult = identityResult
            };

            if (identityResult.Succeeded)
            {
                _logger.LogInformation($"{nameof(CreateUserRabbitHandler)}.{nameof(HandleMessageAsync)}: User created.");
                successfullyCreatedUsersCounter.Inc();
                createUserResponse.User = user;
            }
            else
            {
                _logger.LogInformation($"{nameof(CreateUserRabbitHandler)}.{nameof(HandleMessageAsync)}: User creation failed.");
                unsucccessfulCreatedUsersCounter.Inc();
                createUserResponse.User = null;
            }

            return createUserResponse;
        }
    }
}
