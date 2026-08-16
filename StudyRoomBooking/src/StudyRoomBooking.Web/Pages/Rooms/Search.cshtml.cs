using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Web.Pages.Rooms;

/// <summary>
/// Browse rooms by the filters that matter up front (capacity, location,
/// type) without needing an exact date/time — availability is then
/// checked per-day on the room's calendar page.
/// </summary>
public class SearchModel : PageModel
{
    private readonly IRoomSearchService _roomSearchService;

    public SearchModel(IRoomSearchService roomSearchService)
    {
        _roomSearchService = roomSearchService;
    }

    [BindProperty(SupportsGet = true)]
    public int? MinCapacity { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Location { get; set; }

    [BindProperty(SupportsGet = true)]
    public RoomType? Type { get; set; }

    public IReadOnlyList<Room> Results { get; private set; } = Array.Empty<Room>();

    public void OnGet()
    {
        Results = _roomSearchService.SearchAvailableRooms(new RoomSearchCriteria
        {
            MinCapacity = MinCapacity,
            Location = Location,
            Type = Type
        }).ToList();
    }
}
