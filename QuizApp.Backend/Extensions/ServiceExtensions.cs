using QuizApp.Backend.Interfaces;
using QuizApp.Backend.Models;
using QuizApp.Backend.Repositories;
using QuizApp.Backend.Services;
using Microsoft.AspNetCore.Identity;

namespace QuizApp.Backend.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRefreshTokenService,RefreshTokenService>();
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            return services;
        }
    }
}