using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Infrastructure.Shared.Constants;

namespace StudyRoomBooking.Web.Pages.Search;

public class IndexModel : PageModel
{
    private readonly ISearchService _searchService;

    [BindProperty]
    public SearchViewModel SearchCriteria { get; set; } = new();

    public List<RoomViewModel> SearchResults { get; set; } = new();
    public bool HasSearched { get; set; } = false;
    public string? ErrorMessage { get; set; }

    public IndexModel(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public void OnGet()
    {
        if (!IsAuthenticated())
            RedirectToPage("/Auth/Login");
    }

    public IActionResult OnPost()
    {
        if (!IsAuthenticated())
            return RedirectToPage("/Auth/Login");

        // Validate search date and time
        if (!ValidateSearchDateAndTime())
            return Page();

        HasSearched = true;
        SearchResults = _searchService.SearchRooms(SearchCriteria);
        return Page();
    }

    private bool ValidateSearchDateAndTime()
    {
        // Check if date is in the past
        if (SearchCriteria.SearchDate < DateTime.Today)
        {
            ErrorMessage = "Cannot search for past dates. Please select today or a future date.";
            return false;
        }

        // Check if date is within 60 days
        if (SearchCriteria.SearchDate > DateTime.Today.AddDays(60))
        {
            ErrorMessage = "Cannot search more than 60 days in advance.";
            return false;
        }

        // If times are provided, validate them
        if (SearchCriteria.StartTime.HasValue && SearchCriteria.EndTime.HasValue)
        {
            // Check if start equals end
            if (SearchCriteria.StartTime == SearchCriteria.EndTime)
            {
                ErrorMessage = "Start time and end time cannot be the same.";
                return false;
            }

            // Check if start is before end
            if (SearchCriteria.StartTime > SearchCriteria.EndTime)
            {
                ErrorMessage = "End time must be after start time.";
                return false;
            }

            // For today's date, check if times are in the future
            if (SearchCriteria.SearchDate == DateTime.Today)
            {
                var searchStartDateTime = DateTime.Today.Add(SearchCriteria.StartTime.Value);
                if (searchStartDateTime < DateTime.Now)
                {
                    ErrorMessage = "Cannot search for past times. Please select a future time.";
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsAuthenticated()
    {
        var userId = HttpContext.Session.GetString(AppConstants.UserSessionKey);
        return !string.IsNullOrEmpty(userId);
    }
}
