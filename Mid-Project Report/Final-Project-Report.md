# I.	Introduction


# II. Project Completion & Final Implementation

# III. Requirements Verification & Traceability
## 3.1 Review and Finalisation of Requirements
The functional requirements (FR1 - FR12) and non-functional requirements (NFR1 - NFR7) defined in Assessment 1 (Tables 3 & 4) have been reviewed against the final implementation. All twelve functional requirements remain valid and relevant to the completed system; no requirements were removed. Eleven of the twelve functional requirements have now been implemented. FR12 (administrator reporting) has been formally deferred to a later sprint, as documented in Section 1.3.

Functional Requirements
No ID	Functional requirements
FR1	    Students can search available rooms using date, time, location, capacity and room type filters
FR2	    Students can book an available room
FR3	    The system shall validate the availability of study rooms before confirming a booking
FR4	    The system will validate programme or major eligibility before allowing specialised room bookings
FR5	    Student will receive booking confirmation via email
FR6	    Student can modify future bookings
FR7	    Student can cancel future bookings
FR8	    Student can view booking history
FR9	    Academic staff can create recurring bookings
FR10	Administrators can manage room information and room access rules
FR11	Administrators can override or cancel bookings when necessary
FR12	Administrators can generate room utilisation reports

Non-Functional Requirements
No ID	Requirements	Measurement
NFR1	Performance	    Room searches will return results within 2 seconds for at least 50 reservations
NFR2	Reliability	    The system will prevent duplicate bookings for the same room and time slot
NFR3	Security	    Only authorised users may access administrative functions
NFR4	Usability	    First-time users will complete a booking within 4 minutes without assistance
NFR5	Maintainability	The system will use a modular architecture to simplify future development
NFR6	Accessibility	Text shall remain readable, and navigation will support keyboard interaction where appropriate. The system will support both Vietnamese and English interfaces, allowing users to switch languages without restarting the application
NFR7	Availability	The prototype shall remain operational throughout demonstration and testing sessions

## 3.2 Updated Requirements Traceability Matrix
The initial test cases (TC-01 to TC-08) designed in Assessment 1 (Table 8) have been reviewed and expanded with twelve additional test cases (TC-09 to TC-20) to achieve broader coverage across all functional and non-functional requirements, bringing the total to 20 test cases ahead of execution in Task 3.

