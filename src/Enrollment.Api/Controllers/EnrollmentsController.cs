using Enrollment.Api.Clients;
using Enrollment.Api.Data;
using Enrollment.Api.Messaging;
using Enrollment.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Enrollment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly EnrollmentDbContext _db;
    private readonly IStudentClient _students;
    private readonly ICourseClient _courses;
    private readonly IEventPublisher _publisher;

    public EnrollmentsController(EnrollmentDbContext db, IStudentClient students,
        ICourseClient courses, IEventPublisher publisher)
    {
        _db = db;
        _students = students;
        _courses = courses;
        _publisher = publisher;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseEnrollment>>> GetAll(
        [FromQuery] int? studentId, [FromQuery] int? courseId)
    {
        var query = _db.Enrollments.AsNoTracking();
        if (studentId is not null) query = query.Where(e => e.StudentId == studentId);
        if (courseId is not null) query = query.Where(e => e.CourseId == courseId);
        return await query.ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CourseEnrollment>> Get(int id)
    {
        var e = await _db.Enrollments.FindAsync(id);
        return e is null ? NotFound() : e;
    }

    [HttpPost]
    public async Task<ActionResult<CourseEnrollment>> Create(
        CreateEnrollmentDto dto, CancellationToken ct)
    {
        StudentInfo? student;
        CourseInfo? course;
        try
        {
            student = await _students.GetAsync(dto.StudentId, ct);
            course = await _courses.GetAsync(dto.CourseId, ct);
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "Student- oder Course-Service nicht erreichbar.");
        }

        if (student is null) return NotFound($"Student {dto.StudentId} nicht gefunden.");
        if (course is null) return NotFound($"Kurs {dto.CourseId} nicht gefunden.");

        if (await _db.Enrollments.AnyAsync(
                e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId, ct))
            return Conflict("Student ist bereits in diesem Kurs eingeschrieben.");

        var taken = await _db.Enrollments.CountAsync(e => e.CourseId == dto.CourseId, ct);
        if (taken >= course.Capacity)
            return Conflict("Der Kurs ist voll.");

        var enrollment = new CourseEnrollment
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId
        };
        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync(ct);

        await _publisher.PublishAsync("enrollment.created", new
        {
            enrollment.Id,
            enrollment.StudentId,
            enrollment.CourseId,
            StudentEmail = student.Email,
            CourseTitle = course.Title
        });

        return CreatedAtAction(nameof(Get), new { id = enrollment.Id }, enrollment);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await _db.Enrollments.FindAsync(id);
        if (e is null) return NotFound();
        _db.Enrollments.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}