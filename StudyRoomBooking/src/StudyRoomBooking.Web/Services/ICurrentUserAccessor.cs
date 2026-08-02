using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Web.Services;

/// <summary>
/// UI-only helper that simulates a logged-in user via session, since the
/// assessment scope explicitly excludes production authentication. This
/// class contains no business rules — it only resolves "who is the
/// current session pretending to be" and hands that User object to pages,
/// which then pass it into the real Application services for any
/// access-control decision.
/// </summary>
public interface ICurrentUserAccessor
{
    User? GetCurrentUser();
    void SetCurrentUser(Guid userId);
    void ClearCurrentUser();
}
