# 1.	Introduction
Software quality assurance (SQA) is an essential part of software development because developing software quickly does not necessarily guarantee that the resulting software will be of high quality. Modern software organisations increasingly rely on Agile practices, automation, DevOps workflows, and AI to improve development efficiency. However, these approaches can also introduce software quality concerns such as unclear requirements, insufficient validation, poor usability, unreliable workflows, security issues, maintainability problems, and inadequate testing. Therefore, systematic quality assurance practices are necessary to identify, prevent, and reduce potential software quality problems throughout the software development process.
This project focuses on identifying a meaningful real-world software quality problem and developing a software-based solution or quality improvement prototype. The project involves understanding the problem context and stakeholder needs, identifying functional and non-functional requirements, and analysing these requirements to ensure that they are clear, complete, consistent, correct, feasible, and testable. Relevant software quality attributes are also considered to ensure that the proposed solution meets both functional expectations and broader quality requirements.
AI-assisted development is also incorporated into the project to support software development and quality assurance activities. AI can assist with tasks such as code development, testing, validation, debugging, and quality improvement. However, AI-generated outputs may contain errors, security vulnerabilities, inappropriate solutions, or maintainability issues. Therefore, AI-assisted outputs must be reviewed, validated, modified, and tested by the development team to ensure that they are appropriate and meet the required quality standards.
The project also uses GitHub to support collaborative development, version control, and project progress tracking. Git commits provide a record of the team's development activities and individual contributions throughout the project. Combined with systematic testing and quality assurance practices, including test case design, requirements traceability, defect management, and quality metrics, GitHub supports a structured development process. GitHub link: https://github.com/Bhenjamin/Software-Quality-Assurance.
Overall, the project demonstrates the application of software quality assurance principles, practical software development, collaborative version control, and responsible use of AI to address a real-world software quality problem and improve the quality of the proposed solution.

# 2. Problem Definition 
## 2.1. Background and Motivation
Study rooms support collaborative learning, presentations, group discussions, tutorials, project meetings, workshops, and independent study. However, the manual booking process used by many universities remains inefficient.
This project aims to develop a centralised platform that improves the booking process through automation — reducing human error and booking conflicts, improving room visibility, minimising administrative workload, and providing a better user experience for students and staff.
The system will also give university management data on room utilisation, peak and off-peak periods, frequently used facilities, and overall demand, supporting better planning and decision-making.

## 2.2. Problem Statement
Many universities in Vietnam still rely on manual processes to manage study and meeting rooms. Students typically visit the library or student desk in person, where staff check availability using paper schedules, whiteboards, or spreadsheets; other bookings are arranged informally by phone or messaging, leading to inconsistent records and wasted time.
This is manageable during low demand but becomes inefficient during peak periods such as exams and assignment deadlines, when students may travel to a room only to find it occupied. Manual scheduling increases double bookings, errors, and delays, while staff spend considerable time on enquiries and conflict resolution rather than higher-value tasks. For students, this means limited transparency, uncertainty even after a booking is confirmed, and difficulty modifying or cancelling bookings without contacting staff. For administrators, limited visibility into utilisation and demand makes planning difficult.
To address this, the project proposes a Study Room Booking and Management Application providing a centralised platform. Students will check availability using filters such as time, date, capacity, room type, and location, with the system validating requests before confirmation — including access requirements for specialised rooms (e.g. design studios reserved for Design students, unless administrative approval is granted). Students will receive email confirmation, view booking history, and modify or cancel bookings as needed.
The system will support multiple roles: academic staff can reserve meeting rooms and laboratories and request recurring bookings; administrators manage room information, cancel bookings, configure access permissions, and monitor usage through reporting. Role-based access control ensures users only perform actions appropriate to their responsibilities.

