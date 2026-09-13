using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<TableService>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.Services.GetRequiredService<TableService>();

app.UseCors();
app.MapControllers();
app.Run();
