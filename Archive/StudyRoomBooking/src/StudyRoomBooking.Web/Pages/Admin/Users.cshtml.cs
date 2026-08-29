using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;
using StudyRoomBooking.Web.Services;

namespace StudyRoomBooking.Web.Pages.Admin;

public class UsersModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly IAccessControlService _accessControlService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public UsersModel(IUserRepository userRepository, IAccessControlService accessControlService, ICurrentUserAccessor currentUserAccessor)
    {
        _userRepository = userRepository;
        _accessControlService = accessControlService;
        _currentUserAccessor = currentUserAccessor;
    }

    public IReadOnlyList<User> Users { get; private set; } = Array.Empty<User>();

    public IActionResult OnGet()
    {
        var admin = _currentUserAccessor.GetCurrentUser();
        if (admin is null)
        {
            return RedirectToPage("/Index");
        }

        if (!_accessControlService.CanPerformAdminAction(admin))
        {
            TempData["Error"] = "Administrator access is required to view this page.";
            return RedirectToPage("/Rooms/Search");
        }

        // Read-only "user role visibility" per the Assessment 1 GUI scope.
        // Editing roles/permissions from the UI is a Developer 2 extension.
        Users = _userRepository.GetAll().OrderBy(u => u.Role).ThenBy(u => u.FullName).ToList();
        return Page();
    }
}
