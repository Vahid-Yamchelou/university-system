using Enrollment.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Enrollment.Api.Data;

public class EnrollmentDbContext : DbContext
{
    public EnrollmentDbContext(DbContextOptions<EnrollmentDbContext> options) : base(options) { }

    public DbSet<CourseEnrollment> Enrollments => Set<CourseEnrollment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Ein Student kann sich pro Kurs nur einmal einschreiben
        b.Entity<CourseEnrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();
    }
}