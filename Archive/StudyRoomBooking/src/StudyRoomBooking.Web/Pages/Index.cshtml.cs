using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;
using StudyRoomBooking.Web.Services;

namespace StudyRoomBooking.Web.Pages;

/// <summary>
/// Login simulation: the assessment scope explicitly excludes production
/// authentication, so this page lets you pick which seeded user you are
/// acting as. Every downstream page uses that identity when calling the
/// real Application services, so access control still behaves correctly
/// per role.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public IndexModel(IUserRepository userRepository, ICurrentUserAccessor currentUserAccessor)
    {
        _userRepository = userRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public IReadOnlyList<User> AvailableUsers { get; private set; } = Array.Empty<User>();

    public void OnGet()
    {
        AvailableUsers = _userRepository.GetAll().OrderBy(u => u.Role).ThenBy(u => u.FullName).ToList();
    }

    public IActionResult OnGetSwitchUser()
    {
        _currentUserAccessor.ClearCurrentUser();
        return RedirectToPage("Index");
    }

    public IActionResult OnPostSelectUser(Guid userId)
    {
        _currentUserAccessor.SetCurrentUser(userId);
        return RedirectToPage("Rooms/Search");
    }
}
