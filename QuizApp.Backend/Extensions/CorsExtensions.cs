namespace com.QuizAppBackend.Extensions
{
    public static class CoreExtensions
    {
        public static IServiceCollection SetupCors(this IServiceCollection services)
        {
            services.AddCors(options =>
       {
           options.AddPolicy("AllowReactApp", policy =>
           {
               policy.WithOrigins("http://localhost:5173")
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();
           });
       });

            return services;
        }
    }
}