# RoomHub Completion Plan

## Goal

Transform the starter from layered, in-memory services into a vertical-slice application that
keeps the existing HTTP contract unchanged and satisfies the business rules R1–R11.

The starter currently has:

- `Endpoints`, `Services`, `Repositories`, and `Models` folders instead of feature slices.
- Anemic models with public setters.
- Business validation and booking rules in `RoomService` and `BookingService`.
- `BusinessException` plus exception middleware for expected business outcomes.
- In-memory singleton repositories, so data is lost on restart.
- No `TimeProvider`, four-hour duration check, correct active-booking interval logic, or
  already-cancelled conflict handling.
- No participant daily booking-limit rule (R9).
- No capacity-dependent two-hour duration rule (R10).
- No cancellation cutoff rule (R11).

## Target structure

Use at least these five independently recognizable slices:

```text
src/RoomHub.Api/
  Features/
    Rooms/
      CreateRoom/
      GetRoom/
    Bookings/
      CreateBooking/
      GetBooking/
      CancelBooking/
  Domain/
    Error.cs
    Result.cs
    Repositories/
    UnitOfWork/
  Infrastructure/
    Persistence/
  Shared/
    CommandHandling/
```

The exact names may follow the project's conventions, but each operation should have a clear
feature folder. Each feature should own its request/response contracts, command/query, handler,
and endpoint. Endpoints should only bind requests, invoke a handler, and translate a result to
the required HTTP response. The final structure should not leave business operations in the
top-level `Endpoints`, `Services`, or `Models` folders; persistence implementations belong under
`Infrastructure`, while domain entities and repository/unit-of-work abstractions remain outside
that layer.

## Delivery order

### Phase 1 — Establish a compiling vertical-slice baseline

1. Add the feature folders and move or recreate the five operations:
   - create room;
   - get room;
   - create booking;
   - get booking;
   - cancel booking.
2. Keep the routes, request fields, response fields, status strings, `Location` headers, and
   status codes exactly as specified in `ASSIGNMENT.md`.
3. Keep the first implementation simple enough to compile before introducing persistence.
4. Remove or stop registering the old endpoint/service path only after the new routes compile.
5. Run `dotnet build`.
6. Make the first meaningful commit after the baseline compiles.
7. Treat the old top-level endpoint files as transitional only; retire them after the feature
   mappings are active.

### Phase 2 — Introduce the domain and Result/Error flow

1. Add a typed `Error` representation and `Result<T>`/success-result support.
2. Add `ICommandHandler` and command/query handlers for the feature operations as appropriate.
3. Replace expected business exceptions with returned errors:
   - validation: 400;
   - missing room or booking: 404;
   - occupied room, repeated cancellation, participant daily limit, or cancellation cutoff:
     409.
4. Keep unexpected exceptions visible; do not convert all exceptions into success-shaped results.
5. Make domain entities enforce invariants through constructors/factory methods and behavior
   methods rather than public setters.
6. Move room and booking entities into the domain layer and leave services/handlers responsible
   only for orchestration and dependency access.
7. Ensure the booking creation flow checks that the room exists before validating the booking
   request, as required by the contract.
8. Run focused build and HTTP checks for room creation, room lookup, booking creation, missing
   resources, and error status codes.
9. Make the second meaningful commit.

### Phase 3 — Implement every business rule in the domain

Implement and verify the rules in domain behavior, not in endpoints:

- R1: room name is non-empty after trimming; capacity is greater than zero.
- R2: `bookedBy` is required; trim it only after validation.
- R3: `endUtc` is strictly after `startUtc`.
- R4: attendees are positive and do not exceed room capacity.
- R5: compare the booking start with an injected `TimeProvider`; never use
  `DateTime.UtcNow` or `DateTime.Now`.
- R6: duration must be no greater than four hours; exactly four hours is valid.
- R7: compare only active bookings and use half-open interval logic:
  `existing.StartUtc < new.EndUtc && new.StartUtc < existing.EndUtc`.
- R8: cancellation changes active to cancelled, returns a conflict for a second cancellation,
  returns not-found for a missing booking, and excludes cancelled bookings from conflict checks.
- R9: count active bookings for the same `bookedBy` across all rooms using the UTC date of
  `startUtc`; reject the third booking on that date with 409, ignore cancelled bookings, and
  keep bookings on other UTC dates independent.
