using QuizApp.Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace QuizApp.Backend.Extensions
{
    public static class DataBaseExtension
    {
      public static IServiceCollection AddDatabase(this IServiceCollection services,
                                                  IConfiguration configuration)
        {
            services.AddDbContext<QuizAppDBContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DBConnection")));
           return services;
        }
    }

}