| Test ID   | Requirement Covered    | Test Case Description                                                    | Test Steps (Given/When/Then)                                                                                                                                                                                                                                                    | Expected Result                                                                                                  |
| --------- | ---------------------- | ------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| **TC-01** | FR2, FR3               | Book an available room successfully                                      | **Given** a room is available for the selected date/time<br>**When** a student submits a booking request<br>**Then** the booking is created                                                                                                                                     | Booking succeeds. `BookingResult.Success = true`; confirmation message returned                                  |
| **TC-02** | FR3, NFR2              | Reject a double-booking for the same room and time slot                  | **Given** a room is already booked for a time slot<br>**When** another student attempts to book the same room and time<br>**Then** the booking is rejected                                                                                                                      | `BookingResult.Success = false`; validation message explains the conflict                                        |
| **TC-03** | FR4, NFR3              | Hide a specialised room from an ineligible student’s search results      | **Given** a student’s programme doesn’t match a room’s access requirement (e.g. a Business student searching while a room is restricted to Design students)<br>**When** the student searches for available rooms<br>**Then** the restricted room does not appear in the results | The restricted room is excluded from the returned room list                                                      |
| **TC-04** | FR4, NFR3              | Show a specialised room in an eligible student’s search results          | **Given** a student’s programme matches a room’s access requirement<br>**When** the student searches for available rooms<br>**Then** the restricted room appears in the results                                                                                                 | The restricted room is included in the returned room list                                                        |
| **TC-05** | FR6                    | Modify an existing future booking to a new available time                | **Given** a student has an existing future booking<br>**When** the student changes the booking to a new available date/time<br>**Then** the booking is updated successfully                                                                                                     | `BookingResult.Success = true`; booking reflects the new date/time, and the original time slot becomes available |
| **TC-06** | FR7                    | Cancel a future booking                                                  | **Given** a future booking exists<br>**When** the student cancels the booking<br>**Then** the booking will be removed and the room will become available for future bookings                                                                                                    | Booking status updated to cancelled; room reappears in availability search                                       |
| **TC-07** | FR3 (edge case)        | Reject a booking request for a date/time in the past                     | **Given** a student attempts to book a room for a date/time that has already passed<br>**When** the booking request is submitted<br>**Then** the system rejects the booking                                                                                                     | `BookingResult.Success = false`; message explains the date is invalid, or `ArgumentException` thrown             |
| **TC-08** | FR2 (edge case)        | Reject a booking with a missing / invalid student ID                     | **Given** a booking request with an empty or null student ID<br>**When** the booking is submitted<br>**Then** the system rejects it                                                                                                                                             | `ArgumentException` thrown or `BookingResult.Success = false` with a clear message                               |
| **TC-09** | FR1                    | Search returns rooms matching all selected filters                       | **Given** rooms with varying date, time, capacity, and room type<br>**When** a student searches using a combination of filters<br>**Then** only rooms matching all criteria are returned                                                                                        | Correct filtered result set; no non-matching rooms included                                                      |
| **TC-10** | FR1 (negative)         | Search with no matching rooms returns a clear empty-result message       | **Given** no rooms match the selected filters<br>**When** a student searches<br>**Then** the system displays a “no rooms found” message                                                                                                                                         | Empty result set; clear message shown, no error thrown                                                           |
| **TC-11** | FR5                    | Confirmation email is sent with correct booking details                  | **Given** a booking is successfully created<br>**When** the confirmation email is generated<br>**Then** the email contains the correct room, date, time, and booking reference                                                                                                  | Email content matches the actual booking details                                                                 |
| **TC-12** | FR8                    | Booking history displays correct status for past and upcoming bookings   | **Given** a student has a mix of expired, cancelled, and upcoming bookings<br>**When** the student views booking history<br>**Then** each booking shows the correct status                                                                                                      | All bookings listed with accurate status labels                                                                  |
| **TC-13** | FR9                    | Recurring booking creates a linked series, skipping conflicts            | **Given** academic staff request a weekly recurring booking over 4 weeks<br>**When** one occurrence conflicts with an existing booking<br>**Then** the system creates the other 3 bookings and flags/skips the conflicting one                                                  | 3 bookings created; conflicting occurrence flagged, not silently dropped                                         |
| **TC-14** | FR10, NFR3             | Administrator can update room access rules                               | **Given** an administrator is logged in<br>**When** they update a room’s access rules (e.g. restrict to Design students)<br>**Then** the new rule applies to all future bookings of that room                                                                                   | Updated rule saved and enforced on subsequent booking attempts                                                   |
| **TC-15** | FR11, NFR3             | Non-administrator cannot override or cancel another user’s booking       | **Given** a logged-in student (non-admin)<br>**When** they attempt to cancel another student’s booking<br>**Then** the system denies the action                                                                                                                                 | Action blocked; appropriate authorisation error returned                                                         |
| **TC-16** | FR11                   | Administrator override cancels booking and notifies the affected student | **Given** an administrator overrides an existing booking<br>**When** the override is confirmed<br>**Then** the booking is cancelled, the room becomes available, and the student is notified                                                                                    | Booking cancelled; room freed; notification sent/logged                                                          |
| **TC-17** | NFR1 (Performance)     | Room search returns results within 2 seconds for 50+ reservations        | **Given** at least 50 sample reservations exist in the database<br>**When** a student performs a search<br>**Then** results are returned within 2 seconds                                                                                                                       | Search completes within the 2-second threshold                                                                   |
| **TC-18** | NFR4 (Usability)       | First-time user completes a booking within 4 minutes unassisted          | **Given** a first-time user with no prior instruction<br>**When** they attempt to search for and book a room<br>**Then** they complete the booking within 4 minutes                                                                                                             | Booking completed within the time threshold, without external help                                               |
| **TC-19** | NFR6 (Accessibility)   | Language switch applies immediately without restarting the application   | **Given** a user is on any page of the application<br>**When** they select a different language (Vietnamese/English)<br>**Then** the interface text updates without requiring the program restart                                                                               | All visible text updates to the selected language; no restart required                                           |
| **TC-20** | NFR2, FR3 (Regression) | Re-verify double-booking prevention after database migration             | **Given** the system now uses a database instead of in-memory storage<br>**When** two students attempt to book the same room and time concurrently<br>**Then** the system still correctly rejects the conflicting booking                                                       | Double-booking still prevented after the architecture change (regression check)                                  |

