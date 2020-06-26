using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prometheus;
using RabbitMQHelper;
using RabbitMQHelper.Models;
using System;
using System.Threading.Tasks;
using UserService.DataAccess;
using UserService.RabbitMQ.Requests;
using UserService.RabbitMQ.Responses;

namespace UserService.RabbitMQ.Handlers
{
    public class DeleteUserRabbitHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "DeleteUser";

        private readonly ILogger<DeleteUserRabbitHandler> _logger;
        private readonly IUserRepository _userRepository;

        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("DeleteUserRabbitMessagesRecieved", "Number of rabbit messages recieved to delete user handler");
        private readonly Counter successfullyDeletedUsersCounter = Metrics.CreateCounter("successfullyDeletedUsers", "Number of successfully deleted users");
        private readonly Counter unsucccessfulDeletedUsersCounter = Metrics.CreateCounter("unsucccessfulDeletedUsers", "Number of unsuccessfull deleted users");

        public DeleteUserRabbitHandler(ILogger<DeleteUserRabbitHandler> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(DeleteUserRabbitHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<DeleteUserRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(DeleteUserRabbitRequest deleteUserRabbitRequest)
        {
            var deleteUserRabbitResponse = new DeleteUserRabbitResponse();

            try
            {
                var identityResult = await _userRepository.DeleteAsync(deleteUserRabbitRequest.Id);

                if (identityResult.Succeeded)
                {
                    _logger.LogInformation($"{nameof(DeleteUserRabbitHandler)}.{nameof(HandleMessageAsync)}: User deleted with id: {deleteUserRabbitRequest.Id}.");
                    successfullyDeletedUsersCounter.Inc();
                }
                else 
                {
                    _logger.LogInformation($"{nameof(DeleteUserRabbitHandler)}.{nameof(HandleMessageAsync)}: User not removed with id {deleteUserRabbitRequest.Id}.");
                    unsucccessfulDeletedUsersCounter.Inc();
                }

                deleteUserRabbitResponse.Successful = identityResult.Succeeded;
            }
            catch
            {
                _logger.LogInformation($"{nameof(DeleteUserRabbitHandler)}.{nameof(HandleMessageAsync)}: User doesn't exist with id: {deleteUserRabbitRequest.Id}.");
                unsucccessfulDeletedUsersCounter.Inc();
                deleteUserRabbitResponse.Successful = false;
            }

            return deleteUserRabbitResponse;
        }
    }
}