- R10: when room capacity is greater than 10, maximum duration is two hours and exactly two
  hours is valid; rooms with capacity 10 or less retain the four-hour maximum.
- R11: cancellation succeeds only when the booking starts at least 60 minutes from the current
  `TimeProvider` time; otherwise return 409.

Register `TimeProvider.System` in production and make it replaceable for tests. Pass the current
UTC value into domain behavior so the domain does not call wall-clock APIs directly. Keep all
stored and compared booking times in UTC.

### Phase 4 — Add EF Core, SQLite, repository interfaces, and Unit of Work

1. Add only the EF Core SQLite packages required by the project.
2. Define persistence-agnostic repository and unit-of-work interfaces in the domain/application
   side.
3. Implement those interfaces with EF Core in `Infrastructure`.
4. Add a `DbContext`, room and booking mappings, enum persistence, and the SQLite connection.
5. Register the context, repositories, unit of work, handlers, and `TimeProvider` in
   `Program.cs`.
6. Ensure creation, cancellation, and reads use the unit of work consistently.
7. Add a repository query such as `CountActiveForParticipantOnUtcDate` (the final name may follow
   the codebase convention) that counts active bookings across every room for one participant
   and UTC date.
8. Ensure the R7 overlap check, R9 count check, and booking insert are protected by one
   transaction or equivalent persistence strategy so concurrent requests cannot bypass either
   limit.
9. Confirm the database file is ignored and is not committed.
10. Run the application, create records, stop it, restart it, and verify records survive.
11. Make the third meaningful commit.

### Phase 5 — Contract and regression verification

Verify the exact contract with HTTP requests or focused automated tests:

1. Create and get a room: 201/200, camelCase body, and `Location` header.
2. Create and get a booking: 201/200, expected fields, enum text status, and `Location`.
3. Cancel a booking: 204 and status becomes `Cancelled`.
4. Missing room is 404 before invalid booking-field validation.
5. Missing booking is 404.
6. Invalid room name/capacity, missing `bookedBy`, invalid range, and invalid attendees are 400.
7. Past start is rejected using the configured `TimeProvider`.
8. More than four hours is rejected; exactly four hours succeeds.
9. Overlap between active bookings is 409.
10. Touching intervals succeed.
11. A cancelled booking no longer blocks its interval.
12. Cancelling an already cancelled booking is 409.
13. A participant can create two active bookings on one UTC start date across different rooms.
14. The third active booking for that participant and UTC date is 409.
15. A cancelled booking does not count toward the participant's daily limit.
16. Bookings on a different UTC start date do not count toward the limit.
17. The daily-limit check is independent of room-overlap checking and still applies across rooms.
18. A room with capacity 10 or less accepts exactly four hours.
19. A room with capacity greater than 10 accepts exactly two hours but rejects longer bookings
    with 400.
20. Cancellation at least 60 minutes before start succeeds.
21. Cancellation less than 60 minutes before start returns 409.
22. Restart persistence works.
23. `dotnet build` succeeds from a clean checkout.

Do not alter the documented routes or response shapes to make an internal design easier.

### Phase 6 — Required written deliverables and submission

1. Complete `DECISIONS.md` in personal wording, using the final feature and class names.
2. Complete `BUGS.md` in personal wording:
   - retain the verified starter discrepancies;
   - fill “What you changed” only after the corresponding fix is actually made;
   - remove template placeholders.
3. Append each real AI request and outcome to `AI-LOG.md`, with approximate elapsed time.
4. Check that `bin/`, `obj/`, and SQLite database files are not tracked.
5. Confirm at least three commits were made during the work.
6. Create or update the public repository and verify the submitted link before the quiz.
7. Stop coding before the quiz; the quiz must be completed without AI or a laptop.

## Suggested prompt sequence

Use these prompts one at a time. After each implementation prompt, inspect the diff and run the
requested validation before continuing.

### Prompt 1 — Baseline and vertical slices

> Re-read `ASSIGNMENT.md` and inspect the current RoomHub starter. Implement only the vertical
> slice baseline for the five required operations: CreateRoom, GetRoom, CreateBooking, GetBooking,
> and CancelBooking. Preserve every route, request field, response field, status code, enum text,
> and `Location` header from the API contract. Keep the project compiling, do not add EF Core yet,
> and report the files changed plus the exact build command and result. Do not modify
> `DECISIONS.md`, `BUGS.md`, or `AI-LOG.md` in this prompt.

