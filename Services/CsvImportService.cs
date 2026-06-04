using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Lyceum.Models;
using Microsoft.AspNetCore.Identity;

namespace Lyceum.Services;

public class CsvImportService(LyceumDbContext context, UserManager<User> userManager)
{
    public async Task<CsvImportResult> ImportStudentsAsync(Stream csvStream)
    {
        var result = new CsvImportResult();
        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null
        });

        var records = csv.GetRecords<StudentCsvRow>().ToList();

        foreach (var row in records)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(row.FullName) || string.IsNullOrWhiteSpace(row.Email))
                {
                    result.Errors.Add($"Row {result.SuccessCount + result.Errors.Count + 1}: FullName and Email are required.");
                    continue;
                }

                var username = row.Email.Split('@')[0].ToLower().Replace(".", "");
                var existingUser = await userManager.FindByEmailAsync(row.Email);
                if (existingUser != null)
                {
                    result.Errors.Add($"Row {result.SuccessCount + result.Errors.Count + 1}: Email {row.Email} already exists.");
                    continue;
                }

                var user = new User
                {
                    UserName = username,
                    FullName = row.FullName,
                    Email = row.Email,
                    Role = UserRole.Student,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(user, row.Password ?? "Student@123");
                if (!createResult.Succeeded)
                {
                    result.Errors.Add($"Row {result.SuccessCount + result.Errors.Count + 1}: {createResult.Errors.First().Description}");
                    continue;
                }

                var randomSuffix = new Random().Next(1000, 9999);
                var student = new Student
                {
                    UserId = user.Id,
                    StudentCode = $"STU-{DateTime.UtcNow.Year}-{randomSuffix}",
                    Gender = row.Gender,
                    Phone = row.Phone,
                    Address = row.Address,
                    EnrollmentDate = DateTime.UtcNow
                };

                if (DateTime.TryParse(row.DateOfBirth, out var dob))
                    student.DateOfBirth = dob;

                context.Students.Add(student);
                await context.SaveChangesAsync();
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {result.SuccessCount + result.Errors.Count + 1}: {ex.Message}");
            }
        }

        return result;
    }
}

public class StudentCsvRow
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Password { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}

public class CsvImportResult
{
    public int SuccessCount { get; set; }
    public List<string> Errors { get; set; } = [];
}
