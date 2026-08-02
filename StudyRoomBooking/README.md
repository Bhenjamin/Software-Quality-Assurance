# Study Room Booking and Management System — Assessment 1 Prototype

A two-week prototype demonstrating Software Quality Assurance practice
alongside a working booking system, built with a clean, layered C# solution.

> **Scope note:** this is a foundation, not a finished product. See
> [`TEAM_TASKS.md`](TEAM_TASKS.md) for what's intentionally left for each
> team member to build on.

## Solution structure

```
StudyRoomBooking.sln
src/
  StudyRoomBooking.Domain/          Room, User, Booking models + enums — no dependencies
  StudyRoomBooking.Application/     Interfaces, DTOs, and core services (search, booking, access control)
  StudyRoomBooking.Infrastructure/  In-memory repositories + sample data seeding
  StudyRoomBooking.ConsoleDemo/     Runnable console demo exercising every core workflow
  StudyRoomBooking.Web/             ASP.NET Core Razor Pages GUI (see below)
tests/
  StudyRoomBooking.Tests/           MSTest suite (example coverage — see TEAM_TASKS.md)
```

Dependencies flow one way: `ConsoleDemo`/`Web`/`Tests` → `Infrastructure` → `Application` → `Domain`.
Nothing in `Domain` or `Application` depends on `Infrastructure` or `Web`, so
swapping the in-memory repositories for a real database later (EF Core, etc.)
needs no changes to the business logic, pages, or tests — this is the
"modular design" the maintainability requirement calls for.

## GUI framework choice

**ASP.NET Core Razor Pages**, over WPF/WinForms. Reasoning:

- WPF and WinForms only run (and can only be designed) on Windows — this
  team develops on a mix of platforms including Linux, so a desktop-only
  framework would block some members from running or demoing it.
- Razor Pages runs anywhere the .NET SDK does, and the app itself just
  needs a browser to view — easiest option for a shared demo.
- Pages contain **no business logic**. Every page's code-behind
  (`*.cshtml.cs`) is a thin adapter that builds a request DTO, calls an
  existing `Application` service (`IRoomSearchService`, `IBookingService`,
  `IAccessControlService`), and renders the result. All the rules that
  matter for grading (validation, double-booking prevention, access
  control) still live in `Application` and are still covered by the same
  MSTest suite, untouched by the UI existing.
- "Login" is simulated via a session-stored user selection
  (`Pages/Index.cshtml` + `Web/Services/CurrentUserAccessor`) rather than
  real authentication, per the assessment's explicit scope exclusion.

### Screens implemented

**Student workflow:** user selection (login simulation) → room search with
filters → availability results → booking creation form → confirmation →
booking history → modify → cancel.

**Administrator workflow (basic examples, per GUI scope):** room list +
add-room form, all-bookings overview, read-only user/role list.

Reporting dashboards and room editing beyond "add" are intentionally left
as extensions — see `TEAM_TASKS.md`.

## Running the GUI

```bash
dotnet run --project src/StudyRoomBooking.Web
```

Then open the URL it prints (defaults to `http://localhost:5080`).

## What's implemented (Assessment 1 priority list)

1. Clean C# solution architecture ✅
2. Core domain models (`Room`, `User`, `Booking`) ✅
3. Room searching (`RoomSearchService`, filters by date/time/capacity/location/type) ✅
4. Booking creation (`BookingService.CreateBooking`) ✅
5. Booking validation (time sanity, user/room existence) ✅
6. Double-booking prevention (overlap check per room) ✅
7. User roles (`Student`, `AcademicStaff`, `Administrator`, `ManagementUser`) ✅
8. Basic access control (`AccessControlService`, restricted rooms, admin actions) ✅
9. Booking modification (`BookingService.ModifyBooking`) ✅
10. Booking cancellation (`BookingService.CancelBooking`) ✅
11. MSTest test suite (starter coverage per area, see `TEAM_TASKS.md`) ✅

Explicitly out of scope for this phase (interfaces/placeholders only, per
the assessment brief): advanced reporting, production authentication, real
email integration, recurring bookings, full multilingual support.

## Running it

Requires the .NET 8 SDK.

```bash
# Run the GUI (this is the main way to demonstrate the prototype)
dotnet run --project src/StudyRoomBooking.Web

# Run the console demo (still useful for a fast, scripted walkthrough of the same logic)
dotnet run --project src/StudyRoomBooking.ConsoleDemo

# Run the test suite
dotnet test
```

## Requirements traceability (examples)

| Requirement (testable form) | Implemented in | Covered by |
|---|---|---|
| Search results return within 5s for ≥100 reservations | `RoomSearchService.SearchAvailableRooms` | `RoomSearchServiceTests` (perf test still TODO — see `TEAM_TASKS.md`) |
| System rejects overlapping reservations | `BookingService.CreateBooking` / `HasConflict` | `BookingLifecycleTests.CreateBooking_Fails_WhenTimeOverlapsExistingConfirmedBooking` |
| Only authorised users access restricted rooms/admin functions | `AccessControlService` | `AccessControlServiceTests` |

A full traceability matrix mapping every requirement in the project brief is
a Developer 3 task (see `TEAM_TASKS.md`).
