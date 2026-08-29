# Study Room Booking and Management System — Assessment 1 Prototype

A two-week prototype demonstrating Software Quality Assurance practice
alongside a working booking system, built with a clean, layered C# solution.

> **Scope note:** This is a foundation prototype, not a finished product. 

## Solution structure

```text

StudyRoomBooking/
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

***

## GUI framework choice

**ASP.NET Core Razor Pages** was chosen over WPF or WinForms because it runs across platforms and is the easiest way to demonstrate the system in a shared development environment.

- WPF and WinForms are Windows-only in practice, which restricts team collaboration and demoing.
- Razor Pages works across machines with the .NET SDK and a browser.
- Pages are thin adapters and do not contain business rules.
- Validation, overlap prevention, and authorization remain in the application services and are tested through the MSTest suite.
- Login is simulated with a session-based user selection, consistent with the assessment scope.

***

## Features Completed So Far

### Authentication & Roles
- Login / logout with role-based access (Student, Staff, Admin)

### Room Search & Booking (Student)
- Search available rooms by date, time, capacity, room type, and location
- Room search results respect major-based access restrictions — specialised rooms (e.g. Design Studio, Engineering Lab) only appear for students in the required major
- View room details
- Create a booking for an available room
- Booking validation: rejects overlapping/double bookings for the same room and time slot
- Booking validation: rejects bookings in the past
- Booking validation: rejects bookings more than 60 days in advance
- Modify an existing future booking to a new time/date
- Cancel a future booking
- View booking history / "My Bookings"

### Recurring Bookings (Staff)
- Create recurring bookings (daily, weekly, bi-weekly, monthly patterns)
- Recurring bookings can skip the standard advance-booking-window check

### Admin
- Room management (create/update/delete rooms)
- Access rule management (major-based room restrictions)
- Booking management / overrides (admin can override booking rules with a reason)
- User role management
- Reports: booking statistics and room utilisation by date range

### Other
- Unit test suite (MSTest + Moq) covering core booking and search/eligibility logic

***

## Running it

Requires the .NET 8 or 10 SDK.

```bash
# Run the GUI
dotnet run --project src/StudyRoomBooking.Web

# Then Open in your browser
http://localhost:5176

# Run the test suite
dotnet test
```
