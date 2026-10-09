using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student.Api.Controllers;
using Student.Api.Data;
using Student.Api.Models;

namespace Student.Tests;

public class StudentsControllerTests
{
    private static StudentDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<StudentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new StudentDbContext(options);
    }

    [Fact]
    public async Task Create_ReturnsCreated_AndSavesStudent()
    {
        using var db = CreateDb();
        var controller = new StudentsController(db);

        var result = await controller.Create(
            new CreateStudentDto("Ali", "Khan", "ali@uni.de", "M1001"));

        Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Single(db.Students);
    }

    [Fact]
    public async Task Get_UnknownId_ReturnsNotFound()
    {
        using var db = CreateDb();
        var controller = new StudentsController(db);

        var result = await controller.Get(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetAll_ReturnsAllStudents(int count)
    {
        using var db = CreateDb();
        for (int i = 0; i < count; i++)
            db.Students.Add(new Student.Api.Models.Student
            { FirstName = "A", LastName = "B", Email = $"{i}@x.de", MatriculationNumber = $"M{i}" });
        await db.SaveChangesAsync();

        var result = await new StudentsController(db).GetAll();

        Assert.Equal(count, result.Value!.Count());
    }
}