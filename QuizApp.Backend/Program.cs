

using QuizApp.Backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.SetupCors();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddControllers();
var app = builder.Build();
await app.Services.SeedAdminUserAsync();
app.ConfigureMiddleware();
app.Run();