# I. PROBLEM TITLE 
Quality Assurance Improvement for a University Study Room Booking and Management System 

# II. PROBLEM STATEMENT VERSION 1

There are a great number of universities in Vietnam that still rely heavily on manual processes to manage available rooms for studying or meetings within their facilities. Students who want to book a study room for either individual or group study, project meetings, or exam preparation often need to physically visit the library or student desk, where staff manually check available rooms using paper or whiteboard schedules or spreadsheets in Excel before confirming a booking. In some other cases, these bookings are arranged through phone calls or messaging, resulting in inconsistent record-keeping and consuming the time of both students and university staff. 

When demand is low, this process seems to work fine, but it becomes significantly inefficient during peak periods, especially before mid-term or final exams and assignment deadlines. Students may need to travel back and forth just to find out that a room has already been allocated to another person. Furthermore, manual scheduling also increases the chance of double bookings, overlapping sessions, booking errors, and delays in confirming the availability of the room. Staff also need to spend a great amount of time answering booking enquiries, constantly updating reservation records, and resolving scheduling conflicts instead of focusing on other higher-value administrative tasks. 

These issues expose some important software quality issues. From the student**'s** point of view, the booking process lacks transparency, convenience, and reliability because room availability cannot be checked in real time, and reservations also seem uncertain even though they have already booked the room. Students also have limited ability to change, cancel, or update existing bookings without contacting university staff directly. From the staff perspective, manually managing bookings is repetitive, time-consuming, stressful, and particularly prone to human error, especially when multiple bookings are received simultaneously. University administrators also have limited access to accurate booking information, room utilisation, low and peak booking periods, or demand across different facilities, making it hard to plan ahead for the future and evaluate resource usage to improve efficiency and support students. 

To address this major problem, our project proposes the development of a Study Room Booking and Management Application that provides a centralised platform for managing room bookings. Students will be able to check room availability using filters such as time, date, room capacity, room type, and location. The system will validate booking requests before confirmation, ensuring that the room is available at the chosen time and that users satisfy any access restrictions for specialised rooms. For example, printing and design studios may only be available for Design students unless administrative approval is granted, or biology labs may only be reserved by Chemistry students. Students will also be able to receive booking confirmation via email, view their booking history, and modify, update, or cancel future bookings when necessary. 

Our system will also support different user roles. Academic staff can reserve meeting rooms, conference rooms, laboratories, and also request recurring bookings for teaching activities, weekly discussions, or regular workshops. Administrative users will have additional privileges to manage room information, cancel bookings when necessary, set up access permissions, update room details such as equipment or room capacity, and monitor overall room usage through reporting features. Role-based access control is important to make sure that each user can only perform actions appropriate to their tasks and responsibilities. 

The project scope extends beyond only developing a basic and simple booking application. It focuses on improving software quality assurance throughout the booking workflow by integrating validation rules, conflict detection such as double booking, role-based authorisation, booking history management, quality monitoring, and regular reporting. The prototype will include realistic workflows covering everything from successful bookings to exceptional situations such as duplicate reservations, overlapping time slots, unauthorised room access, invalid booking requests, booking cancellations, and administrative overrides. Sample data will be used to simulate this workflow without requiring integration with any existing university database system. 

This project is a good example of applying Software Quality Assurance because it includes both functional and non-functional quality requirements. Firstly, functional requirements include searching for room availability, creating bookings, modifying bookings, cancelling bookings, validating room eligibility, sending confirmation emails, maintaining room information, and generating usage reports. Non-functional requirements include various principles of usability, reliability, security, performance, maintainability, accessibility, compatibility, and scalability. These quality attributes are important because this application must not only perform the required functions but also provide a secure, reliable, user-friendly, and efficient experience for different types of users. 

The project also provides a chance to apply requirements quality analysis. General requirements such as "the system should be fast and user-friendly" are often too vague for effective testing. These requirements will be refined into measurable and testable statements. For example, a requirement may specify that "this system will prevent overlapping bookings for the same room and will display an appropriate validation message before the booking is confirmed", or "search results will be displayed within less than five seconds when searching a dataset containing at least 100 reservations". Refining requirements helps improve clarity, completeness, consistency, feasibility, correctness, and testability, allowing them to be verified through quality assurance activities. 

