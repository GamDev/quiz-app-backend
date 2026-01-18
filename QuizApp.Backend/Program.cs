

using com.QuizAppBackend.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddCors();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddControllers();
var app = builder.Build();
app.ConfigureMiddleware();
app.Run();