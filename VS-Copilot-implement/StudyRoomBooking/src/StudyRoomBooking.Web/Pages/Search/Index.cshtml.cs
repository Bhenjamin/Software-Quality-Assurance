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

        HasSearched = true;
        SearchResults = _searchService.SearchRooms(SearchCriteria);
        return Page();
    }

    private bool IsAuthenticated()
    {
        var userId = HttpContext.Session.GetString(AppConstants.UserSessionKey);
        return !string.IsNullOrEmpty(userId);
    }
}
