using Microsoft.EntityFrameworkCore;

namespace Student.Api.Data;

public class StudentDbContext : DbContext
{
    public StudentDbContext(DbContextOptions<StudentDbContext> options) : base(options) { }

    public DbSet<Models.Student> Students => Set<Models.Student>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Models.Student>(e =>
        {
            e.HasIndex(s => s.MatriculationNumber).IsUnique();
            e.Property(s => s.Email).HasMaxLength(200);
        });
    }
}