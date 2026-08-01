I. PROBLEM TITLE 

Quality Assurance Improvement for a University Study Room Booking and Management System 

II. PROBLEM STATEMENT 

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

1. Meaningful real-world software quality problem 

Many universities in Vietnam currently have no centralised, reliable, and sustainable way for students to check availability and book study rooms across campus. This project aims to develop a Study Room Booking Application that allows students to search for available rooms, reserve them for a specific time, cancel a booking if needed, and easily manage their reservations, while allowing staff to oversee room usage — with a strong focus on software quality assurance, including validation, reliability, usability, efficiency, and maintainability. 

2. Background 

Universities in Vietnam are usually small and have a limited number of study rooms across their campuses, while demand for booking a study room is especially high during assignment and exam periods. During this time, many students seek a quiet space to study in groups or individually. When students want to book a room, they need to meet with a librarian and fill out manual sign-up sheets or make informal arrangements; the librarian then checks the room's availability manually on paper before notifying the student. This often leads to double-bookings, wasted trips to fully occupied rooms, and inefficiencies in the student study process. 

3. Core booking Features 

1. Search/browser available rooms by date and time, viewing capacity and building/floor 

2. Book a room for a specific date/time slot 

3. Receive a confirmation of the booking via email 

4. Cancel or modify an existing booking 

5. Prevent double-booking / overlapping time conflicts 

6. Admin can view/manage all bookings, override/cancel bookings 

7. Basic usage reporting like most-booked rooms, peak times  

8. Multi-language support 