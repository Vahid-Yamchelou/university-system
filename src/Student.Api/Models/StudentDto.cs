using System.ComponentModel.DataAnnotations;

namespace Student.Api.Models;

public record CreateStudentDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required, EmailAddress] string Email,
    [Required] string MatriculationNumber);