using System.ComponentModel.DataAnnotations;

namespace Course.Api.Models;

public record CreateCourseDto(
    [Required] string Code,
    [Required] string Title,
    [Range(1, 30)] int Credits,
    [Range(1, 500)] int Capacity);