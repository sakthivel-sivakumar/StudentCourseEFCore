using Microsoft.EntityFrameworkCore;
using StudentCourseEFCore.Models;

namespace StudentCourseEFCore.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");

            entity.HasKey(d => d.DepartmentId);

            entity.Property(d => d.DepartmentName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(d => d.Location)
                .HasMaxLength(100);

            entity.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasIndex(d => d.DepartmentName)
                .IsUnique();
        });

        // Student configuration
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students", table =>
            {
                table.HasCheckConstraint(
                    "CK_Students_Age",
                    "[Age] BETWEEN 17 AND 60");
            });

            entity.HasKey(s => s.StudentId);

            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(s => s.Age)
                .IsRequired();

            entity.HasIndex(s => s.Email)
                .IsUnique();

            entity.HasOne(s => s.Department)
                .WithMany(d => d.Students)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Course configuration
        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses", table =>
            {
                table.HasCheckConstraint(
                    "CK_Courses_Credits",
                    "[Credits] BETWEEN 1 AND 6");
            });

            entity.HasKey(c => c.CourseId);

            entity.Property(c => c.CourseName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Credits)
                .IsRequired();

            entity.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasIndex(c => c.CourseName)
                .IsUnique();
        });

        // Enrollment configuration
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollments");

            entity.HasKey(e => e.EnrollmentId);

            entity.Property(e => e.EnrollmentId)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.EnrolledAt)
                .IsRequired()
                .HasDefaultValueSql("SYSDATETIME()");

            // Enrollment -> Student
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enrollment -> Course
            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // No unique index on StudentId + CourseId.
            // A student may enroll in the same course multiple times.
        });
    }
}