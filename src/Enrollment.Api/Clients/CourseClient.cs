using System.Net;

namespace Enrollment.Api.Clients;

public record CourseInfo(int Id, string Code, string Title, int Capacity);

public interface ICourseClient
{
    Task<CourseInfo?> GetAsync(int id, CancellationToken ct);
}

public class CourseClient : ICourseClient
{
    private readonly HttpClient _http;
    public CourseClient(HttpClient http) => _http = http;

    public async Task<CourseInfo?> GetAsync(int id, CancellationToken ct)
    {
        var response = await _http.GetAsync($"api/courses/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CourseInfo>(ct);
    }
}