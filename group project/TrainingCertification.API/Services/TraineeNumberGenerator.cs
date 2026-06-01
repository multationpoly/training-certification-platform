using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrainingCertification.API.Models;

namespace TrainingCertification.API.Services;

public static class TraineeNumberGenerator
{
    private const string Prefix = "TR";
    private const int MaxAttempts = 10;

    public static async Task<string> GenerateUniqueAsync(UserManager<ApplicationUser> userManager)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var traineeNumber = Generate();
            if (!await userManager.Users.AnyAsync(user => user.TraineeNumber == traineeNumber))
            {
                return traineeNumber;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique trainee number.");
    }

    private static string Generate()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var suffix = RandomNumberGenerator.GetInt32(0, 10000).ToString("D4");
        return $"{Prefix}-{timestamp}-{suffix}";
    }
}
