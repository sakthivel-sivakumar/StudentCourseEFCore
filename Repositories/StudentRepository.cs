
using Microsoft.EntityFrameworkCore;
using StudentCourseEFCore.Data;
using StudentCourseEFCore.Models;

namespace StudentCourseEFCore.Repositories;

public class StudentRepository
{
    private readonly AppDbContext db;

    public StudentRepository(AppDbContext context)
    {
        db = context;
    }


    // 1. Add Student
    public async Task<int> AddAsync(Student student)
    {
        db.Students.Add(student);

        await db.SaveChangesAsync();

        return student.StudentId;
    }

    // 2. Get All Students
    public async Task<List<Student>> GetAllAsync()
    {
        return await db.Students
            .Include(s => s.Department)
            .OrderBy(s => s.StudentId)
            .ToListAsync();
    }

    // 3. Get Student By ID
    public async Task<Student?> GetByIdAsync(int id)
    {
        return await db.Students
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.StudentId == id);
    }

    // 4. Update Student
    public async Task<bool> UpdateAsync(
        int id,
        string name,
        string email,
        int age,
        int departmentId)
    {
        var student = await db.Students.FindAsync(id);

        if (student is null)
        {
            return false;
        }

        // Verify that the selected department exists.
        var departmentExists = await db.Departments
            .AnyAsync(d => d.DepartmentId == departmentId);

        if (!departmentExists)
        {
            throw new InvalidOperationException(
                "The specified department does not exist.");
        }

        student.Name = name;
        student.Email = email;
        student.Age = age;
        student.DepartmentId = departmentId;

        await db.SaveChangesAsync();

        return true;
    }

    // 5. Delete Student
    public async Task<bool> DeleteAsync(int id)
    {
        var student = await db.Students.FindAsync(id);

        if (student is null)
        {
            return false;
        }

        db.Students.Remove(student);

        await db.SaveChangesAsync();

        return true;
    }
}
