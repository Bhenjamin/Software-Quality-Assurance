namespace StudyRoomBooking.Domain.Exceptions;

public class AccessDeniedException : DomainException
{
    public AccessDeniedException(string message) : base(message) { }
}
