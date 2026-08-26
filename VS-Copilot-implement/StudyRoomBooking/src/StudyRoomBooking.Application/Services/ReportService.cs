using StudyRoomBooking.Infrastructure.Data;
using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.ViewModels;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.Services;

public class ReportService : IReportService
{
    private readonly DataStore _dataStore;

    public ReportService(DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public ReportViewModel GenerateOccupancyReport(DateTime startDate, DateTime endDate)
    {
        var bookings = _dataStore.Bookings
            .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate && b.Status != BookingStatus.Cancelled)
            .ToList();

        var totalRooms = _dataStore.Rooms.Count;
        var bookedRooms = bookings.Select(b => b.RoomId).Distinct().Count();

        return new ReportViewModel
        {
            Title = "Room Occupancy Report",
            GeneratedDate = DateTime.UtcNow,
            Data = new List<ReportDataViewModel>
            {
                new ReportDataViewModel
                {
                    Label = "Booked Rooms",
                    Value = bookedRooms,
                    Percentage = totalRooms > 0 ? (decimal)bookedRooms / totalRooms * 100 : 0
                },
                new ReportDataViewModel
                {
                    Label = "Available Rooms",
                    Value = totalRooms - bookedRooms,
                    Percentage = totalRooms > 0 ? (decimal)(totalRooms - bookedRooms) / totalRooms * 100 : 0
                },
                new ReportDataViewModel
                {
                    Label = "Total Bookings",
                    Value = bookings.Count,
                    Percentage = 100
                }
            }
        };
    }

    public ReportViewModel GenerateUserBookingReport(int userId)
    {
        var bookings = _dataStore.Bookings
            .Where(b => b.UserId == userId)
            .ToList();

        var confirmed = bookings.Count(b => b.Status == BookingStatus.Confirmed);
        var cancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled);
        var pending = bookings.Count(b => b.Status == BookingStatus.Pending);

        return new ReportViewModel
        {
            Title = $"User Booking Report - {userId}",
            GeneratedDate = DateTime.UtcNow,
            Data = new List<ReportDataViewModel>
            {
                new ReportDataViewModel { Label = "Confirmed", Value = confirmed, Percentage = bookings.Count > 0 ? (decimal)confirmed / bookings.Count * 100 : 0 },
                new ReportDataViewModel { Label = "Cancelled", Value = cancelled, Percentage = bookings.Count > 0 ? (decimal)cancelled / bookings.Count * 100 : 0 },
                new ReportDataViewModel { Label = "Pending", Value = pending, Percentage = bookings.Count > 0 ? (decimal)pending / bookings.Count * 100 : 0 }
            }
        };
    }

    public ReportViewModel GenerateRoomBookingReport(int roomId)
    {
        var bookings = _dataStore.Bookings
            .Where(b => b.RoomId == roomId)
            .ToList();

        var confirmed = bookings.Count(b => b.Status == BookingStatus.Confirmed);
        var cancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled);
        var pending = bookings.Count(b => b.Status == BookingStatus.Pending);

        var room = _dataStore.Rooms.FirstOrDefault(r => r.Id == roomId);

        return new ReportViewModel
        {
            Title = $"Room Booking Report - {room?.RoomName}",
            GeneratedDate = DateTime.UtcNow,
            Data = new List<ReportDataViewModel>
            {
                new ReportDataViewModel { Label = "Confirmed", Value = confirmed, Percentage = bookings.Count > 0 ? (decimal)confirmed / bookings.Count * 100 : 0 },
                new ReportDataViewModel { Label = "Cancelled", Value = cancelled, Percentage = bookings.Count > 0 ? (decimal)cancelled / bookings.Count * 100 : 0 },
                new ReportDataViewModel { Label = "Pending", Value = pending, Percentage = bookings.Count > 0 ? (decimal)pending / bookings.Count * 100 : 0 }
            }
        };
    }
}
