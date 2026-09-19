using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Entities;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Web.Pages.Admin;

public class UserRolesModel : AdminPageModel
{
    private readonly IUserService _userService;

    public List<User> Users { get; set; } = new();

    public UserRolesModel(IUserService userService)
    {
        _userService = userService;
    }

    public async Task OnGetAsync()
    {
        Users = await _userService.GetAllUsersAsync();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, UserRole role)
    {
        if (!Enum.IsDefined(role))
        {
            ModelState.AddModelError(string.Empty, "Invalid role.");
        }
        else
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
            }
            else
            {
                user.Role = role;
                await _userService.UpdateUserAsync(user);
                return RedirectToPage();
            }
        }

        Users = await _userService.GetAllUsersAsync();
        return Page();
    }
}
