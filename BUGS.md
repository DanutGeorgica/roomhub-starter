# BUGS

The starter contains behaviors that contradict `ASSIGNMENT.md`. Write here every discrepancy you found.
Fill it in yourself, in your own words. There is no required number: what counts is that they are real.

| Which rule from ASSIGNMENT.md is violated (R1–R11) | Where the problem was in the starter (file, approximate line) | How you discovered it (test, reading, AI) | What you changed |
|---|---|---|---|
| _(fill in)_ | _(fill in)_ | _(fill in)_ | _(fill in)_ |
| R5 | `src/RoomHub.Api/Services/BookingService.cs`, around lines 8–42 | AI + manual reading of the booking creation that goes against R5 | moved the past-start validation into Domain/Booking.cs , currently CreateBookingHandler obtains the current time from TimeProvider and passes it to the domain |
| R6 | `src/RoomHub.Api/Services/BookingService.cs`, around lines 8–42 | AI | added the four-hour maximum in Booking.Create |
| R7 | `src/RoomHub.Api/Services/BookingService.cs`, around lines 26–29 and `src/RoomHub.Api/Repositories/BookingRepository.cs`, around lines 8–16 | AI | changed overlap detection to active-only half-open intervals, allowed touching bookings, and wrapped checking plus insertion in the unit-of-work transaction |
| R8 | `src/RoomHub.Api/Services/BookingService.cs`, around lines 26–29 and 56–60 | AI | added cancellation state validation in Booking.Cancel , repeated cancellation returns 409, missing bookings return 404, and cancelled bookings no longer block intervals |
| R10 | `src/RoomHub.Api/Services/BookingService.cs`, around lines 8–42 | AI | added the two-hour maximum for rooms with capacity greater than 10, and two hours remain valid |
| R11 | `src/RoomHub.Api/Services/BookingService.cs`, around lines 56–60 | AI | added the 60-minute cancellation cutoff in Booking.Cancel, using the current time supplied by TimeProvider |
