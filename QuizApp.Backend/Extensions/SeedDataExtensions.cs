
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizApp.Backend.Data;
using QuizApp.Backend.Users;


namespace QuizApp.Backend.Extensions
{
    public static class SeedDataExtensions
    {
        public static async Task SeedAdminUserAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<QuizAppDBContext>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var passwordHasher = new PasswordHasher<User>();

            // Check if admin already exists
            if (await dbContext.Users.AnyAsync(u => u.Role == UserRole.Admin))
                return;

            var admin = new User
            {
                Email = "admin@quizapp.com",
                Role = UserRole.Admin,
                PasswordHash = passwordHasher.HashPassword(null, "test101")
            };

            await userService.CreateUser(admin);
        }
    }

}