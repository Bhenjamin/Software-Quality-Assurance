using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;

namespace StudyRoomBooking.Web.Pages.Admin;

public class UsersModel : PageModel
{
    private readonly IAdminService _adminService;

    public List<UserViewModel> Users { get; set; } = new();

    public UsersModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public IActionResult OnGet()
    {
        if (!IsAdmin())
            return RedirectToPage("/Index");

        Users = _adminService.GetAllUsers();
        return Page();
    }

    public IActionResult OnPostDisable(int userId)
    {
        if (!IsAdmin())
            return RedirectToPage("/Index");

        _adminService.DisableUser(userId);
        return RedirectToPage();
    }

    public IActionResult OnPostEnable(int userId)
    {
        if (!IsAdmin())
            return RedirectToPage("/Index");

        _adminService.EnableUser(userId);
        return RedirectToPage();
    }

    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return role == "Admin";
    }
}
