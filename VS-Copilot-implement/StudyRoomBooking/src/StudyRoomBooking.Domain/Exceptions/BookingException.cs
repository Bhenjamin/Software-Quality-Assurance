namespace StudyRoomBooking.Domain.Exceptions;

public class BookingException : DomainException
{
    public BookingException(string message) : base(message) { }
}
