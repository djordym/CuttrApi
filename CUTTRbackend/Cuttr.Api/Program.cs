using Cuttr.Api.Middleware;
using Cuttr.Business.Interfaces.ManagerInterfaces;
using Cuttr.Business.Interfaces.RepositoryInterfaces;
using Cuttr.Business.Managers;
using Cuttr.Business.Utilities;
using Cuttr.Infrastructure;
using Cuttr.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

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

// Add services to the container.
builder.Services.AddControllers();

// Register DbContext with DI
builder.Services.AddDbContext<CuttrDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CuttrDb"));
});

// Register Manager Services
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IPlantManager, PlantManager>();
builder.Services.AddScoped<ISwipeManager, SwipeManager>();
builder.Services.AddScoped<IMatchManager, MatchManager>();
builder.Services.AddScoped<IMessageManager, MessageManager>();
builder.Services.AddScoped<IReportManager, ReportManager>();
builder.Services.AddScoped<IUserPreferencesManager, UserPreferencesManager>();

// Register JwtTokenGenerator
builder.Services.AddScoped<JwtTokenGenerator>();


// Register Repository Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlantRepository, PlantRepository>();
builder.Services.AddScoped<ISwipeRepository, SwipeRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply pending migrations automatically (Development only)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CuttrDbContext>();
        dbContext.Database.Migrate();
    }

    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
