using Microsoft.EntityFrameworkCore;
using StudyRoomBooking.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add session configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add application services
builder.Services.AddScoped<StudyRoomBooking.Application.Services.IBookingService, StudyRoomBooking.Application.Services.BookingService>();
builder.Services.AddScoped<StudyRoomBooking.Application.Services.IRoomService, StudyRoomBooking.Application.Services.RoomService>();
builder.Services.AddScoped<StudyRoomBooking.Application.Services.IUserService, StudyRoomBooking.Application.Services.UserService>();
builder.Services.AddScoped<StudyRoomBooking.Application.Services.IAccessRuleService, StudyRoomBooking.Application.Services.AccessRuleService>();
builder.Services.AddScoped<StudyRoomBooking.Application.Services.IReportService, StudyRoomBooking.Application.Services.ReportService>();
builder.Services.AddScoped<StudyRoomBooking.Application.Services.INotificationService, StudyRoomBooking.Application.Services.NotificationService>();
builder.Services.AddScoped<StudyRoomBooking.Application.Services.IAuthenticationService, StudyRoomBooking.Application.Services.AuthenticationService>();

var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseAnonKey = builder.Configuration["Supabase:AnonKey"];
if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(supabaseAnonKey))
{
    throw new InvalidOperationException("Supabase:Url and Supabase:AnonKey are required for authentication.");
}

builder.Services.AddHttpClient();
builder.Services.AddScoped<StudyRoomBooking.Application.Services.SupabaseAuthClient>(serviceProvider =>
    new StudyRoomBooking.Application.Services.SupabaseAuthClient(
        serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(),
        supabaseUrl,
        supabaseAnonKey));

var supabaseConnectionString = builder.Configuration.GetConnectionString("Supabase")
    ?? builder.Configuration["SUPABASE_CONNECTION_STRING"];
if (string.IsNullOrWhiteSpace(supabaseConnectionString))
{
    throw new InvalidOperationException("ConnectionStrings:Supabase is required. Configure it with dotnet user-secrets or the SUPABASE_CONNECTION_STRING environment variable.");
}

builder.Services.AddDbContext<StudyRoomBookingDbContext>(options => options.UseNpgsql(supabaseConnectionString));
builder.Services.AddScoped<StudyRoomBooking.Domain.Interfaces.IUnitOfWork, StudyRoomBooking.Infrastructure.Repositories.EfUnitOfWork>();
builder.Services.AddSingleton<StudyRoomBooking.Infrastructure.Localization.ILocalizationService, StudyRoomBooking.Infrastructure.Localization.LocalizationService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StudyRoomBookingDbContext>();
    db.Database.EnsureCreated();
}