The project also supports a testing strategy appropriate for a semester-long Software Quality Assurance project. Testing activities include unit testing, system testing, integration testing, usability testing, acceptance testing, regression testing, and selected non-functional testing. Test cases will verify room searching, booking creation, booking updating, cancellation, email notifications, access control, conflict prevention, validation of specialised rooms, reporting functions, and role-based permissions. A requirements traceability matrix will link each requirement to corresponding test cases, ensuring complete test coverage and supporting future maintenance and change management. 

This prototype will be using sample data to simulate how it works and will not integrate with existing university authentication systems or production databases. Instead, this project will focus on demonstrating software quality assurance practices through a working prototype, documented requirements, quality analysis, GitHub Copilot-assisted development, test planning, traceability, defect management, quality metrics, and evidence of continuous development through GitHub commit history. 

In summary, the proposed system represents a realistic and achievable semester-long project. It involves multiple user roles, validation rules, reporting, workflow management, and quality assurance activities while remaining manageable within the available timeframe. More importantly, it provides sufficient complexity to illustrate the application of software quality assurance principles, responsible AI-assisted development, testing methods, and continuous quality improvement throughout the software development lifecycle. 

# II. PROBLEM STATEMENT VERSION 2 

Many universities in Vietnam still rely on manual processes to manage study and meeting rooms. Students booking a room for study, project meetings, or exam preparation often visit the library or student desk, where staff manually check availability using paper schedules, whiteboards, or spreadsheets. Other bookings are arranged by phone or messaging, leading to inconsistent record-keeping and wasted time for both students and staff. 

This works fine when demand is low, but becomes inefficient during peak periods such as exams and assignment deadlines. Students may travel to a room only to find it already taken. Manual scheduling increases double bookings, overlapping sessions, errors, and delays confirming availability. Staff spend considerable time answering enquiries, updating records, and resolving conflicts instead of higher-value tasks. 

These issues expose important software quality concerns. Students face a process that lacks transparency and reliability, since availability cannot be checked in real time and confirmed bookings still feel uncertain, and they cannot easily change or cancel bookings without contacting staff. Staff face repetitive, error-prone manual management, especially with simultaneous requests. Administrators have limited visibility into utilisation and peak demand, making it hard to plan ahead or improve efficiency. 

To address this, our project proposes a Study Room Booking and Management Application providing a centralised booking platform. Students will check availability using filters such as time, date, capacity, room type, and location. The system will validate requests before confirmation, ensuring rooms are available and users meet access restrictions for specialised rooms — for example, design studios reserved for Design students, unless administrative approval is granted. Students will receive confirmation via email, view booking history, and modify or cancel bookings when needed. 

The system will support different user roles. Academic staff can reserve meeting rooms and laboratories, and request recurring bookings for teaching or workshops. Administrators will manage room information, cancel bookings, set access permissions, update room details, and monitor usage through reporting. Role-based access control ensures users only perform actions appropriate to their responsibilities. 

The project scope extends beyond a basic booking application, focusing on quality assurance throughout the workflow — validation, conflict detection, authorisation, booking history, and reporting. The prototype will cover workflows from successful bookings to exceptions such as duplicate reservations, unauthorised access, and administrative overrides, using sample data rather than integrating with any real university database. 


# III. PROBLEM DEFINITION AND APPROVAL  
#### 1. Meaningful real-world software quality problem 

Many universities in Vietnam still rely heavily on manual processes to manage their study room reservations. Students who want to book a room for either individual or group study, discussions, meetings, or exam preparation are required to travel to the library or student service desk, ask about room availability, and then staff manually check it using paper schedules, whiteboards, or spreadsheets before confirming the booking. In some other cases, bookings are arranged through phone calls or messaging applications, resulting in inconsistent record keeping and overlapping bookings. 

Although this process seems acceptable during periods of low demand, it becomes significantly inefficient during assignment periods, mid-semester tests, and final examinations. Students are not able to book rooms easily and may spend a long time arranging a booking or even discover that their requested room has already been booked by another person. Staff must also spend a great amount of time checking availability, updating booking records, and resolving scheduling issues manually, which reduces productivity and increases the possibility of human error. 