## 2.3. Stakeholders and Users 
| Stakeholder | Responsibilities | Expected Benefits |
| - | - | - |
| Students | Search for and book study rooms | Book rooms online, save travelling time, view real-time availability, reduce booking conflicts |
| Academic staff | Reserve presentation, meeting rooms, laboratories and teaching spaces | Easier recurring bookings and room management |
| Administrative staff | Manage bookings and room information | Reduced manual workload and the possibility of human error |
| University Management | Monitor room usage and utilisation | Better resource planning and decision-making |
Table 1 – Stakeholder, Responsibilities and Expected Benefits

Summary of Major Use Cases:
•	Student:
o	Search available rooms
o	Book a room
o	Modify or edit a booking
o	Cancel a booking
o	View reservation history
o	Receive booking confirmation
•	Academic Staff:
o	Book specialised rooms
o	Create recurring bookings
•	Administrator:
o	Manage rooms
o	Manage bookings
o	Override bookings
o	Configure room access rules
o	Generate reports
o	Manage user roles
•	For all users:
o	Can choose their preferred languages
This project is highly suitable for apply Software Quality Assurance because it includes both functional and non-functional quality requirements.

## 2.4. Software Quality Issues
The current manual booking process issues identified in this project are analysed using the ISO/IEC 25010 software product quality model, which provides internationally recognised quality characteristics for evaluating software systems. Analysing these quality characteristics in the development stages will support identifying potential risks, defining measurable quality requirements, and establishing a strong foundation for subsequent testing, validation, and continuous quality improvement throughout the software development lifecycle.
**Functional Suitability**
The current process does not satisfy users’ needs because room availability is checked manually. The increases of possibility of incorrect bookings, inconsistent booking records, and booking conflicts. The proposed system should solve these issues by correctly perfoming searching, booking, validation, cancellation, modification and reporting functions. 
**Reliability**
Manual booking process increases the likelihood of duplicate bookings, overlapping bookings, and lost booking information. The proposed system should maintain accurate booking records and prevent any conflicting reservations through automatic validation. 
**Usability**
Students need to physically visit the library or contact staff before knowing whether the room is available. The booking process is inconvenient, time-consuming, and difficult to follow. The proposed system should provide an intuitive interface that support users to search, modify, book, and cancel bookings when necessary. 

**Performance Efficiency**
During peak periods such as assignment deadlines or final exams, many students may search for rooms simultaneously. The system should return search results and booking confirmation quickly to ensure a smooth user experience during peak demand. 

**Security**
Different room types require different access permissions. For example, the design rooms should only be accessible to authorised students or staff. Administrative functions should also be protected through role-based access control. 

**Maintainability**
Room information, booking policies, and access control rules may change over time. The system shall be designed using a modular architecture in order to support future changes without affecting existing functionality. 

## 2.5. Project Scope
The project will develop a prototype Study Room Booking and Management System covering room searching, booking, modification and cancellation, validation, double-booking prevention, role-based access control, and basic administrative management. Email notifications and reporting will be included at a basic prototype level where practicable.
From an SQA perspective, the project will cover requirements quality analysis, traceability, unit/integration/system/acceptance/regression testing, defect management, and quality measurement, evaluating selected ISO/IEC 25010 characteristics including functional suitability, usability, reliability, performance efficiency, security, and maintainability.
The scope is intentionally limited to a prototype, allowing a three-person team to implement and test core functionality within the available timeframe while retaining enough complexity for meaningful SQA activities.

## 2.6. Project Feasibility
This project is achievable within one semester, as it focuses on a prototype rather than a production-ready application.
The system will use sample data rather than integrating with existing university databases. The scope covers core booking functionality, user management, validation, reporting, and SQA artefacts, while remaining realistic for a student project.


# 3. Requirements and Quality Analysis
## 3.1. Functional Requirements
| No ID | Functional requirements |
| - | - |
| FR1 | Students can search available rooms using date, time, location, capacity and room type filters |
| FR2 | Students can book an available room |
| FR3 | The system shall validate the availability of study rooms before confirming a booking |
| FR4 | The system will validate programme or major eligibility before allowing specialised room bookings |
| FR5 | Student will receive booking confirmation via email |
| FR6 | Student can modify future bookings |
| FR7 | Student can cancel future bookings |
| FR8 | Student can view booking history |
| FR9 | Academic staff can create recurring bookings |
| FR10 | Administrators can manage room information and room access rules |
| FR11 | Administrators can override or cancel bookings when necessary |
| FR12 | Administrators can generate room utilisation reports |