Requirements Traceability Matrix
| Requirement ID | Requirement Summary                                        | Related NFR (if any) | Test Cases                 | Status                 |
| -------------- | ---------------------------------------------------------- | -------------------- | -------------------------- | ---------------------- |
| **FR1**        | Search available rooms by filters                          | NFR1                 | TC-09, TC-10               | Pending execution      |
| **FR2**        | Book an available room                                     | —                    | TC-01, TC-08               | Passed                 |
| **FR3**        | Validate room availability before confirming booking       | NFR2                 | TC-01, TC-02, TC-07, TC-20 | Passed (TC-20 pending) |
| **FR4**        | Validate programme/major eligibility for specialised rooms | NFR3                 | TC-03, TC-04               | Passed                 |
| **FR5**        | Send booking confirmation via email                        | —                    | TC-11                      | Passed                 |
| **FR6**        | Modify future bookings                                     | —                    | TC-05                      | Passed                 |
| **FR7**        | Cancel future bookings                                     | —                    | TC-06                      | Passed                 |
| **FR8**        | View booking history                                       | —                    | TC-12                      | Pending execution      |
| **FR9**        | Academic staff can create recurring bookings               | —                    | TC-13                      | Pending execution      |
| **FR10**       | Administrators manage room information and access rules    | NFR3                 | TC-14                      | Pending execution      |
| **FR11**       | Administrators override or cancel bookings                 | NFR3                 | TC-15, TC-16               | Pending execution      |
| **FR12**       | Administrators generate room utilisation reports           | —                    | None — out of scope        | Deferred (see 2.4)     |

Non-Requirements Traceability Matrix
| Requirement ID | Requirement Summary                                  | Test Cases                 | Status                                                     |
| -------------- | ---------------------------------------------------- | -------------------------- | ---------------------------------------------------------- |
| **NFR1**       | Performance (search ≤2s, 50+ reservations)           | TC-17                      | Pending execution                                          |
| **NFR2**       | Reliability (no duplicate bookings)                  | TC-02, TC-20               | Passed (TC-20 pending)                                     |
| **NFR3**       | Security (authorised access only)                    | TC-03, TC-04, TC-14, TC-15 | Partially verified; TC-14/15 pending                       |
| **NFR4**       | Usability (4-min unassisted booking)                 | TC-18                      | Pending execution                                          |
| **NFR5**       | Maintainability (modular architecture)               | —                          | Supported by database migration; no direct test case       |
| **NFR6**       | Accessibility (language switch; keyboard navigation) | TC-19                      | Partially covered — TC-19 verifies language switching only |
| **NFR7**       | Availability (operational during demo/testing)       | —                          | Observed during testing sessions; not a discrete test case |

## 3.3.	Requirements Not Fully Verified or Out of Scope
- FR12 — Administrators can generate room utilisation reports
=> Formally deferred to a later sprint (see Section 1.3). The team prioritised database implementation, administrator management, email confirmation, multi-language support, and recurring bookings for this assignment. FR12 remains valid and will be addressed in a future iteration.
- TC-09 through TC-20 — Designed but not yet executed (Test in Task 3, will update once done)
=> Twelve new test cases were added during this review to close coverage gaps identified in Assessment 1's original eight test cases, extending coverage to FR1, FR5, FR8, FR9, FR10, FR11, and all seven NFRs. These have been designed but not yet run against the completed system; execution and results recording are addressed in Task 3.
- NFR6 — Partially covered
=> The original test case for NFR6 covered both keyboard navigation and language switching. Following a revision to TC-19, the test case now verifies language switching only; keyboard navigation through the booking flow is not currently covered by any test case. This is a known gap the team has chosen to accept at this stage, given the project's limited timeframe and focus on core booking functionality; it is flagged here for transparency rather than silently omitted, and may be addressed in a future iteration if time permits.
- NFR5 and NFR7 — No discrete test case
=> NFR5 (Maintainability) is supported by the architectural decision to migrate from in-memory repositories to a database (Section 1.3), rather than verified through a specific automated test. NFR7 (Availability) will be confirmed through observation during demonstration and testing sessions rather than a standalone test case, as uptime is not something a unit or integration test can meaningfully simulate for this prototype.

