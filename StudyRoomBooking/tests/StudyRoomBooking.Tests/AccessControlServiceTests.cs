using Microsoft.VisualStudio.TestTools.UnitTesting;
using StudyRoomBooking.Domain.Enums;

namespace StudyRoomBooking.Tests;

/// <summary>
/// Starter tests for IAccessControlService. This class deliberately covers
/// only the core rule for each scenario type — restricted-room access,
/// admin privilege, and booking ownership — as a pattern to extend.
///
/// TODO (Developer 2 - User Management & Administration): add cases for
///   - AcademicStaff booking a restricted room with no programme set
///   - a room with AllowedRoles set but AllowedProgrammes empty
///   - ManagementUser role behaviour (currently untested)
/// </summary>
[TestClass]
public class AccessControlServiceTests
{
    private TestFixture _fixture = null!;

    [TestInitialize]
    public void Setup() => _fixture = new TestFixture();

    [TestMethod]
    public void CanAccessRoom_ReturnsTrue_ForUnrestrictedRoom_RegardlessOfRole()
    {
        var room = _fixture.RoomOfType(RoomType.StudyPod);
        var student = _fixture.StudentInProgramme("Graphic Design");

        Assert.IsTrue(_fixture.AccessControlService.CanAccessRoom(student, room));
    }

    [TestMethod]
    public void CanAccessRoom_ReturnsFalse_ForStudentOutsideAllowedProgramme()
    {
        var lab = _fixture.RoomOfType(RoomType.Laboratory); // allowed: Computer Science, Software Engineering
        var designStudent = _fixture.StudentInProgramme("Graphic Design");

        Assert.IsFalse(_fixture.AccessControlService.CanAccessRoom(designStudent, lab));
    }

    [TestMethod]
    public void CanAccessRoom_ReturnsTrue_ForAdministrator_OnAnyRestrictedRoom()
    {
        var lab = _fixture.RoomOfType(RoomType.Laboratory);
        var admin = _fixture.Admin();

        Assert.IsTrue(_fixture.AccessControlService.CanAccessRoom(admin, lab));
    }

    [TestMethod]
    public void CanManageBooking_ReturnsFalse_ForUnrelatedStudent()
    {
        var room = _fixture.RoomOfType(RoomType.StudyPod);
        var owner = _fixture.StudentInProgramme("Computer Science");
        var otherStudent = _fixture.StudentInProgramme("Graphic Design");

        var created = _fixture.BookingService.CreateBooking(new StudyRoomBooking.Application.DTOs.BookingRequest
        {
            RoomId = room.Id,
            UserId = owner.Id,
            StartTime = DateTime.UtcNow.AddHours(1),
            EndTime = DateTime.UtcNow.AddHours(2)
        });

        Assert.IsFalse(_fixture.AccessControlService.CanManageBooking(otherStudent, created.Booking!));
    }
}
