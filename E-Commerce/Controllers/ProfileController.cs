using E_Commerce.DTOS.Auth.Responses;
using E_Commerce.DTOS.Profile;
using E_Commerce.Application.Interfaces.Common;
using E_Commerce.Core.Shared.Utilties.Identity;
namespace E_Commerce.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = $"{Roles.User},{Roles.Admin}")]

public class ProfileController(IPhotoService _photoService, UserManager<AppUser> _userManager, IMapper _mapper,SignInManager<AppUser>_signInManager) : ControllerBase
{
    [HttpPost("upload-photo")]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        try
        {
            var photoPath = await _photoService.UploadImageAsync(file);
            var user = await _userManager.GetUserAsync(User);
            user!.PhotoPath = photoPath;
            await _userManager.UpdateAsync(user);
            return Ok(new { PhotoPath = photoPath });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("download-user-image/{userId}")]
    public async Task<IActionResult> DownloadUserImage(string userId)
    {
        try
        {
            var fileResult = await _photoService.DownloadUserImageAsync(userId);
            return Ok(fileResult);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while downloading the photo");
        }
    }

    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileRequest updateDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Email = updateDto.Email ?? user.Email;
            user.PhoneNumber = updateDto.PhoneNumber ?? user.PhoneNumber;
            user.Address = updateDto.Address ?? user.Address;
            user.Full_Name = updateDto.FullName ?? user.Full_Name;
            user.Job_Title = updateDto.JobTitle ?? user.Job_Title;
            var emailChanged = !string.Equals(user.Email, updateDto.Email, StringComparison.OrdinalIgnoreCase);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (emailChanged)
            {
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Profile updated. Please log in again with your new email."
                });
            }
            else
            {
                await _signInManager.RefreshSignInAsync(user);
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Profile updated successfully"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "An error occurred while updating profile"
            });
        }
    }

}



