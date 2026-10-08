using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Student.Api.Data;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("Default")!;
builder.Services.AddDbContext<StudentDbContext>(o =>
    o.UseMySql(cs, new MySqlServerVersion(new Version(8, 0, 36))));

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

public partial class Program { }   // wird für Integrationstests gebraucht