## 3.2. Non-functional Requirements
| No | Requirements | Measurement |
| - | - | - |
| NFR1 | Performance | Room searches will return results within 5 seconds for at least 100 reservations |
| NFR2 | Reliability | The system will prevent duplicate bookings for the same room and time slot |
| NFR3 | Security | Only authorised users may access administrative functions |
| NFR4 | Usability | First-time users will complete a booking within 4 minutes without assistance |
| NFR5 | Maintainability | The system will use a modular architecture to simplify future development |
| NFR6 | Accessibility | Text shall remain readable, and navigation will support keyboard interaction where appropriate. The system will support both Vietnamese and English interfaces, allowing users to switch languages without restarting the application |
| NFR7 | Availability | The prototype shall remain operational throughout demonstration and testing sessions |

## 3.3. Requirements Quality Analysis
Several initial requirements are too general or vague and should be refined into measurable requirements 

| Original requirement | Improved requirement | Quality improvement |
| - | - | - |
| The system should be easy to use | A new user should complete a booking within 4 minutes without external assistance | Testable and measurable |
| The system should be fast | Search results should be displayed within 5 seconds for at least 100 reservations | Specific and measurable |
| The system should prevent double bookings | The system should reject overlapping bookings and display an appropriate validation message | Clear and testable |
| Only authorised users should use specialised rooms | The system shall verify programme eligibility before confirming specialised room bookings | Correct and verfiable |

The refined requirements improve clarity, consistency, correctness, completeness, feasibility, and testability, making them easier to verify during testing.

## 3.4. Acceptance Criteria
| No ID | Functional Requirements | Acceptance Criteria |
| - | - | - |
| FR2 | Book a room | Given a student has selected an available room<br><br>When the student submits the booking request<br><br>Then the booking will be created successfully and a confirmation email shall be sent |
| FR3 | Prevent Double Booking  | Given a room has already been booked for a selected time<br><br>When another user attempts to reserve the same room during the same period<br><br>Then the system will reject booking and display an appropriate validation message |
| FR4 | Programme Validation | Given a student attempts to reserve a specialised laboratory<br><br>When the student’s programme does not satisfy the access requirements<br><br>Then the booking shall not be created and an error message shall be displayed |
| FR7 | Cancel Booking | Given a future booking exists<br><br>When the student cancels the booking<br><br>Then the booking will be removed and the room will become available for future bookings |

## 3.5. Software Quality Attributes
The proposed system will focus on some important software quality attributes 

| Quality Attribute | Importance to the Project |
| - | - |
| Usability | Students should easily search and reserve rooms without training |
| Reliability | Booking conflicts and duplicate bookings must be prevented |
| Performance Efficiency | Room searches and booking confirmations should be completed quickly |
| Security | Restricted rooms and administrator functions should only be accessible to authorised users |
| Maintainability | The system should support future enhancements with minimal changes |
| Accessibility | The interface should be usable by users with different accessibility needs. |

## 3.6. Summary and Traceability
Each requirement — including both functional and non-functional requirements, as well as their associated acceptance criteria — has been assigned a unique identifier (FR1–FR12 for functional requirements and NFR1–NFR7 for non-functional requirements, with corresponding acceptance criteria linked to their respective functional requirements) to support consistent referencing throughout the project. These identifiers are subsequently used in the Requirements Traceability Matrix (Task 6) to link each requirement directly to the test case(s) designed to verify it, enabling the group to demonstrate that every requirement has been considered during the testing process and that no requirement has been left unverified.

# 4. Proposed Solution and Initial Prototype

# 5. AI-Assisted Development Using GitHub Copilot

# 6. Initial Test Strategy and Test Planning
## 6.1. Testing Scope

## 6.2. Test Levels and Types

## 6.3. Functional Testing Approach

