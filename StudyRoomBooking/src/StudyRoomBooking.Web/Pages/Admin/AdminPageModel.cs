using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudyRoomBooking.Web.Pages.Admin;

public abstract class AdminPageModel : PageModel
{
    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (HttpContext.Session.GetString("CurrentUserRole") != "Admin")
        {
            context.Result = RedirectToPage("/Index");
        }

        base.OnPageHandlerExecuting(context);
    }
}
