using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Models;

namespace UserService.DataAccess
{
    public class UserRepository : IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _applicationDbContext;

        public UserRepository(UserManager<User> userManager, ApplicationDbContext applicationDbContext, ILogger<UserRepository> logger)
        {
            _logger = logger;
            _userManager = userManager;
            _applicationDbContext = applicationDbContext;
        }

        public async Task<User> GetAsync(string id)
        {
            _logger.LogInformation($"{nameof(UserRepository)}.{nameof(GetAsync)}: Getting user with id: {id}.");
            var user = await _userManager.FindByIdAsync(id);
            return user;
        }

        public async Task<List<User>> GetAllAsync(string currentUserid, GetUsersFilters getUsersFilters)
        {
            var currentUser = await GetAsync(currentUserid);

            IQueryable<User> users = null;

            if (getUsersFilters.LocationScope == "Country")
            {
                users = _applicationDbContext.ApplicationUsers.Where(x => x.CurrentCountry == currentUser.CurrentCountry);
            }
            else
            {
                users = _applicationDbContext.ApplicationUsers.Where(x => x.CurrentCity == currentUser.CurrentCity);
            }

            if (getUsersFilters.Travellers)
            {
                users = users.Where(x => x.BirthCountry != currentUser.CurrentCountry);
            }
            else
            {
                users = users.Where(x => x.BirthCountry == currentUser.CurrentCountry);
            }

            if (getUsersFilters.GenderOption == 0) users = users.Where(x => x.Male == false);
            else if (getUsersFilters.GenderOption == 1) users = users.Where(x => x.Male == true);

            users = users.Where(x => x.DOB.Date <= GenerateDateForMinAge(getUsersFilters.MinAge));
            users = users.Where(x => x.DOB.Date >= GenerateDateForMaxAge(getUsersFilters.MaxAge));

            return users.Where(x => x.Id != currentUser.Id).ToList();
        }

        private DateTime GenerateDateForMinAge(int age)
        {
            var current = DateTime.Now;
            return current.AddYears(-age);
        }

        private DateTime GenerateDateForMaxAge(int age)
        {
            var current = DateTime.Now;
            return current.AddYears(-(age));
        }

        public async Task<IdentityResult> AddAsync(User user, string password)
        {
            _logger.LogInformation($"{nameof(UserRepository)}.{nameof(AddAsync)}: Adding new user: {user.Id}");
            var identityResult = await _userManager.CreateAsync(user, password);
            return identityResult;
        }

        public async Task<IdentityResult> UpdateAsync(UpdateUserModel updateUserModel)
        {
            _logger.LogInformation($"{nameof(UserRepository)}.{nameof(UpdateAsync)}: Updating user: {updateUserModel.Id}");
            var user = await GetAsync(updateUserModel.Id);
            if (!string.IsNullOrEmpty(updateUserModel.Email)) user.Email = updateUserModel.Email;
            if (!string.IsNullOrEmpty(updateUserModel.FirstName)) user.FirstName = updateUserModel.FirstName;
            if (!string.IsNullOrEmpty(updateUserModel.LastName)) user.LastName = updateUserModel.LastName;
            if (!string.IsNullOrEmpty(updateUserModel.BirthCountry)) user.BirthCountry = updateUserModel.BirthCountry;
            if (updateUserModel.Latitude.HasValue && updateUserModel.Longitude.HasValue)
            {
                user.Latitude = updateUserModel.Latitude.Value;
                user.Longitude = updateUserModel.Longitude.Value;
            }
            if (!string.IsNullOrEmpty(updateUserModel.CurrentCountry)) user.CurrentCountry = updateUserModel.CurrentCountry;
            if (!string.IsNullOrEmpty(updateUserModel.CurrentCity)) user.CurrentCity = updateUserModel.CurrentCity;
            if (updateUserModel.DOB.HasValue) user.DOB = updateUserModel.DOB.Value;
            if (updateUserModel.Male.HasValue) user.Male = updateUserModel.Male.Value;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteAsync(string id)
        {
            var user = await GetAsync(id);
            if (user != null) _logger.LogInformation($"{nameof(UserRepository)}.{nameof(DeleteAsync)}: Deleting user: {id}");
            return await _userManager.DeleteAsync(user);
        }

        public List<User> GetSelectedAsync(List<string> userIds)
        {
            return _applicationDbContext.ApplicationUsers.Where(x => userIds.Contains(x.Id)).ToList();
        }
    }
}
