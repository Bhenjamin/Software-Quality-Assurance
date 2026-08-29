# Team Task Breakdown

This document exists because the project is built by a three-person team, and
the AI-generated foundation must leave real, demonstrable work for each
member — not hand back a finished system.

## What the AI foundation includes

- Domain models: `Room`, `User`, `Booking` plus enums such as `RoomType`, `UserRole`, and `BookingStatus`
- Repository interfaces and in-memory implementations, designed so they can be swapped for a real database later
- Core services: `RoomSearchService`, `AccessControlService`, and `BookingService`
  covering search, booking creation, overlap prevention, restricted-room access,
  booking modification, and cancellation
- A console demo (`StudyRoomBooking.ConsoleDemo`) exercising the main happy paths
- A Razor Pages GUI (`StudyRoomBooking.Web`) covering the student booking workflow
  from search through booking confirmation, history, modification, and cancellation,
  plus basic admin screens for room listing and booking overview
- A small number of example MSTest tests per area, with `TODO` markers showing what to add next

## What is deliberately left as team work

### Developer 1 — Core Booking & Validation
- Extend `BookingLifecycleTests` with the edge cases flagged in its `TODO`s,
  including adjacent-not-overlapping bookings, modify-into-a-conflict,
  cancel-an-already-cancelled-booking, and the admin override path
- Add validation rules not yet covered, such as minimum/maximum booking duration
  or business-hours restrictions, and their corresponding tests
- Review `BookingService` for additional edge cases in conflict detection and validation

### Developer 2 — User Management & Administration
- Extend `AccessControlServiceTests` with the cases flagged in its `TODO`s,
  such as staff with no programme, role-only restrictions, and `ManagementUser` access
- Implement room management operations for administrators, including edit and deactivate flows,
  plus GUI support for setting `AllowedRoles` and `AllowedProgrammes`
- Implement the admin override workflow for conflicting bookings in the GUI,
  building on the existing `BookingRequest.OverrideConflict` support in the application layer

### Developer 3 — Quality Assurance & Supporting Features
- Build the requirements traceability matrix linking each measurable requirement
  to the implementing class, page, and test
- Add quality metrics collection, such as test pass rate and defect counts
- Draft usability test scripts and run them against the actual GUI in `StudyRoomBooking.Web`
- Turn the admin bookings overview into a room-utilisation reporting view with date-range filters and summary counts

These are suggested starting points, not a rigid contract; re-balance based on
who is most comfortable with each area.

## Commit planning

Break work into commit-sized tasks. A good commit represents one clear,
reviewable change:

```text
Create Room and Booking domain models
Add booking availability validation
Add MSTest coverage for booking conflicts
Implement student room eligibility checking
```

Avoid single giant commits like `Completed entire booking system` — they're not
reviewable and do not demonstrate individual contribution clearly.

Aim for at least 2 meaningful commits per person per week, each with a message that
explains what changed and why, so contribution is visible in the Git history without
needing to ask.
