using Microsoft.EntityFrameworkCore;

namespace Course.Api.Data;

public class CourseDbContext : DbContext
{
    public CourseDbContext(DbContextOptions<CourseDbContext> options) : base(options) { }

    public DbSet<Models.Course> Courses => Set<Models.Course>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Models.Course>(e =>
        {
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.Code).HasMaxLength(20);
            e.Property(c => c.Title).HasMaxLength(200);
        });
    }
}