To address these issues, this project proposes the development of a Study Room Booking and Management System. This system will provide a centralised platform where students and staff can search for available rooms, create reservations, update, modify, or cancel bookings, receive booking confirmations, and view their booking history. Administrative users will also be responsible for managing room information, access permissions, booking overrides, and room utilisation reports. 

#### 2. Background and Motivation 
Study rooms play an important role in supporting collaborative learning, presentations, group discussions, tutorials, project meetings, workshops, and independent study. However, the current manual booking process in many universities remains inefficient. 

The motivation for this project is to create ​a centralised platform that​ improves the quality ​of the booking​ process by introducing an automated solution that reduces human error, booking conflicts, improves room visibility, minimises administrative workload, and provides a better user experience for both students and university staff. 

The system will also provide university management with valuable information regarding room utilisation, low and peak booking periods, frequently used facilities, and resource demand, supporting future planning and decision-making. 

#### 3. Stakeholders and Users 
| Stakeholder | Responsibilities | Expected Benefits |
| - | - | - |
| Students | Search for and book study rooms | Book rooms online, save travelling time, view real-time availability, reduce booking conflicts |
| Academic staff | Reserve presentation, meeting rooms, laboratories and teaching spaces | Easier recurring bookings and room management |
| Administrative staff | Manage bookings and room information | Reduced manual workload and the possibility of human error |
| University Management | Monitor room usage and utilisation | Better resource planning and decision-making |

#### 4. Summary of Major Use Cases 
##### 4.1 Student 
- Search available rooms 
- Book a room
- Modify or edit​ a booking
- Cancel​ a booking
- View reservation history​ 
- Receive booking confirmation 

##### 4.2 Academic Staff 
- Book specialised rooms 
- Create recurring bookings 

##### 4.3 Administrator 
- Manage rooms 
- Manage bookings 
- Override bookings 
- Configure room access rules 
- Generate reports 
- Manage user roles 

##### 4.4 For all users 
- Can choose their preferred languages 

#### 5. Software Quality Issues 
The current manual booking process issues identified in this project are analysed using ​the ISO/IEC 25010 software​ product ​quality model, which​ provides internationally recognised quality characteristics for evaluating software systems. Analysing these quality characteristics in the development stages will support identifying potential risks, defining measurable quality requirements, and establishing a strong foundation for subsequent testing, validation, and continuous quality improvement throughout the software development lifecycle. 

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

 

#### 6. Why this project t is suitable for Software Quality Assurance 
This project is highly suitable for apply Software Quality Assurance because it includes both functional and non-functional quality requirements.  

To be more detailed, from a functional requirement, the system includes searching rooms, booking rooms, updating, email notifications, validation rules, role-based access control, reporting, and administrative management. 

While from a quality perspective, this project allows the application of Software Quality Assurance activities including: 

- Requirements quality analysis 
- Requirements traceability 
- Unit testing 
- System testing 
- Integration testing 
- Acceptance testing 
- Regression testing 
- Defect management 
- Quality metrics 
- AI-assisted software development using GitHub Copilot 

This project also evaluates some software ​quality characteristics defined​ in ​ISO/IEC 25010, including​: 
- Functional suitability 
- Usability 
- Reliability 
- Performance efficiency 
- Security 
- Maintainability 

#### 7. Project Feasibility 

This project is achievable within one semester long because it focuses on developing a prototype rather than a production-ready application. 

The system will also use sample data instead of connecting with any existing university databases. The proposed scope includes core booking functionality, user management, validation, reporting, and software quality assurance artefacts while remaining realistic for students. 

# IV. REQUIREMENTS AND QUALITY ANALYSIS    

#### 1. Functional Requirements 
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

#### 2. Non-functional Requirements 
| No | Requirements | Measurement |
| - | - | - |
| NFR1 | Performance | Room searches will return results within 5 seconds for at least 100 reservations |
| NFR2 | Reliability | The system will prevent duplicate bookings for the same room and time slot |
| NFR3 | Security | Only authorised users may access administrative functions |
| NFR4 | Usability | First-time users will complete a booking within 4 minutes without assistance |
| NFR5 | Maintainability | The system will use a modular architecture to simplify future development |
| NFR6 | Accessibility | Text shall remain readable, and navigation will support keyboard interaction where appropriate. The system will support both Vietnamese and English interfaces, allowing users to switch languages without restarting the application |
| NFR7 | Availability | The prototype shall remain operational throughout demonstration and testing sessions |

