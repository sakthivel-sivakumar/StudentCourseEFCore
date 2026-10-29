using StudentCourseEFCore.Models;
using StudentCourseEFCore.Repositories;

namespace StudentCourseEFCore.Services;

public class CourseService
{
    private readonly CourseRepository _repository;

    public CourseService(CourseRepository repository)
    {
        _repository = repository;
    }

    public Task AddAsync(Course course)
        => _repository.AddAsync(course);

    public Task<List<Course>> GetAllAsync()
        => _repository.GetAllAsync();

    public Task<Course?> GetByIdAsync(int id)
        => _repository.GetByIdAsync(id);

    public Task UpdateAsync(Course course)
        => _repository.UpdateAsync(course);

    public Task<bool> DeleteAsync(int id)
        => _repository.DeleteAsync(id);
}