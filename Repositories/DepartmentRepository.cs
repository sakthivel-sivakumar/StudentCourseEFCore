
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentCourseEFCore.Data;
using StudentCourseEFCore.Models;

namespace StudentCourseEFCore.Repositories;

public class DepartmentRepository
{
    private readonly AppDbContext db;

    public DepartmentRepository(AppDbContext context)
    {
        db = context;
    }

    public async Task<int> AddAsync(Department department)
    {
        db.Departments.Add(department);

        try
        {
            await db.SaveChangesAsync();
            return department.DepartmentId;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx
                  && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            throw new InvalidOperationException(
                "A department with this name already exists.", ex);
        }
    }

    public async Task<List<Department>> GetAllAsync()
    {
        return await db.Departments
            .OrderBy(d => d.DepartmentId)
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await db.Departments.FindAsync(id);
    }

    public async Task<bool> UpdateAsync(
    int id,
    string departmentName,
    string? location)
    {
        var department = await db.Departments.FindAsync(id);

        if (department is null)
            return false;

        department.DepartmentName = departmentName;
        department.Location = location;

        try
        {
            await db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlEx
                  && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            throw new InvalidOperationException(
                "A department with this name already exists.", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await db.Departments.FindAsync(id);

        if (department is null)
            return false;

        db.Departments.Remove(department);

        await db.SaveChangesAsync();

        return true;
    }
}
