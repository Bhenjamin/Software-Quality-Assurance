using StudyRoomBooking.Application.Interfaces;
using StudyRoomBooking.Application.Services;
using StudyRoomBooking.Infrastructure.Repositories;
using StudyRoomBooking.Infrastructure.SeedData;
using StudyRoomBooking.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
});
builder.Services.AddHttpContextAccessor();

// Repositories are registered as singletons so the sample data seeded at
// startup is shared across every request — this is a prototype using
// in-memory data, not a real multi-user backing store.
builder.Services.AddSingleton<IRoomRepository, InMemoryRoomRepository>();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();

// Application services are stateless wrappers around the repositories
// above, so they can be scoped per request.
builder.Services.AddScoped<IAccessControlService, AccessControlService>();
builder.Services.AddScoped<IRoomSearchService, RoomSearchService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// UI-layer helper only (session plumbing) — contains no business rules.
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

var app = builder.Build();

// Seed sample data once at startup.
using (var scope = app.Services.CreateScope())
{
    var roomRepository = scope.ServiceProvider.GetRequiredService<IRoomRepository>();
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    SampleDataSeeder.Seed(roomRepository, userRepository);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapRazorPages();

app.Run();
