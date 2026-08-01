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

III. PROBLEM DEFINITION AND APPROVAL  

1. Meaningful real-world software quality problem 

Many universities in Vietnam still rely heavily on manual processes to manage their study room reservations. Students who want to book a room for either individual or group study, discussions, meetings, or exam preparation are required to travel to the library or student service desk, ask about room availability, and then staff manually check it using paper schedules, whiteboards, or spreadsheets before confirming the booking. In some other cases, bookings are arranged through phone calls or messaging applications, resulting in inconsistent record keeping and overlapping bookings. 

Although this process seems acceptable during periods of low demand, it becomes significantly inefficient during assignment periods, mid-semester tests, and final examinations. Students are not able to book rooms easily and may spend a long time arranging a booking or even discover that their requested room has already been booked by another person. Staff must also spend a great amount of time checking availability, updating booking records, and resolving scheduling issues manually, which reduces productivity and increases the possibility of human error. 

To address these issues, this project proposes the development of a Study Room Booking and Management System. This system will provide a centralised platform where students and staff can search for available rooms, create reservations, update, modify, or cancel bookings, receive booking confirmations, and view their booking history. Administrative users will also be responsible for managing room information, access permissions, booking overrides, and room utilisation reports. 

2. Background and Motivation 

Study rooms play an important role in supporting collaborative learning, presentations, group discussions, tutorials, project meetings, workshops, and independent study. However, the current manual booking process in many universities remains inefficient. 

The motivation for this project is to create ​a centralised platform that​ improves the quality ​of the booking​ process by introducing an automated solution that reduces human error, booking conflicts, improves room visibility, minimises administrative workload, and provides a better user experience for both students and university staff. 

The system will also provide university management with valuable information regarding room utilisation, low and peak booking periods, frequently used facilities, and resource demand, supporting future planning and decision-making. 

3. Stakeholders and Users 

Stakeholder 

Responsibilities 

Expected Benefits 

Students 

Search for and book study rooms 

Book rooms online, save travelling time, view real-time availability, reduce booking conflicts 

Academic staff 

Reserve presentation, meeting rooms, laboratories and teaching spaces 

Easier recurring bookings and room management 

Administrative staff 

Manage bookings and room information 

Reduced manual workload and the possibility of human error 

University Management 

Monitor room usage and utilisation 

Better resource planning and decision-making 

 

4. Summary of Major Use Cases 

4.1 Student 

- Search available rooms 

- Book a room 

​​- Modify or edit​ a booking 

​​- Cancel​ a booking 

​​- View reservation history​ 

- Receive booking confirmation 

4.2 Academic Staff 

- Book specialised rooms 

- Create recurring bookings 

4.3 Administrator 

- Manage rooms 

- Manage bookings 

- Override bookings 

- Configure room access rules 

- Generate reports 

- Manage user roles 

4.4 For all users 

- Can choose their preferred languages 

5. Software Quality Issues 

The current manual booking process issues identified in this project are analysed using ​the ISO/IEC 25010 software​ product ​quality model, which​ provides internationally recognised quality characteristics for evaluating software systems. Analysing these quality characteristics in the development stages will support identifying potential risks, defining measurable quality requirements, and establishing a strong foundation for subsequent testing, validation, and continuous quality improvement throughout the software development lifecycle. 

Functional Suitability 

The current process does not satisfy users’ needs because room availability is checked manually. The increases of possibility of incorrect bookings, inconsistent booking records, and booking conflicts. The proposed system should solve these issues by correctly perfoming searching, booking, validation, cancellation, modification and reporting functions. 

Reliability 

Manual booking process increases the likelihood of duplicate bookings, overlapping bookings, and lost booking information. The proposed system should maintain accurate booking records and prevent any conflicting reservations through automatic validation. 

Usability 

Students need to physically visit the library or contact staff before knowing whether the room is available. The booking process is inconvenient, time-consuming, and difficult to follow. The proposed system should provide an intuitive interface that support users to search, modify, book, and cancel bookings when necessary. 

Performance Efficiency 

During peak periods such as assignment deadlines or final exams, many students may search for rooms simultaneously. The system should return search results and booking confirmation quickly to ensure a smooth user experience during peak demand. 

Security 

Different room types require different access permissions. For example, the design rooms should only be accessible to authorised students or staff. Administrative functions should also be protected through role-based access control. 

Maintainability 

Room information, booking policies, and access control rules may change over time. The system shall be designed using a modular architecture in order to support future changes without affecting existing functionality.  

 

6. Why this project t is suitable for Software Quality Assurance 

This project is highly suitable for apply Software Quality Assurance because it includes both functional and non-functional quality requirements.  

To be more detailed, from a functional requirement, the system includes searching rooms, booking rooms, updating, email notifications, validation rules, role-based access control, reporting, and administrative management. 

While from a quality perspective, this project allows the application of Software Quality Assurance activities including: 

Requirements quality analysis 

Requirements traceability 

Unit testing 

System testing 

Integration testing 

Acceptance testing 

Regression testing 

Defect management 

Quality metrics 

AI-assisted software development using GitHub Copilot 

This project also evaluates some software ​quality characteristics defined​ in ​ISO/IEC 25010, including​: 

Functional suitability 

Usability 

Reliability 

Performance efficiency 

Security 

Maintainability 

7. Project Feasibility 

This project is achievable within one semester long because it focuses on developing a prototype rather than a production-ready application. 

The system will also use sample data instead of connecting with any existing university databases. The proposed scope includes core booking functionality, user management, validation, reporting, and software quality assurance artefacts while remaining realistic for students. 