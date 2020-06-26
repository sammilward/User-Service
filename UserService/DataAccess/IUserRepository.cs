using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Models;

namespace UserService.DataAccess
{
    public interface IUserRepository
    {
        Task<User> GetAsync(string id);
        Task<List<User>> GetAllAsync(string currentUserid, GetUsersFilters getUsersFilters);
        List<User> GetSelectedAsync(List<string> userIds);
        Task<IdentityResult> AddAsync(User user, string password);
        Task<IdentityResult> UpdateAsync(UpdateUserModel updateUserModel);
        Task<IdentityResult> DeleteAsync(string id);
    }
}
