# Study Room Booking and Management System — Assessment 1 Prototype

A two-week prototype demonstrating Software Quality Assurance practice
alongside a working booking system, built with a clean, layered C# solution.

> **Scope note:** This is a foundation prototype, not a finished product. See
> [`TEAM_TASKS.md`](TEAM_TASKS.md) for the planned extensions and responsibilities.

## Solution structure

```text
VS-Copilot-implement/
└── StudyRoomBooking/
    ├── StudyRoomBooking.slnx
    ├── src/
    │   ├── StudyRoomBooking.Domain/          Room, User, Booking models + enums — no dependencies
    │   ├── StudyRoomBooking.Application/     Interfaces, DTOs, and core services
    │   ├── StudyRoomBooking.Infrastructure/  In-memory repositories + seed data
    │   ├── StudyRoomBooking.Web/             ASP.NET Core Razor Pages GUI
    │   └── StudyRoomBooking.ConsoleDemo/     Console walkthrough of the application logic
    └── tests/
        └── StudyRoomBooking.Tests/           MSTest suite for validation and regression checks
```

Dependencies flow one way: `Web` / `ConsoleDemo` / `Tests` → `Infrastructure` → `Application` → `Domain`.
Nothing in `Domain` or `Application` depends on `Infrastructure` or `Web`, so a future swap to a real database or API layer can be done without disturbing the business logic or tests.

## GUI framework choice

**ASP.NET Core Razor Pages** was chosen over WPF or WinForms because it runs across platforms and is the easiest way to demonstrate the system in a shared development environment.

- WPF and WinForms are Windows-only in practice, which restricts team collaboration and demoing.
- Razor Pages works across machines with the .NET SDK and a browser.
- Pages are thin adapters and do not contain business rules.
- Validation, overlap prevention, and authorization remain in the application services and are tested through the MSTest suite.
- Login is simulated with a session-based user selection, consistent with the assessment scope.

### Screens implemented

**Student workflow:** user selection → room search with filters → availability results → booking form → confirmation → booking history → modify → cancel.

**Administrator workflow:** room list → add-room form → all-bookings overview → user/role overview.

Reporting dashboards and more advanced room management are intentionally left for future extension.

## Running the GUI

```bash
dotnet run --project src/StudyRoomBooking.Web
```

Then open the URL printed by the app, usually `http://localhost:5176`.

## What's implemented (Assessment 1 priority list)

1. Clean C# solution architecture ✅
2. Core domain models (`Room`, `User`, `Booking`) ✅
3. Room searching with filters for date, time, capacity, location, and room type ✅
4. Booking creation via `BookingService.CreateBooking` ✅
5. Booking validation (time sanity, user/room existence) ✅
6. Double-booking prevention through overlap checks per room ✅
7. User roles (`Student`, `AcademicStaff`, `Administrator`, `ManagementUser`) ✅
8. Basic access control (`AccessControlService`) ✅
9. Booking modification and cancellation ✅
10. MSTest suite covering core workflows ✅

Out of scope for this phase: advanced reporting, real authentication, email integration, recurring bookings, and broader multilingual support.

## Running it

Requires the .NET 8 SDK.

```bash
# Run the GUI
dotnet run --project src/StudyRoomBooking.Web

# Run the console demo
dotnet run --project src/StudyRoomBooking.ConsoleDemo

# Run the test suite
dotnet test
```

## Requirements traceability (examples)

| Requirement (testable form) | Implemented in | Covered by |
|---|---|---|
| Search results return within 5s for ≥100 reservations | `RoomSearchService.SearchAvailableRooms` | `RoomSearchServiceTests` |
| System rejects overlapping reservations | `BookingService.CreateBooking` / `HasConflict` | `BookingLifecycleTests` |
| Only authorised users access restricted rooms/admin functions | `AccessControlService` | `AccessControlServiceTests` |

A full traceability matrix mapping every requirement in the project brief is a future developer task.
