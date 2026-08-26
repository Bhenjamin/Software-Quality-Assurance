using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Application.ViewModels;

public class ReportViewModel
{
    public string Title { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public List<ReportDataViewModel> Data { get; set; } = new();
}

public class ReportDataViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public decimal Percentage { get; set; }
}