// Initialize seed data
InitializeSeedData(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware BEFORE MapRazorPages
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

// Seed initial data
void InitializeSeedData(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<StudyRoomBooking.Application.Services.IUserService>();
    var roomService = scope.ServiceProvider.GetRequiredService<StudyRoomBooking.Application.Services.IRoomService>();

    if (userService.GetAllUsersAsync().Result.Count > 0 || roomService.GetAllRoomsAsync().Result.Count > 0)
    {
        return;
    }

    // Seed users
    var student1 = new StudyRoomBooking.Domain.Entities.User
    {
        UserId = "student1@university.edu",
        Name = "Nguyễn Văn A",
        Email = "student1@university.edu",
        Role = StudyRoomBooking.Domain.Enums.UserRole.Student,
        Major = StudyRoomBooking.Domain.Enums.StudentMajor.Engineering
    };

    var student2 = new StudyRoomBooking.Domain.Entities.User
    {
        UserId = "student2@university.edu",
        Name = "Trần Thị B",
        Email = "student2@university.edu",
        Role = StudyRoomBooking.Domain.Enums.UserRole.Student,
        Major = StudyRoomBooking.Domain.Enums.StudentMajor.Business
    };

    var student3 = new StudyRoomBooking.Domain.Entities.User
    {
        UserId = "student3@university.edu",
        Name = "Lê Văn C",
        Email = "student3@university.edu",
        Role = StudyRoomBooking.Domain.Enums.UserRole.Student,
        Major = StudyRoomBooking.Domain.Enums.StudentMajor.Science
    };

    var staff = new StudyRoomBooking.Domain.Entities.User
    {
        UserId = "staff@university.edu",
        Name = "Phạm Quốc C",
        Email = "staff@university.edu",
        Role = StudyRoomBooking.Domain.Enums.UserRole.Staff
    };

    var admin = new StudyRoomBooking.Domain.Entities.User
    {
        UserId = "admin@university.edu",
        Name = "Administrator",
        Email = "admin@university.edu",
        Role = StudyRoomBooking.Domain.Enums.UserRole.Admin
    };

    // Add users
    try
    {
        userService.CreateUserAsync(student1).Wait();
        userService.CreateUserAsync(student2).Wait();
        userService.CreateUserAsync(student3).Wait();
        userService.CreateUserAsync(staff).Wait();
        userService.CreateUserAsync(admin).Wait();
    }
    catch { /* Users might already exist */ }

    // Seed rooms
    var rooms = new List<StudyRoomBooking.Domain.Entities.Room>
    {
        new()
        {
            Code = "SR001",
            Name = "Study Room 1",
            Location = "Building A, Floor 1",
            Capacity = 4,
            Type = StudyRoomBooking.Domain.Enums.RoomType.Study,
            Description = "Small study room for group work - Open to all majors"
        },
        new()
        {
            Code = "SR002",
            Name = "Study Room 2",
            Location = "Building A, Floor 2",
            Capacity = 6,
            Type = StudyRoomBooking.Domain.Enums.RoomType.Study,
            Description = "Medium study room - Open to all majors"
        },
        new()
        {
            Code = "LB001",
            Name = "Computer Lab 1",
            Location = "Building B, Floor 1",
            Capacity = 30,
            Type = StudyRoomBooking.Domain.Enums.RoomType.ComputerLab,
            Description = "Lab for programming and IT courses - Engineering & Science majors only"
        },
        new()
        {
            Code = "MB001",
            Name = "Meeting Room 1",
            Location = "Building C, Floor 1",
            Capacity = 10,
            Type = StudyRoomBooking.Domain.Enums.RoomType.Meeting,
            Description = "Conference room for meetings - Open to all majors"
        },
        new()
        {
            Code = "SM001",
            Name = "Seminar Room",
            Location = "Building C, Floor 2",
            Capacity = 20,
            Type = StudyRoomBooking.Domain.Enums.RoomType.Seminar,
            Description = "Seminar and workshop room - Open to all majors"
        },
        new()
        {
            Code = "DS001",
            Name = "Design Studio",
            Location = "Building D, Floor 1",
            Capacity = 12,
            Type = StudyRoomBooking.Domain.Enums.RoomType.DesignStudio,
            Description = "Design and creative workspace - Business major only"
        },
        new()
        {
            Code = "EL001",
            Name = "Engineering Lab",
            Location = "Building E, Floor 1",
            Capacity = 25,
            Type = StudyRoomBooking.Domain.Enums.RoomType.EngineeringLab,
            Description = "Advanced engineering lab - Engineering major only"
        }
    };

    try
    {
        foreach (var room in rooms)
        {
            roomService.CreateRoomAsync(room).Wait();
        }
    }
    catch { /* Rooms might already exist */ }

    // Seed room major restrictions
    var accessRuleService = scope.ServiceProvider.GetRequiredService<StudyRoomBooking.Application.Services.IAccessRuleService>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<StudyRoomBooking.Domain.Interfaces.IUnitOfWork>();

    try
    {
        // Get the created rooms to add restrictions
        var allRooms = roomService.GetAllRoomsAsync().Result;
        var computerLab = allRooms?.FirstOrDefault(r => r.Code == "LB001");
        var designStudio = allRooms?.FirstOrDefault(r => r.Code == "DS001");
        var engineeringLab = allRooms?.FirstOrDefault(r => r.Code == "EL001");

        if (computerLab != null)
        {
            // Computer Lab: Engineering and Science majors only
            unitOfWork.RoomMajorRestrictions.AddAsync(new StudyRoomBooking.Domain.Entities.RoomMajorRestriction
            {
                RoomId = computerLab.Id,
                Major = StudyRoomBooking.Domain.Enums.StudentMajor.Engineering
            }).Wait();
            unitOfWork.RoomMajorRestrictions.AddAsync(new StudyRoomBooking.Domain.Entities.RoomMajorRestriction
            {
                RoomId = computerLab.Id,
                Major = StudyRoomBooking.Domain.Enums.StudentMajor.Science
            }).Wait();
        }

        if (designStudio != null)
        {
            // Design Studio: Business majors only
            unitOfWork.RoomMajorRestrictions.AddAsync(new StudyRoomBooking.Domain.Entities.RoomMajorRestriction
            {
                RoomId = designStudio.Id,
                Major = StudyRoomBooking.Domain.Enums.StudentMajor.Business
            }).Wait();
        }

        if (engineeringLab != null)
        {
            // Engineering Lab: Engineering majors only
            unitOfWork.RoomMajorRestrictions.AddAsync(new StudyRoomBooking.Domain.Entities.RoomMajorRestriction
            {
                RoomId = engineeringLab.Id,
                Major = StudyRoomBooking.Domain.Enums.StudentMajor.Engineering
            }).Wait();
        }

        unitOfWork.SaveChangesAsync().Wait();
    }
    catch { /* Restrictions might already exist */ }
}
