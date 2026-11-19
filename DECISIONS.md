# DECISIONS

Answer **yourself**, in your own words, in 3–5 sentences per question.
Refer to concrete files and classes from your own repo (for example `Features/Bookings/CreateBooking/...`).
An unanswered question gives 0 points.

## 1. A rule you moved
Pick one business rule (R1–R8). In which file did it live in the starter and in which file does it live now? Why there?

In the starter, the overlap rule lived in src/RoomHub.Api/Services/BookingService.cs. in the final project it lives in Booking.Create inside src/RoomHub.Api/Domain/Booking.cs. The domain now checks only active bookings and uses half-open interval logic, so bookings that touch are allowed. i put the rule in the domain because the booking entity should protect its own invariants regardless of which endpoint calls it.

## 2. Result instead of an exception
Why is `Result` a better choice than an exception for "the room is occupied"? What happens in your code when an `Error` is returned?

Result is better for “the room is occupied” because this is an expected business outcome, not an unexpected system failure. Booking.Create returns a failed Result<Booking> containing an Error with status 409. the endpoint sees the failed result and converts it to ProblemDetails.

## 3. Who knows about EF Core?
Which classes in your project know about EF Core and which do not? What do you gain and what do you pay with Repository + Unit of Work?

RoomHubDbContext, EfRoomRepository, EfBookingRepository, and EfUnitOfWork know about EF Core. The domain entities, repository interfaces, handlers, and endpoints do not depend directly on EF Core. Repository and Unit of Work improve separation and make the application layer easier to test, but they add interfaces, mapping code, and some indirection.

## 4. The flow of a request
Draw in text (ASCII arrows) the path of a `POST /api/rooms/{roomId}/bookings` request through your project, with the **real** names of files and classes, from the endpoint to the database.

POST /api/rooms/{roomId}/bookings
  -> Features/Bookings/CreateBooking/CreateBookingEndpoint.cs
  -> CreateBookingEndpoint
  -> CreateBookingHandler.Handle(CreateBookingCommand)
  -> IRoomRepository.Get
  -> IBookingRepository.ListActiveForRoom
  -> IBookingRepository.CountActiveForParticipantOnDate
  -> Booking.Create
  -> IBookingRepository.Add
  -> IUnitOfWork.ExecuteInTransaction
  -> EfUnitOfWork
  -> RoomHubDbContext
  -> SQLite roomhub.db
  -> Result<Booking>
  -> CreateBookingEndpoint returns 201 or ProblemDetails

## 5. What you rejected from what the AI gave you
Give a concrete example of code or an idea proposed by the AI that you corrected or rejected, and say why.

i rejected the first approach of loading all bookings and counting the participant’s bookings in application code. I porposed to change it to CountActiveForParticipantOnDate on IBookingRepository, implemented by EfBookingRepository. this keeps the query efficient, counts bookings across all rooms, and keeps the rule in the handler/domain flow instead of the endpoint. (i did ask it for alternative suggestions to come to this conclusion)

## 6. If you had 30 more minutes
What would you improve and what risk did you leave in the project?

I would add automated integration tests using a fake TimeProvider and a temporary SQLite database. I would especially test exact UTC-date boundaries, concurrent booking creation. Also, currently the HTTP verification is currently mostly manual which kinda sucks.
