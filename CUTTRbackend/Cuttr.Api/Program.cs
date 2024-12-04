using Cuttr.Api.Middleware;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Business.Managers;
using Cuttr.Business.Utilities;
using Cuttr.Infrastructure;
using Cuttr.Infrastructure.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ISwipeManager, SwipeManager>();
builder.Services.AddScoped<ISwipeRepository, SwipeRepository>();
builder.Services.AddScoped<IMatchManager, MatchManager>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMessageManager, MessageManager>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IReportManager, ReportManager>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IUserPreferencesManager, UserPreferencesManager>();
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();

//dbcontext
builder.Services.AddDbContext<CuttrDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CuttrDb"));
});


// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Read configuration from appsettings.json
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Replace default logging with Serilog
builder.Host.UseSerilog();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseHttpsRedirection();



app.Run();

