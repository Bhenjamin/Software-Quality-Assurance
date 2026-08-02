using Microsoft.AspNetCore.Http;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Domain.Models;

namespace StudyRoomBooking.Web.Services;

public class CurrentUserAccessor : ICurrentUserAccessor
{
    private const string SessionKey = "CurrentUserId";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
    }

    public User? GetCurrentUser()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        var idString = session?.GetString(SessionKey);
        if (idString is null || !Guid.TryParse(idString, out var userId))
        {
            return null;
        }

        return _userRepository.GetById(userId);
    }

    public void SetCurrentUser(Guid userId)
    {
        _httpContextAccessor.HttpContext?.Session.SetString(SessionKey, userId.ToString());
    }

    public void ClearCurrentUser()
    {
        _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
    }
}
