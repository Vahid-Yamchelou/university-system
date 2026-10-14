namespace Enrollment.Api.Models;

public class CourseEnrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}

public record CreateEnrollmentDto(int StudentId, int CourseId);