using Enrollment.Api.Clients;
using Enrollment.Api.Controllers;
using Enrollment.Api.Data;
using Enrollment.Api.Messaging;
using Enrollment.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Enrollment.Tests;

public class EnrollmentsControllerTests
{
    // --- Fakes -------------------------------------------------------------
    // Bekannte Studenten: 1 und 2. Bekannter Kurs: 10.
    private class FakeStudentClient : IStudentClient
    {
        public Task<StudentInfo?> GetAsync(int id, CancellationToken ct) =>
            Task.FromResult<StudentInfo?>(
                id is 1 or 2 ? new StudentInfo(id, "A", "B", $"{id}@uni.de") : null);
    }

    private class FakeCourseClient : ICourseClient
    {
        private readonly int _capacity;
        public FakeCourseClient(int capacity) => _capacity = capacity;

        public Task<CourseInfo?> GetAsync(int id, CancellationToken ct) =>
            Task.FromResult<CourseInfo?>(
                id == 10 ? new CourseInfo(10, "CS101", "Informatik", _capacity) : null);
    }

    private class FakePublisher : IEventPublisher
    {
        public List<(string Queue, object Message)> Sent = new();
        public Task PublishAsync<T>(string queue, T message)
        {
            Sent.Add((queue, message!));
            return Task.CompletedTask;
        }
    }

    // --- Hilfsmethoden ------------------------------------------------------
    private static EnrollmentDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<EnrollmentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static EnrollmentsController Create(
        EnrollmentDbContext db, FakePublisher publisher, int capacity = 5) =>
        new(db, new FakeStudentClient(), new FakeCourseClient(capacity), publisher);

    // --- Tests --------------------------------------------------------------
    [Fact]
    public async Task Create_ValidRequest_SavesAndPublishesEvent()
    {
        using var db = CreateDb();
        var publisher = new FakePublisher();

        var result = await Create(db, publisher)
            .Create(new CreateEnrollmentDto(1, 10), CancellationToken.None);

        Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Single(db.Enrollments);
        Assert.Single(publisher.Sent);
        Assert.Equal("enrollment.created", publisher.Sent[0].Queue);
    }

    [Fact]
    public async Task Create_UnknownStudent_ReturnsNotFound_AndSavesNothing()
    {
        using var db = CreateDb();
        var publisher = new FakePublisher();

        var result = await Create(db, publisher)
            .Create(new CreateEnrollmentDto(99, 10), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Empty(db.Enrollments);
        Assert.Empty(publisher.Sent);
    }

    [Fact]
    public async Task Create_UnknownCourse_ReturnsNotFound()
    {
        using var db = CreateDb();

        var result = await Create(db, new FakePublisher())
            .Create(new CreateEnrollmentDto(1, 99), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_DuplicateEnrollment_ReturnsConflict()
    {
        using var db = CreateDb();
        var controller = Create(db, new FakePublisher());

        await controller.Create(new CreateEnrollmentDto(1, 10), CancellationToken.None);
        var second = await controller.Create(new CreateEnrollmentDto(1, 10), CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(second.Result);
        Assert.Single(db.Enrollments);
    }

    [Fact]
    public async Task Create_CourseFull_ReturnsConflict()
    {
        using var db = CreateDb();
        var controller = Create(db, new FakePublisher(), capacity: 1);

        await controller.Create(new CreateEnrollmentDto(1, 10), CancellationToken.None);
        var second = await controller.Create(new CreateEnrollmentDto(2, 10), CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(second.Result);
        Assert.Single(db.Enrollments);
    }
}