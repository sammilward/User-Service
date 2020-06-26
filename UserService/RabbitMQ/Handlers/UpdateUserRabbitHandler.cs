using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Prometheus;
using RabbitMQHelper;
using RabbitMQHelper.Models;
using System.Globalization;
using System.Threading.Tasks;
using UserService.DataAccess;
using UserService.Geocoding;
using UserService.Models;
using UserService.RabbitMQ.Requests;
using UserService.RabbitMQ.Responses;

namespace UserService.RabbitMQ.Handlers
{
    public class UpdateUserRabbitHandler : RabbitMessageHandler
    {
        protected override string MethodCanHandle => "UpdateUser";

        private readonly ILogger<UpdateUserRabbitHandler> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IReverseGeocodeRestAPIInvoker _reverseGeocodeRestAPIInvoker;

        private readonly Counter rabbitMessagesRecievedCounter = Metrics.CreateCounter("UpdateUserRabbitMessagesRecieved", "Number of rabbit messages recieved to update user handler");
        private readonly Counter successfullyUpdateUsersRequestsCounter = Metrics.CreateCounter("successfullyUpdateUsers", "Number of successfully update users request");
        private readonly Counter unsucccessfulUpdateUsersRequestsCounter = Metrics.CreateCounter("unsucccessfulUpdateUsers", "Number of unsuccessful update users requests");

        public UpdateUserRabbitHandler(ILogger<UpdateUserRabbitHandler> logger, IUserRepository userRepository, IReverseGeocodeRestAPIInvoker reverseGeocodeRestAPIInvoker)
        {
            _logger = logger;
            _userRepository = userRepository;
            _reverseGeocodeRestAPIInvoker = reverseGeocodeRestAPIInvoker;
        }

        protected override async Task<object> ConvertMessageAndHandle(RabbitMessageRequestModel messageRequest)
        {
            rabbitMessagesRecievedCounter.Inc();
            _logger.LogInformation($"{nameof(UpdateUserRabbitHandler)}.{nameof(ConvertMessageAndHandle)}: Converting message.");

            return await HandleMessageAsync(JsonConvert.DeserializeObject<UpdateUserRabbitRequest>(messageRequest.Data.ToString()));
        }

        private async Task<object> HandleMessageAsync(UpdateUserRabbitRequest updateUserRabbitRequest)
        {
            var updateUserModel = new UpdateUserModel()
            {
                Id = updateUserRabbitRequest.Id,
                Email = updateUserRabbitRequest.Email,
                FirstName = updateUserRabbitRequest.FirstName,
                LastName = updateUserRabbitRequest.LastName,
                BirthCountry = updateUserRabbitRequest.BirthCountry,
                DOB = updateUserRabbitRequest.DOB,
                Male = updateUserRabbitRequest.Male
            };

            if (updateUserRabbitRequest.Latitude.HasValue && updateUserRabbitRequest.Longitude.HasValue)
            {
                _logger.LogInformation($"{nameof(UpdateUserRabbitHandler)}.{nameof(HandleMessageAsync)}: Attempting to update user location.");

                TextInfo textInfo = new CultureInfo("en-UK", false).TextInfo;

                _logger.LogInformation($"{nameof(UpdateUserRabbitHandler)}.{nameof(HandleMessageAsync)}: Sending API request to reverse geocode.");

                var jsonResponse = await _reverseGeocodeRestAPIInvoker.RequestAndWaitForResponseAsync(updateUserRabbitRequest.Latitude.Value, updateUserRabbitRequest.Longitude.Value);

                if (jsonResponse != null)
                {
                    _logger.LogInformation($"{nameof(UpdateUserRabbitHandler)}.{nameof(HandleMessageAsync)}: Successfull response from API. Updating user location.");

                    updateUserModel.Longitude = updateUserRabbitRequest.Longitude;
                    updateUserModel.Latitude = updateUserRabbitRequest.Latitude;

                    if (jsonResponse.ContainsKey("country")) updateUserModel.CurrentCountry = textInfo.ToTitleCase(textInfo.ToLower(jsonResponse["country"].ToString()));
                    if (jsonResponse.ContainsKey("city")) updateUserModel.CurrentCity = textInfo.ToTitleCase(textInfo.ToLower(jsonResponse["city"].ToString()));
                }
            }

            var identityResult = await _userRepository.UpdateAsync(updateUserModel);

            var updateUserRabbitResponse = new UpdateUserRabbitResponse()
            {
                IdentityResult = identityResult
            };

            if (identityResult.Succeeded)
            {
                _logger.LogInformation($"{nameof(UpdateUserRabbitHandler)}.{nameof(HandleMessageAsync)}: User updated.");
                successfullyUpdateUsersRequestsCounter.Inc();
                updateUserRabbitResponse.User = await _userRepository.GetAsync(updateUserRabbitRequest.Id);
            }
            else
            {
                _logger.LogInformation($"{nameof(UpdateUserRabbitHandler)}.{nameof(HandleMessageAsync)}: User update failed.");
                unsucccessfulUpdateUsersRequestsCounter.Inc();
                updateUserRabbitResponse.User = null;
            }

            return updateUserRabbitResponse;
        }
    }
}
