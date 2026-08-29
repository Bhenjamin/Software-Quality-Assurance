using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Models;
using StudyRoomBooking.Web.Services;

namespace StudyRoomBooking.Web.Pages.Admin;

public class RoomsModel : PageModel
{
    private readonly IRoomRepository _roomRepository;
    private readonly IAccessControlService _accessControlService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public RoomsModel(IRoomRepository roomRepository, IAccessControlService accessControlService, ICurrentUserAccessor currentUserAccessor)
    {
        _roomRepository = roomRepository;
        _accessControlService = accessControlService;
        _currentUserAccessor = currentUserAccessor;
    }

    public IReadOnlyList<Room> Rooms { get; private set; } = Array.Empty<Room>();

    [BindProperty]
    public string NewRoomName { get; set; } = string.Empty;

    [BindProperty]
    public string NewRoomLocation { get; set; } = string.Empty;

    [BindProperty]
    public int NewRoomCapacity { get; set; }

    [BindProperty]
    public RoomType NewRoomType { get; set; }

    private IActionResult? RequireAdmin(out User? admin)
    {
        admin = _currentUserAccessor.GetCurrentUser();
        if (admin is null)
        {
            return RedirectToPage("/Index");
        }

        if (!_accessControlService.CanPerformAdminAction(admin))
        {
            TempData["Error"] = "Administrator access is required to view this page.";
            return RedirectToPage("/Rooms/Search");
        }

        return null;
    }

    public IActionResult OnGet()
    {
        var redirect = RequireAdmin(out _);
        if (redirect is not null) return redirect;

        Rooms = _roomRepository.GetAll().OrderBy(r => r.Name).ToList();
        return Page();
    }

    public IActionResult OnPostAddRoom()
    {
        var redirect = RequireAdmin(out _);
        if (redirect is not null) return redirect;

        // NOTE for Developer 2: this is a minimal "basic example" per the
        // Assessment 1 GUI scope. Editing/deactivating existing rooms,
        // and setting AllowedRoles/AllowedProgrammes from the UI, are
        // good next commits — IRoomRepository.Update already supports it.
        _roomRepository.Add(new Room
        {
            Name = NewRoomName,
            Location = NewRoomLocation,
            Capacity = NewRoomCapacity,
            Type = NewRoomType
        });

        TempData["Success"] = $"Room '{NewRoomName}' added.";
        return RedirectToPage();
    }
}
