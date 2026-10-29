
using Microsoft.EntityFrameworkCore;
using StudentCourseEFCore.Helpers;
using StudentCourseEFCore.Models;
using StudentCourseEFCore.Services;

namespace StudentCourseEFCore.Menus;

public class EnrollmentMenu
{
    private readonly EnrollmentService _enrollmentService;
    private readonly StudentService _studentService;
    private readonly CourseService _courseService;

    public EnrollmentMenu(
        EnrollmentService enrollmentService,
        StudentService studentService,
        CourseService courseService)
    {
        _enrollmentService = enrollmentService;
        _studentService = studentService;
        _courseService = courseService;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            Console.WriteLine("\n===== ENROLLMENT MANAGEMENT =====");
            Console.WriteLine("1. Enroll Student");
            Console.WriteLine("2. View All Enrollments");
            Console.WriteLine("3. View Enrollments By Student");
            Console.WriteLine("4. View Enrollment By ID");
            Console.WriteLine("5. Cancel Enrollment");
            Console.WriteLine("0. Back To Main Menu");

            Console.Write("Enter your choice: ");

            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Invalid input. Enter a number.");
                continue;
            }

            switch (choice)
            {
                case 1:
                    await AddAsync();
                    break;

                case 2:
                    await GetAllAsync();
                    break;

                case 3:
                    await GetByStudentIdAsync();
                    break;

                case 4:
                    await GetByIdAsync();
                    break;

                case 5:
                    await DeleteAsync();
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
    }

    private async Task AddAsync()
    {
        try
        {
            var students = await _studentService.GetAllAsync();
            var courses = await _courseService.GetAllAsync();

            if (students.Count == 0)
            {
                Console.WriteLine(
                    "No students available. Add a student first.");
                return;
            }

            if (courses.Count == 0)
            {
                Console.WriteLine(
                    "No courses available. Add a course first.");
                return;
            }

            Console.WriteLine("\n--- Available Students ---");

            foreach (var student in students)
            {
                Console.WriteLine(
                    $"ID: {student.StudentId} | Name: {student.Name}");
            }

            if (!ConsoleHelper.TryReadId(
                    "Enter Student ID: ", out int studentId))
            {
                Console.WriteLine("Invalid student ID.");
                return;
            }

            if (!students.Any(s => s.StudentId == studentId))
            {
                Console.WriteLine("Student not found.");
                return;
            }

            Console.WriteLine("\n--- Available Courses ---");

            foreach (var course in courses)
            {
                Console.WriteLine(
                    $"ID: {course.CourseId} | " +
                    $"Name: {course.CourseName} | " +
                    $"Credits: {course.Credits}");
            }

            if (!ConsoleHelper.TryReadId(
                    "Enter Course ID: ", out int courseId))
            {
                Console.WriteLine("Invalid course ID.");
                return;
            }

            if (!courses.Any(c => c.CourseId == courseId))
            {
                Console.WriteLine("Course not found.");
                return;
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId
            };

            int enrollmentId =
                await _enrollmentService.AddAsync(enrollment);

            Console.WriteLine(
                $"\nEnrollment successful! Enrollment ID: {enrollmentId}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Enrollment failed: {ex.Message}");
        }
        catch (DbUpdateException)
        {
            Console.WriteLine(
                "A database error occurred while creating the enrollment.");
        }
    }

    private async Task GetAllAsync()
    {
        var enrollments = await _enrollmentService.GetAllAsync();

        DisplayEnrollments(enrollments);
    }

    private async Task GetByStudentIdAsync()
    {
        if (!ConsoleHelper.TryReadId(
                "Enter Student ID: ", out int studentId))
        {
            Console.WriteLine("Invalid student ID.");
            return;
        }

        var enrollments =
            await _enrollmentService.GetByStudentIdAsync(studentId);

        DisplayEnrollments(enrollments);
    }

    private async Task GetByIdAsync()
    {
        if (!ConsoleHelper.TryReadId(
                "Enter Enrollment ID: ", out int enrollmentId))
        {
            Console.WriteLine("Invalid enrollment ID.");
            return;
        }

        var enrollment =
            await _enrollmentService.GetByIdAsync(enrollmentId);

        if (enrollment is null)
        {
            Console.WriteLine("Enrollment not found.");
            return;
        }

        DisplayEnrollment(enrollment);
    }

    private async Task DeleteAsync()
    {
        if (!ConsoleHelper.TryReadId(
                "Enter Enrollment ID to cancel: ", out int enrollmentId))
        {
            Console.WriteLine("Invalid enrollment ID.");
            return;
        }

        bool deleted =
            await _enrollmentService.DeleteAsync(enrollmentId);

        Console.WriteLine(
            deleted
                ? "Enrollment cancelled successfully."
                : "Enrollment not found.");
    }

    private static void DisplayEnrollments(
        List<Enrollment> enrollments)
    {
        if (enrollments.Count == 0)
        {
            Console.WriteLine("No enrollments found.");
            return;
        }

        Console.WriteLine("\n--- Enrollment Records ---");

        foreach (var enrollment in enrollments)
        {
            DisplayEnrollment(enrollment);
        }
    }

    private static void DisplayEnrollment(Enrollment enrollment)
    {
        Console.WriteLine("----------------------------------");
        Console.WriteLine($"Enrollment ID : {enrollment.EnrollmentId}");
        Console.WriteLine($"Student ID    : {enrollment.StudentId}");
        Console.WriteLine($"Student Name  : {enrollment.Student?.Name}");
        Console.WriteLine($"Course ID     : {enrollment.CourseId}");
        Console.WriteLine($"Course Name   : {enrollment.Course?.CourseName}");
        Console.WriteLine($"Enrolled At   : {enrollment.EnrolledAt}");
    }
}
