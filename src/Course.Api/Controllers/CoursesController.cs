using Course.Api.Data;
using Course.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Course.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly CourseDbContext _db;
    public CoursesController(CourseDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.Course>>> GetAll()
        => await _db.Courses.AsNoTracking().ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Models.Course>> Get(int id)
    {
        var c = await _db.Courses.FindAsync(id);
        return c is null ? NotFound() : c;
    }

    [HttpPost]
    public async Task<ActionResult<Models.Course>> Create(CreateCourseDto dto)
    {
        var c = new Models.Course
        {
            Code = dto.Code,
            Title = dto.Title,
            Credits = dto.Credits,
            Capacity = dto.Capacity
        };
        _db.Courses.Add(c);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = c.Id }, c);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateCourseDto dto)
    {
        var c = await _db.Courses.FindAsync(id);
        if (c is null) return NotFound();
        c.Code = dto.Code;
        c.Title = dto.Title;
        c.Credits = dto.Credits;
        c.Capacity = dto.Capacity;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Courses.FindAsync(id);
        if (c is null) return NotFound();
        _db.Courses.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}