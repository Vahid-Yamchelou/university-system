using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student.Api.Data;
using Student.Api.Models;

namespace Student.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly StudentDbContext _db;
    public StudentsController(StudentDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Models.Student>>> GetAll()
        => await _db.Students.AsNoTracking().ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Models.Student>> Get(int id)
    {
        var s = await _db.Students.FindAsync(id);
        return s is null ? NotFound() : s;
    }

    [HttpPost]
    public async Task<ActionResult<Models.Student>> Create(CreateStudentDto dto)
    {
        var s = new Models.Student
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            MatriculationNumber = dto.MatriculationNumber
        };
        _db.Students.Add(s);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = s.Id }, s);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateStudentDto dto)
    {
        var s = await _db.Students.FindAsync(id);
        if (s is null) return NotFound();
        s.FirstName = dto.FirstName;
        s.LastName = dto.LastName;
        s.Email = dto.Email;
        s.MatriculationNumber = dto.MatriculationNumber;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Students.FindAsync(id);
        if (s is null) return NotFound();
        _db.Students.Remove(s);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}