### Prompt 2 — Result/Error and handlers

> Continue from the current RoomHub state. Add the shared `Result<T>` and `Error` model and
> `ICommandHandler`-style handlers, then make the endpoints thin translators of handler results.
> Replace expected business exceptions with explicit errors mapped to 400, 404, and 409.
> Preserve the API contract and ensure missing rooms are checked before booking validation.
> Do not add persistence yet. Build the project and list any remaining starter behavior.

### Prompt 3 — Rich domain rules

> Implement R1–R11 in rich domain classes rather than endpoints or application services. Entities
> must not expose public setters. Inject and use `TimeProvider`; never use `DateTime.UtcNow` or
> `DateTime.Now`. Enforce the four-hour maximum, active-only half-open interval overlap,
> touching intervals, repeated-cancellation conflict behavior, and R9's maximum of two active
> bookings per participant per UTC start date across all rooms. Add focused tests or HTTP checks
> for boundary cases, especially exactly four hours, touching bookings, two allowed daily
> bookings, the rejected third booking, cancelled-booking exclusion, a different UTC date,
> R10's capacity-dependent duration boundary, and R11's 60-minute cancellation boundary.
> Keep R9–R11 out of the endpoint and preserve the documented API contract. Build afterward.

### Prompt 4 — EF Core and SQLite

> Add EF Core with SQLite and migrate the current repository abstractions to persistence. Define
> repository and Unit of Work interfaces outside Infrastructure, implement them with a DbContext,
> register them in `Program.cs`, and ensure data survives process restart. Keep entities/domain
> code independent of EF-specific details where possible. Add a repository method for counting
> active bookings for a participant on a UTC start date across all rooms. Make both R7 overlap
> detection and R9 daily-limit detection plus insertion safe against concurrent requests in one
> transaction. Ensure R10 and R11 use persisted booking/room data but keep their decisions in
> the domain/handler flow. Do not commit database files. Build and perform a restart persistence
> check.

### Prompt 5 — Contract verification

> Test the complete RoomHub API against every operation and rule in `ASSIGNMENT.md`. Verify exact
> routes, JSON names, enum text, status codes, ProblemDetails responses, `Location` headers,
> missing-room validation order, TimeProvider behavior, duration boundaries, active overlap,
> touching intervals, cancellation, R9's cross-room daily limit and UTC-date boundary cases,
> R10's capacity-dependent duration limits, R11's cancellation cutoff, and restart persistence.
> Fix only issues introduced by the implementation, then run `dotnet build` and report the
> evidence.

### Prompt 6 — Documentation and release review

> Review the final implementation against `ASSIGNMENT.md`, `BUGS.md`, `DECISIONS.md`, and
> `AI-LOG.md`. Do not invent claims. Fill the documents in concise personal wording using the
> actual final file and class names, leave no template placeholders, confirm “What you changed”
> entries describe completed fixes, and verify no `bin/`, `obj/`, or `*.db` files are tracked.
> Check the commit count and public-repository submission readiness. Do not change the API
> contract.

## Completion checklist

- [x] `dotnet build` succeeds.
- [x] At least five feature slices exist under `Features/`.
- [x] Endpoints are thin and handlers implement the request flow.
- [x] Domain entities have no public setters.
- [x] Business errors use `Result/Error`, not business exceptions.
- [x] R1–R11 pass, including all boundary cases.
- [x] R9 counts active bookings across all rooms by `bookedBy` and UTC `startUtc` date.
- [x] R9 rejects the third booking with 409, ignores cancelled bookings, and does not mix dates.
- [x] R10 applies a four-hour limit at capacity 10 or less and a two-hour limit above 10.
- [x] R11 rejects cancellation inside the 60-minute window with 409 using `TimeProvider`.
- [x] `TimeProvider` is used and no `DateTime.UtcNow`/`DateTime.Now` remains.
- [x] EF Core + SQLite persists data across restart.
- [x] Repository and Unit of Work are represented by interfaces outside Infrastructure.
- [x] API contract and ProblemDetails status codes are unchanged.
- [ ] `DECISIONS.md`, `BUGS.md`, and `AI-LOG.md` remain to be completed manually.
- [ ] At least three commits exist (not verified in this environment).
- [ ] The repository is public and the link is ready before the quiz (not verified in this environment).
