# Booking Validation - Quick Reference Guide

## ✅ All 5 Rules Implemented

### Rule 1: Cannot Book Same Start & End Time
```
User tries: 14:00 → 14:00
Result: ❌ ERROR - "Start time and end time cannot be the same"

User tries: 14:00 → 15:00  
Result: ✅ OK - Duration = 1 hour
```

---

### Rule 2: Cannot Book in Past
```
Today: 2024-01-15, Current time: 14:30

User tries: 2024-01-15 @ 14:00 (past time)
Result: ❌ ERROR - "Cannot book in the past"

User tries: 2024-01-15 @ 15:00 (future time)
Result: ✅ OK - After current time

User tries: 2024-01-14 (yesterday)
Result: ❌ ERROR - "Cannot book in the past"
```

---

### Rule 3: Cannot Book >60 Days Ahead
```
Today: 2024-01-15

User tries: 2024-03-15 (Day 60 from today)
Result: ✅ OK - Exactly 60 days

User tries: 2024-03-16 (Day 61)
Result: ❌ ERROR - "Cannot book more than 60 days in advance"
```

---

### Rule 4: Some Rooms Staff Only
```
Room: "Advanced Lab" - AccessRule set: Staff Only

Staff tries to book: ✅ OK
Student tries to book: ❌ ERROR - "You do not have permission to book this room"
Admin tries to book: ✅ OK (Admins can book anything)
```

---

### Rule 5: Students Need Permission for Restricted Rooms
```
Room: "Research Lab" - AccessRule set: Student + Permission

Student WITH permission tries: ✅ OK
Student WITHOUT permission tries: ❌ ERROR - "No permission"
Staff tries: ✅ OK
Admin tries: ✅ OK
```

---

## Where Each Rule is Enforced

| Rule | Service Layer | UI Layer | Database Layer |
|------|---------------|----------|-----------------|
| Same time | ✅ ValidateBookingDateTime() | ✅ JS check | N/A |
| Past booking | ✅ ValidateBookingDateTime() | ✅ Date input min | N/A |
| >60 days | ✅ ValidateBookingDateTime() | ✅ Date input max | N/A |
| Staff only | ✅ HasAccessToRoom() | ✅ Check on load | ✅ AccessRules table |
| Permission | ✅ HasAccessToRoom() | ✅ Check on load | ✅ AccessRules table |

---

## Testing Each Rule

### Test 1: Same Start/End Time
```
Form:
- Date: 2024-03-01
- Start: 14:00
- End: 14:00
Action: Submit
Expected: Error message appears
```

### Test 2: Past Booking  
```
Form:
- Date: 2024-01-01 (last year)
- Start: 10:00
- End: 11:00
Action: Submit
Expected: Error message appears
```

### Test 3: 61 Days Advance
```
Form:
- Date: Today + 61 days
- Start: 14:00
- End: 15:00
Action: Try to click date input
Expected: Date input won't allow selection
```

### Test 4: Staff-Only Room
```
Login as: Student
Navigate: /Bookings/Create?roomId=5 (staff room)
Expected: Redirect with error "You do not have permission"
```

### Test 5: Student Permission
```
Setup: Create AccessRule - Room 6, Role Student, IsActive true
Login as: Student
Navigate: /Bookings/Create?roomId=6
Action: Try to book
Expected: Success ✅
```

---

## How to Configure Room Access

### In DataStore.cs (or via Admin UI):

```csharp
// Staff-only lab
_dataStore.AccessRules.Add(new AccessRule
{
	Id = 1,
	RoomId = 5,
	AllowedRole = UserRole.Staff,
	AccessLevel = AccessLevel.Full,
	IsActive = true,
	CreatedAt = DateTime.UtcNow
});

// Permitted students for research lab
_dataStore.AccessRules.Add(new AccessRule
{
	Id = 2,
	RoomId = 6,
	AllowedRole = UserRole.Student,
	AccessLevel = AccessLevel.WithPermission,
	IsActive = true,
	CreatedAt = DateTime.UtcNow
});
```

### Result:
- Room 5: Only Staff (and Admins) can book
- Room 6: Only Students with this rule + Staff + Admins can book
- Other rooms: Anyone can book (no restrictions)

---

## Error Messages Users See

### When trying to book at same time:
```
❌ Start and end times cannot be the same
```

### When trying to book in past:
```
❌ Cannot book in the past. Please select a future date and time
```

### When trying to book too far ahead:
```
❌ Cannot book more than 60 days in advance
```

### When trying to access restricted room:
```
❌ You do not have permission to book this room
(appears on page load, redirects to bookings list)
```

---

## Files Modified

1. **BookingService.cs** - Added ValidateBookingDateTime() and HasAccessToRoom()
2. **IBookingService.cs** - Added interface methods
3. **Create.cshtml.cs** - Added access checks and GetCurrentUser()
4. **Create.cshtml** - Added client-side validation and constraints

---

## Admin Control

To change validation rules, modify these in **BookingService.cs**:

```csharp
// Change 60 days to 90 days:
var maxAdvanceDate = now.AddDays(90); // was 60

// Add minimum booking duration (e.g., 30 min):
if (endTime.Subtract(startTime).TotalMinutes < 30)
	throw new InvalidOperationException("Minimum booking duration is 30 minutes");

// Add same-day booking restrictions:
if (bookingDate == DateTime.Now.Date)
	throw new InvalidOperationException("Cannot book same day");
```

---

## Quick Validation Summary

```
✅ Start ≠ End Time
✅ Not in Past  
✅ Within 60 Days
✅ User Has Access
✅ Room Available
   ↓
✅ BOOKING CREATED
```

---

## Support

**For Issues:**
- Check ErrorMessage on Create page
- Verify JavaScript console for JS errors
- Check BookingService logs for validation errors
- Verify user role and access rules in database

**For Customization:**
- Edit ValidateBookingDateTime() for time rules
- Edit HasAccessToRoom() for permission rules
- Edit Create.cshtml JS for UI validation changes
