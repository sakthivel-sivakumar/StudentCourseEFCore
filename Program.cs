using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StudentCourseEFCore.Data;
using StudentCourseEFCore.Menus;
using StudentCourseEFCore.Repositories;
using StudentCourseEFCore.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Check connection string
var connectionString =
builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine(
    $"Connection string loaded: {!string.IsNullOrWhiteSpace(connectionString)}");



builder.Services.AddScoped<DepartmentRepository>();
builder.Services.AddScoped<StudentRepository>();
builder.Services.AddScoped<CourseRepository>();
builder.Services.AddScoped<EnrollmentRepository>();

builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<EnrollmentService>();

builder.Services.AddScoped<DepartmentMenu>();
builder.Services.AddScoped<StudentMenu>();
builder.Services.AddScoped<CourseMenu>();
builder.Services.AddScoped<EnrollmentMenu>();

using var host = builder.Build();

await host.StartAsync();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var departmentMenu = services.GetRequiredService<DepartmentMenu>();
var studentMenu = services.GetRequiredService<StudentMenu>();
var courseMenu = services.GetRequiredService<CourseMenu>();
var enrollmentMenu = services.GetRequiredService<EnrollmentMenu>();

while (true)
{
    Console.WriteLine("\n===== STUDENT COURSE MANAGEMENT =====");
    Console.WriteLine("1. Department Management");
    Console.WriteLine("2. Student Management");
    Console.WriteLine("3. Course Management");
    Console.WriteLine("4. Enrollment Management");
    Console.WriteLine("0. Exit");
    Console.Write("Enter your choice: ");

    if (!int.TryParse(Console.ReadLine(), out int choice))
    {
        Console.WriteLine("Invalid input.");
        continue;
    }

    switch (choice)
    {
        case 1:
            await departmentMenu.ShowAsync();
            break;

        case 2:
            await studentMenu.ShowAsync();
            break;

        case 3:
            await courseMenu.ShowAsync();
            break;

        case 4:
            await enrollmentMenu.ShowAsync();
            break;

        case 0:
            return;

        default:
            Console.WriteLine("Invalid choice.");
            break;
    }
}






//EntityState