using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Domain.Entities;

namespace StudyRoomBooking.Web.Pages.Admin;

public class UserRolesModel : PageModel
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
}
