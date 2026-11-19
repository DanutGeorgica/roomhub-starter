using RoomHub.Api.Features.Bookings.CancelBooking;
using RoomHub.Api.Features.Bookings.CreateBooking;
using RoomHub.Api.Features.Bookings.GetBooking;
using RoomHub.Api.Features.Rooms.CreateRoom;
using RoomHub.Api.Features.Rooms.GetRoom;
using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain.Repositories;
using RoomHub.Api.Domain.UnitOfWork;
using RoomHub.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RoomHubDbContext>(options =>
    options.UseSqlite("Data Source=roomhub.db"));
builder.Services.AddScoped<IRoomRepository, EfRoomRepository>();
builder.Services.AddScoped<IBookingRepository, EfBookingRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddScoped<CreateRoomHandler>();
builder.Services.AddScoped<GetRoomHandler>();
builder.Services.AddScoped<CreateBookingHandler>();
builder.Services.AddScoped<GetBookingHandler>();
builder.Services.AddScoped<CancelBookingHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RoomHubDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "RoomHub API is running.");
app.MapCreateRoomEndpoint();
app.MapGetRoomEndpoint();
app.MapCreateBookingEndpoint();
app.MapGetBookingEndpoint();
app.MapCancelBookingEndpoint();

app.Run();
