using Enrollment.Api.Clients;
using Enrollment.Api.Data;
using Enrollment.Api.Messaging;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<EnrollmentDbContext>(o =>
    o.UseMySql(cs, new MySqlServerVersion(new Version(8, 0, 36))));

// Typed HttpClients: die Basis-Adressen kommen aus appsettings.json
builder.Services.AddHttpClient<IStudentClient, StudentClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:Student"]!));
builder.Services.AddHttpClient<ICourseClient, CourseClient>(c =>
    c.BaseAddress = new Uri(builder.Configuration["Services:Course"]!));

builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.UseCors();
app.MapControllers();
app.Run();

public partial class Program { }