#!/usr/bin/env bash
trap 'kill 0' EXIT
dotnet run --project src/Student.Api --urls http://localhost:5001 &
dotnet run --project src/Course.Api --urls http://localhost:5002 &
dotnet run --project src/Enrollment.Api --urls http://localhost:5003 &
dotnet run --project src/Notification.Worker &
wait