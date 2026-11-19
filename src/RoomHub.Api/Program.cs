using RoomHub.Api.Features.Bookings.CancelBooking;
using RoomHub.Api.Features.Bookings.CreateBooking;
using RoomHub.Api.Features.Bookings.GetBooking;
using RoomHub.Api.Features.Rooms.CreateRoom;
using RoomHub.Api.Features.Rooms.GetRoom;
using RoomHub.Api.Repositories;
using RoomHub.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Starter organized in layers: Endpoints -> Services -> Repositories -> Models.
builder.Services.AddSingleton<RoomRepository>();
builder.Services.AddSingleton<BookingRepository>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<CreateRoomHandler>();
builder.Services.AddScoped<GetRoomHandler>();
builder.Services.AddScoped<CreateBookingHandler>();
builder.Services.AddScoped<GetBookingHandler>();
builder.Services.AddScoped<CancelBookingHandler>();

var app = builder.Build();

app.MapGet("/", () => "RoomHub API is running.");
app.MapCreateRoomEndpoint();
app.MapGetRoomEndpoint();
app.MapCreateBookingEndpoint();
app.MapGetBookingEndpoint();
app.MapCancelBookingEndpoint();

app.Run();
