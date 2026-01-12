using E_Commerce.Core.Data;
using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Core.Shared.Results;

namespace E_Commerce.Application.Services.Authentication;
public class UserService(UserManager<AppUser> _userManager, StoreContext _context, IHttpContextAccessor _httpContextAccessor) : IUserService
{
    [ResponseCache(Duration = 60)]
    public IQueryable<AppUser> GetAll()
    {
        var users = _context.Users.AsQueryable();
        return users;
    }
    public async Task<IList<AppUser>> SearchUsers(string searchTerm)
    {
        var users = await _context.Users
        .Where(u =>
    EF.Functions.Like(u.Email, $"%{searchTerm}%") ||
    EF.Functions.Like(u.Full_Name, $"%{searchTerm}%") ||
    EF.Functions.Like(u.PhoneNumber, $"%{searchTerm}%") ||
    EF.Functions.Like(u.Address, $"%{searchTerm}%") ||
    EF.Functions.Like(u.Job_Title, $"%{searchTerm}%"))
.Select(u => new AppUser
{
    Id = u.Id,
    Email = u.Email,
    Full_Name = u.Full_Name,
    PhoneNumber = u.PhoneNumber,
    Address = u.Address,
    Job_Title = u.Job_Title
})
.ToListAsync();
        return users;
    }
    public Task<AppUser?> GetUserByIdAsync(string id)
    {
        var user = _userManager.FindByIdAsync(id);
        return user;
    }
    public string GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }
        return userId;
    }
    public async Task<bool> UpdateUserProfileAsync(AppUser user)
    {
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
    public async Task<Result<AppUser>> AddUserAsync(AppUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<AppUser>(new Error
            {
                Title = "User creation failed",
                StatusCode = StatusCodes.Status400BadRequest
            });
        }

        return Result.Success(user);
    }

    public async Task UpdateUser(AppUser user)
    {
        await _userManager.UpdateAsync(user);
    }
    public async Task DeleteUser(AppUser user)
    {
        await _userManager.DeleteAsync(user);
    }
}

