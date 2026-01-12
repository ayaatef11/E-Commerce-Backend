using E_Commerce.Core.Models.AuthModels;
using E_Commerce.Core.Shared.Results;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce.Application.Interfaces.Authentication
{
    public interface IUserService
    {
        IQueryable<AppUser> GetAll();
        Task<AppUser?> GetUserByIdAsync(string id);
        string GetCurrentUserId();
        Task<bool> UpdateUserProfileAsync(AppUser user);
        Task<Result<AppUser>> AddUserAsync(AppUser user, string password);
        Task UpdateUser(AppUser user);
        Task DeleteUser(AppUser user);
        Task<IList<AppUser>> SearchUsers(string searchTerm);

    }
}
