using E_Commerce.Core.Models.AuthModels;
using E_Commerce.DTOS.Auth.Requests;
using E_Commerce.DTOS.Auth.Responses;
using E_Commerce.Core.Shared.Utilties.Identity;

namespace E_Commerce.Controllers;

[Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles =Roles.Admin)]
    public class RolesController(RoleManager<IdentityRole> _roleManager,UserManager<AppUser>_userManager) : ControllerBase
    {
    
    [Authorize(Roles = "Admin")] 
        [HttpPost("assignRole")]
    //[EndpointSummary("Register a new user")]

    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.UserId))
                    return BadRequest("User ID is required");

            if (string.IsNullOrEmpty(request.RoleName))
                return BadRequest("Role name is required");

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            if (!await _roleManager.RoleExistsAsync(request.RoleName))
            {
                return BadRequest("Role does not exist");
            }

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new ApiResponse{Success=true, Message = "Role assigned successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while assigning the role");
        }
    }
 
    [HttpGet]
        public IActionResult Get()
        {
            var roles = _roleManager.Roles.ToList();
            if (roles.Count() == 0)
            {
                return NotFound();
            }
            return Ok(roles);
        }
      
    [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var role = _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }
    
    [HttpPost("upsert")]
        public async Task<IActionResult> UpsertRole([FromBody] RoleUserRequest model)//?
        {
            IdentityRole role;
            if (string.IsNullOrEmpty(model.Id))
            {
                role = new IdentityRole(model.Name);
                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
            }
            else
            {
                role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    return NotFound("Role not found");
                }

                role.Name = model.Name;
                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
            }

            return Ok(new
            {
                Id = role.Id,
                Name = role.Name
            });
        }

       /* [HttpPost("{roleId}/permissions")]
        public async Task<IActionResult> AssignPermissions(string roleId, [FromBody] List<string> permissions)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound("Role not found");
            }

            var result = await _permissionService.AssignPermissionsToRoleAsync(role.Name!, permissions);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(new
            {
                Role = role.Name,
                Permissions = permissions
            });
        }*/
    }


