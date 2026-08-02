using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.DTOs;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Enums;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Web.Pages.Rooms;

public class SearchModel : PageModel
{
    private readonly IRoomSearchService _roomSearchService;

    public SearchModel(IRoomSearchService roomSearchService)
    {
        _roomSearchService = roomSearchService;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime? Date { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StartTimeOfDay { get; set; } = "09:00";

    [BindProperty(SupportsGet = true)]
    public string? EndTimeOfDay { get; set; } = "10:00";

    [BindProperty(SupportsGet = true)]
    public int? MinCapacity { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Location { get; set; }

    [BindProperty(SupportsGet = true)]
    public RoomType? Type { get; set; }

    public IReadOnlyList<Room> Results { get; private set; } = Array.Empty<Room>();
    public bool HasSearched { get; private set; }
    public string? SearchError { get; private set; }
    public DateTime SearchStart { get; private set; }
    public DateTime SearchEnd { get; private set; }

    public void OnGet()
    {
        if (Date is null)
        {
            return; // First visit — show the empty form only.
        }

        HasSearched = true;

        if (!TimeSpan.TryParse(StartTimeOfDay, out var startTod) || !TimeSpan.TryParse(EndTimeOfDay, out var endTod))
        {
            SearchError = "Please provide valid start and end times.";
            return;
        }

        SearchStart = Date.Value.Date + startTod;
        SearchEnd = Date.Value.Date + endTod;

        try
        {
            Results = _roomSearchService.SearchAvailableRooms(new RoomSearchCriteria
            {
                StartTime = SearchStart,
                EndTime = SearchEnd,
                MinCapacity = MinCapacity,
                Location = Location,
                Type = Type
            }).ToList();
        }
        catch (ArgumentException ex)
        {
            SearchError = ex.Message;
        }
    }
}
