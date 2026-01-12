using E_Commerce.DTOS.Auth.Responses;
using E_Commerce.DTOS.User.Request;
using E_Commerce.Application.Interfaces.Authentication;
using E_Commerce.Core.Shared.Utilties.Identity;
namespace E_Commerce.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles= Roles.Admin)]
public class UserController(IMapper _mapper, StoreContext _context, UserManager<AppUser> _userManager, IUserService _userService/*, IElasticClient elasticClient*/) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userResponse=_mapper.Map<UserResponse>(user);
        return Ok(new
        {
           userResponse,
            Roles = roles
        });
    }

    [HttpPost("Add")]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] UserCreateRequest userDto)
    {
        var user = _mapper.Map<AppUser>(userDto);
        if (user == null) return BadRequest("User is Empty");
        var result = await _userService.AddUserAsync(user!, userDto.Password);
        await _userManager.AddToRoleAsync(user, Roles.User);
 
        if (!result.IsSuccess)
            return Problem(
               title: result.Error!.Title,
               detail: result.Error.Message,
               statusCode: result.Error.StatusCode
           );
        var UserResponse = _mapper.Map<UserResponse>(result.Value);
        return Ok(UserResponse);
    }

    [HttpPut("Update/{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateRequest userDto)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found");
        }
        _mapper.Map(userDto, user);
        var success = await _userService.UpdateUserProfileAsync(user);
        if (!success)
        {
            return BadRequest("Failed to update user");
        }

        var UserResponse = _mapper.Map<UserResponse>(user);
        return Ok(UserResponse);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound("User not found");
        }
        await _userService.DeleteUser(user);
        return Ok("User deleted successfully");
    }


    [HttpGet("GetAll")]
    public async Task<ActionResult<List<UserRequest>>> GetAllUsers()
    {
        var users = _userManager.Users.ToList();
        var userDtos = new List<UserRequest>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userdto = _mapper.Map<UserRequest>(user);
            userdto.Roles = roles;
            userDtos.Add(userdto);
        }
        return Ok(userDtos);
    }

    [HttpGet("Search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return BadRequest("Search term cannot be empty");
        }

        var users = await _context.Users
       .Where(u =>
           EF.Functions.Like(u.Email, $"%{searchTerm}%") ||
           EF.Functions.Like(u.Full_Name, $"%{searchTerm}%") ||
           EF.Functions.Like(u.PhoneNumber, $"%{searchTerm}%") ||
           EF.Functions.Like(u.Address, $"%{searchTerm}%") ||
           EF.Functions.Like(u.Job_Title, $"%{searchTerm}%"))
       .ToListAsync();
        return Ok(users);
    }

    [HttpGet("search-exact")]
    public async Task<IActionResult> SearchUsersExact([FromQuery] UserSearchRequest searchDto)
    {
        var query = _userService.GetAll();

        if (!string.IsNullOrEmpty(searchDto.Email))
        {
            query = query.Where(u => u.Email == searchDto.Email);
        }

        var users = await query
            .Select(u => new UserRequest
            {
                Email = u.Email ?? "",
                FullName = u.Full_Name,
                PhoneNumber = u.PhoneNumber ?? "",
                Address = u.Address,
                JobTitle = u.Job_Title
            })
            .ToListAsync();
        return Ok(users);
    }

}


