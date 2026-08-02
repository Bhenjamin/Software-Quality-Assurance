# Team Task Breakdown

This document exists because the project is built by a three-person team, and
the AI-generated foundation must leave real, demonstrable work for each
member — not hand back a finished system.

## What the AI foundation includes

- Domain models: `Room`, `User`, `Booking` + enums (`RoomType`, `UserRole`, `BookingStatus`)
- Repository interfaces + in-memory implementations (swappable for a real DB later)
- Core services: `RoomSearchService`, `AccessControlService`, `BookingService`
  (covers search, creation, double-booking prevention, restricted-room access,
  modification, cancellation — items 1–10 of the assessment scope)
- A console demo (`StudyRoomBooking.ConsoleDemo`) exercising the happy paths
- A Razor Pages GUI (`StudyRoomBooking.Web`) covering the required student
  workflow end-to-end (search → book → confirm → history → modify → cancel)
  and basic examples of the admin screens (room list/add, bookings overview,
  read-only user list)
- A small number of **example** MSTest tests per area, marked with `TODO`
  comments showing what to add next

## What is deliberately left as team work

### Developer 1 — Core Booking & Validation
- Extend `BookingLifecycleTests` with the edge cases flagged in its `TODO`s
  (adjacent-not-overlapping bookings, modify-into-a-different-conflict,
  cancel-an-already-cancelled-booking, admin override path)
- Add validation rules not yet covered (e.g. minimum/maximum booking
  duration, business-hours restriction) and their tests
- Review `BookingService` for additional conflict-detection edge cases

### Developer 2 — User Management & Administration
- Extend `AccessControlServiceTests` with the cases flagged in its `TODO`s
  (staff with no programme, role-only restrictions, `ManagementUser` role)
- Implement room management operations for administrators (edit/deactivate a
  room, set `AllowedRoles`/`AllowedProgrammes` from the GUI, not just add) —
  `IRoomRepository.Update` already exists to build on
- Implement the admin "override a conflicting booking" workflow in the GUI
  (`BookingRequest.OverrideConflict` already exists in `Application`, just
  needs a page)

### Developer 3 — Quality Assurance & Supporting Features
- Build the requirements traceability matrix mapping each measurable
  requirement in the project brief to its implementing class, page, and test
- Add quality metrics collection (e.g. test pass rate, defect counts)
- Draft usability test scripts (e.g. "complete a booking within 4 minutes"
  using the actual GUI) and run them against `StudyRoomBooking.Web`
- Turn `Pages/Admin/BookingsOverview.cshtml` into the room-utilisation
  reporting view (counts, date-range filters) once metrics are defined

These are suggested starting points, not a rigid contract — rebalance based
on who's comfortable with what.

## Commit planning

Break work into commit-sized tasks. A good commit represents one clear,
reviewable change:

```
Create Room and Booking domain models
Add booking availability validation
Add MSTest coverage for booking conflicts
Implement student room eligibility checking
```

Avoid single giant commits like `Completed entire booking system` — they're
not reviewable and don't demonstrate individual contribution.

Aim for at least 2 meaningful commits per person per week, each with a
message that explains what changed and why, so contribution is visible in
the Git history without needing to ask.