#### 3. Requirements Quality Analysis 
Several initial requirements are too general or vague and should be refined into measurable requirements 

| Original requirement | Improved requirement | Quality improvement |
| - | - | - |
| The system should be easy to use | A new user should complete a booking within 4 minutes without external assistance | Testable and measurable |
| The system should be fast | Search results should be displayed within 5 seconds for at least 100 reservations | Specific and measurable |
| The system should prevent double bookings | The system should reject overlapping bookings and display an appropriate validation message | Clear and testable |
| Only authorised users should use specialised rooms | The system shall verify programme eligibility before confirming specialised room bookings | Correct and verfiable |

The refined requirements improve clarity, consistency, correctness, completeness, feasibility, and testability, making them easier to verify during testing. 

#### 4. Acceptance Criteria  
| No ID | Functional Requirements | Acceptance Criteria |
| - | - | - |
| FR2 | Book a room | Given a student has selected an available room<br><br>When the student submits the booking request<br><br>Then the booking will be created successfully and a confirmation email shall be sent |
| FR3 | Prevent Double Booking  | Given a room has already been booked for a selected time<br><br>When another user attempts to reserve the same room during the same period<br><br>Then the system will reject booking and display an appropriate validation message |
| FR4 | Programme Validation | Given a student attempts to reserve a specialised laboratory<br><br>When the student’s programme does not satisfy the access requirements<br><br>Then the booking shall not be created and an error message shall be displayed |
| FR7 | Cancel Booking | Given a future booking exists<br><br>When the student cancels the booking<br><br>Then the booking will be removed and the room will become available for future bookings |

#### 5. Software Quality Attributes 
The proposed system will focus on some important software quality attributes 

| Quality Attribute | Importance to the Project |
| - | - |
| Usability | Students should easily search and reserve rooms without training |
| Reliability | Booking conflicts and duplicate bookings must be prevented |
| Performance Efficiency | Room searches and booking confirmations should be completed quickly |
| Security | Restricted rooms and administrator functions should only be accessible to authorised users |
| Maintainability | The system should support future enhancements with minimal changes |
| Accessibility | The interface should be usable by users with different accessibility needs. |

# V. Initial Test Strategy and Test Planning
The initial test strategy for the Study Room Booking System focuses on verifying the correctness of the core booking functionality, access-control rules, room searching, and booking lifecycle operations. Testing is designed to identify defects early and provide evidence that the implemented requirements are satisfied.

#### 1 Testing Scope
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

#### 2 Test Levels and Types
**Unit testing** is the main testing level for the current implementation. MSTest is used in Visual Studio to test individual application services and business rules. The existing test project includes tests for `AccessControlService`, `BookingService`, and `RoomSearchService`. Each test uses a fresh `TestFixture` containing isolated in-memory repositories, which prevents test data from affecting other tests.

The main testing approach is **functional unit testing**, using both positive and negative test cases. **Regression testing** is also performed by running the existing MSTest test suite after changes to ensure that previously working functionality has not been broken.

Integration, system, acceptance, usability, and security testing are not the main focus of the current assessment because the testing work is limited to the MSTest project and the core application services.

#### 3 Functional Testing Approach
Functional testing uses a combination of positive and negative test cases. Positive tests verify that valid operations succeed, while negative tests verify that invalid requests are rejected with the correct result or error code.

Boundary and business-rule scenarios are also tested. For example, booking times that overlap an existing booking should fail, while a cancelled booking should release the room so that another user can book the same time slot.

The tests are implemented using MSTest. Assertions check both the operation result and the resulting system state, such as booking status, error codes, and room availability.

#### 4 Non-Functional Testing Approach
Selected non-functional requirements will be tested at an initial level. Reliability is assessed by checking that invalid operations do not corrupt booking state and that cancellation correctly releases a booking slot.

Usability can be evaluated through manual system testing by checking whether the main workflows are clear and understandable.

Maintainability is supported through automated unit tests, isolated test fixtures, and clear separation between the Domain, Application, Infrastructure, and Web projects.

Performance and scalability testing are not included at this stage because the system uses in-memory repositories and is designed as an Assessment 1 prototype rather than a production deployment.

##### 5 Entry and Exit Criteria
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

#### 6 Test Environment and Tools
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


