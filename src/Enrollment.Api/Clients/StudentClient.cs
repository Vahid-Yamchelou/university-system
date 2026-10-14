using System.Net;

namespace Enrollment.Api.Clients;

public record StudentInfo(int Id, string FirstName, string LastName, string Email);

public interface IStudentClient
{
    Task<StudentInfo?> GetAsync(int id, CancellationToken ct);
}

public class StudentClient : IStudentClient
{
    private readonly HttpClient _http;
    public StudentClient(HttpClient http) => _http = http;

    public async Task<StudentInfo?> GetAsync(int id, CancellationToken ct)
    {
        var response = await _http.GetAsync($"api/students/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StudentInfo>(ct);
    }
}