## 6.4. Non-Functional Testing Approach

## 6.5. Entry and Exit Criteria

## 6.6. Test Environment and Tools

# 7. Initial Test Cases and Requirements Traceability
## 7.1. Initial Test Cases

## 7.2. Requirements Traceability

# 8. Project Progress, Risks, and Next Steps

# 9. Conclusion

***



# V. Initial Test Strategy and Test Planning
The initial test strategy for the Study Room Booking System focuses on verifying the correctness of the core booking functionality, access-control rules, room searching, and booking lifecycle operations. Testing is designed to identify defects early and provide evidence that the implemented requirements are satisfied.

#### 1. Testing Scope
The testing scope includes the main functional requirements of the system:
* User and role-based access control
* Room searching and availability checking
* Room booking and validation
* Prevention of double bookings
* Booking modification and cancellation
* Access to restricted rooms
* Administration functions such as viewing users, rooms, and bookings

Selected non-functional requirements are also considered, particularly reliability, usability, and maintainability.

The following items are out of scope:
* Production authentication and authorisation infrastructure
* Real database performance and scalability
* Real external university systems
* Deployment and infrastructure testing
* Mobile application testing
* Full penetration and security testing

The system currently uses in-memory repositories and simulated login. Therefore, testing is primarily focused on application behaviour rather than production infrastructure.

#### 2. Test Levels and Types
**Unit testing** is the main testing level for the current implementation. MSTest is used in Visual Studio to test individual application services and business rules. The existing test project includes tests for `AccessControlService`, `BookingService`, and `RoomSearchService`. Each test uses a fresh `TestFixture` containing isolated in-memory repositories, which prevents test data from affecting other tests.

The main testing approach is **functional unit testing**, using both positive and negative test cases. **Regression testing** is also performed by running the existing MSTest test suite after changes to ensure that previously working functionality has not been broken.

Integration, system, acceptance, usability, and security testing are not the main focus of the current assessment because the testing work is limited to the MSTest project and the core application services.

#### 3. Functional Testing Approach
Functional testing uses a combination of positive and negative test cases. Positive tests verify that valid operations succeed, while negative tests verify that invalid requests are rejected with the correct result or error code.

Boundary and business-rule scenarios are also tested. For example, booking times that overlap an existing booking should fail, while a cancelled booking should release the room so that another user can book the same time slot.

The tests are implemented using MSTest. Assertions check both the operation result and the resulting system state, such as booking status, error codes, and room availability.

#### 4. Non-Functional Testing Approach
Selected non-functional requirements will be tested at an initial level. Reliability is assessed by checking that invalid operations do not corrupt booking state and that cancellation correctly releases a booking slot.

Usability can be evaluated through manual system testing by checking whether the main workflows are clear and understandable.

Maintainability is supported through automated unit tests, isolated test fixtures, and clear separation between the Domain, Application, Infrastructure, and Web projects.

Performance and scalability testing are not included at this stage because the system uses in-memory repositories and is designed as an Assessment 1 prototype rather than a production deployment.

##### 5. Entry and Exit Criteria
###### Entry Criteria
* Core application services compile successfully.
* The test project references the required Domain, Application, and Infrastructure projects.
* Seed data and in-memory repositories are available.
* The test environment is configured with .NET 8 and MSTest.

##### Exit Criteria
* All planned high-priority test cases have been executed.
* All critical functional tests pass.
* No unresolved critical defects remain.
* Failed tests have been investigated and documented.
* Regression tests pass after defect corrections.

#### 6. Test Environment and Tools
Testing is performed using **Visual Studio** and **C#** with the **MSTest** framework. Automated unit tests are executed through **Test Explorer** in Visual Studio.

The test project uses the following dependencies:
* `MSTest.TestFramework` — provides test attributes and assertions such as `[TestClass]`, `[TestMethod]`, and `Assert`.
* `MSTest.TestAdapter` — enables MSTest tests to be discovered and executed.
* `Microsoft.NET.Test.Sdk` — provides the test execution infrastructure.

