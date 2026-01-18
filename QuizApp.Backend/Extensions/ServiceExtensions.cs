using com.QuizAppBackend.Interfaces;
using com.QuizAppBackend.Models;
using com.QuizAppBackend.Repositories;
using com.QuizAppBackend.Services;
using Microsoft.AspNetCore.Identity;

namespace com.QuizAppBackend.Extensions
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