The tests use **in-memory repositories** and seeded data, so an external database or external service is not required for the initial unit testing.

The current testing focuses on the **C# Domain, Application, and Infrastructure components**. Browser and mobile-device testing are not included in the initial unit-testing scope. The Web interface can be tested separately through manual system testing if required.

***

## Task 3: Proposed Solution and Initial Prototype 

How the proposed solution addresses the selected real-world problem. 

The proposed solution is to develop an online Study Room Booking and Management System that replaces the current manual booking process with a C# web platform.  The system addresses initially quality issues by allowing students and staff to find and book available rooms without needing to contact administrative staff directly.  Having an online booking system will help prevent double bookings, overlapping reservations, and unauthorised access to specialised rooms.  The system will also support different user roles, including students, academic staff, and administrators, preventing access features outside their responsibilities.  By providing these features with a user-friendly interface the proposed solution will help reduce human error, decreases administrative workload, and provide a more efficient experience for all stakeholders. 

# VI. INITIAL TEST CASES AND REQUIREMENTS TRACEABILITY
#### 1 Design at least eight (8) initial test cases
| Test ID | Requirement Covered | Test Case Description | Test Steps (Given/When/Then) | Expected Result |
|---|---|---|---|---|
| TC-01 | FR2, FR3 | Book an available room successfully | **Given** a room is available for the selected date/time<br>**When** a student submits a booking request<br>**Then** the booking is created | Booking succeeds; `BookingResult.Success = true`; confirmation message returned |
| TC-02 | FR3, NFR2 | Reject a double-booking for the same room and time slot | **Given** a room is already booked for a time slot<br>**When** another student attempts to book the same room and time<br>**Then** the booking is rejected | `BookingResult.Success = false`; validation message explains the conflict |
| TC-03 | FR4 | Reject a specialised room booking from an ineligible student | **Given** a student's programme does not match a room's access requirement (e.g. a Business student booking the Design studio)<br>**When** the student submits the booking<br>**Then** the booking is rejected | `BookingResult.Success = false`; message explains eligibility requirement |
| TC-04 | FR4 | Allow a specialised room booking from an eligible student | **Given** a student's programme matches the room's access requirements<br>**When** the student submits the booking<br>**Then** the booking succeeds | `BookingResult.Success = true`; message confirms the booking succeeded |
| TC-05 | FR5 | Booking confirmation is sent via email | **Given** a booking is successfully created<br>**When** the booking is confirmed<br>**Then** a confirmation email is triggered | Email notification is generated with correct booking details |
| TC-06 | FR7 | Cancel a future booking | **Given** a future booking exists<br>**When** the student cancels the booking<br>**Then** the booking will be removed and the room will become available for future bookings | Booking status updated to cancelled; room reappears in availability search |
| TC-07 | FR1, NFR1 | Room search returns results within performance target | **Given** at least 50 sample reservations exist<br>**When** a student searches using date/time/capacity/room type filters<br>**Then** results are returned within 5 seconds | Search results returned correctly and within the 5-second threshold |
| TC-08 | FR2 (edge case) | Reject a booking with a missing/invalid student ID | **Given** a booking request with an empty or null student ID<br>**When** the booking is submitted<br>**Then** the system rejects it | `ArgumentException` thrown, or `BookingResult.Success = false` with a clear message |

#### 2 Requirements Traceability Matrix
| Requirement ID | Requirement Summary | Related NFR (if any) | Test Case(s) | Status |
|---|---|---|---|---|
| FR1 | Search available rooms by filters | NFR1 | TC-07 | Pending |
| FR2 | Book an available room | — | TC-01, TC-08 | Pending |
| FR3 | Validate room availability before confirming booking | NFR2 | TC-01, TC-02 | Pending |
| FR4 | Validate programme/major eligibility for specialised rooms | — | TC-03, TC-04 | Pending |
| FR5 | Send booking confirmation via email | NFR3 | TC-05 | Pending |
| FR7 | Cancel future bookings | NFR3 | TC-06 | Pending |
