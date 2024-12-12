
//all_cs_files_combined_cleaned

//Program
var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
.ReadFrom.Configuration(builder.Configuration)
.Enrich.FromLogContext()
.Enrich.WithEnvironmentName()
.Enrich.WithMachineName()
.Enrich.WithThreadId()
.WriteTo.Console()
.WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
.CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddControllers()
.AddJsonOptions(options =>
{
options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddDbContext<CuttrDbContext>(options =>
{
options.UseSqlServer(
builder.Configuration.GetConnectionString("CuttrDb"),
sqlOptions => sqlOptions.UseNetTopologySuite());
});
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IUserManager, UserManager>();
builder.Services.AddScoped<IPlantManager, PlantManager>();
builder.Services.AddScoped<ISwipeManager, SwipeManager>();
builder.Services.AddScoped<IMatchManager, MatchManager>();
builder.Services.AddScoped<IMessageManager, MessageManager>();
builder.Services.AddScoped<IReportManager, ReportManager>();
builder.Services.AddScoped<IUserPreferencesManager, UserPreferencesManager>();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlantRepository, PlantRepository>();
builder.Services.AddScoped<ISwipeRepository, SwipeRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
{
var dbContext = scope.ServiceProvider.GetRequiredService<CuttrDbContext>();
dbContext.Database.Migrate();
}
app.UseSwagger();
app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
//MatchController
namespace Cuttr.Api.Controllers
{
[ApiController]
[Route("api/matches")]
public class MatchController : ControllerBase
{
private readonly IMatchManager _matchManager;
private readonly ILogger<MatchController> _logger;
public MatchController(IMatchManager matchManager, ILogger<MatchController> logger)
{
_matchManager = matchManager;
_logger = logger;
}
[HttpGet]
public async Task<IActionResult> GetMatches()
{
try
{
int userId = GetAuthenticatedUserId();
var matches = await _matchManager.GetMatchesByUserIdAsync(userId);
return Ok(matches);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving matches for user.");
return BadRequest(ex.Message);
}
}
[HttpGet("{matchId}")]
public async Task<IActionResult> GetMatchById(int matchId)
{
try
{
var match = await _matchManager.GetMatchByIdAsync(matchId);
return Ok(match);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"Match with ID {matchId} not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving match with ID {matchId}.");
return BadRequest(ex.Message);
}
}
private int GetAuthenticatedUserId()
{
return int.Parse(User.FindFirst("sub")?.Value);
}
}
}
//MessageController
namespace Cuttr.Api.Controllers
{
[ApiController]
[Route("api/messages")]
public class MessageController : ControllerBase
{
private readonly IMessageManager _messageManager;
private readonly ILogger<MessageController> _logger;
public MessageController(IMessageManager messageManager, ILogger<MessageController> logger)
{
_messageManager = messageManager;
_logger = logger;
}
[HttpPost]
public async Task<IActionResult> SendMessage([FromBody] MessageRequest request)
{
try
{
int senderUserId = GetAuthenticatedUserId();
var messageResponse = await _messageManager.SendMessageAsync(request, senderUserId);
return Ok(messageResponse);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, "Match not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error sending message.");
return BadRequest(ex.Message);
}
}
[HttpGet("/api/matches/{matchId}/messages")]
public async Task<IActionResult> GetMessages(int matchId)
{
try
{
int userId = GetAuthenticatedUserId();
var messages = await _messageManager.GetMessagesByMatchIdAsync(matchId, userId);
return Ok(messages);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"Match with ID {matchId} not found.");
return NotFound(ex.Message);
}
catch (Business.Exceptions.UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access to messages.");
return Forbid(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving messages for match with ID {matchId}.");
return BadRequest(ex.Message);
}
}
private int GetAuthenticatedUserId()
{
return int.Parse(User.FindFirst("sub")?.Value);
}
}
}
//PlantController
namespace Cuttr.Api.Controllers
{
[ApiController]
[Route("api/plants")]
public class PlantController : ControllerBase
{
private readonly IPlantManager _plantManager;
private readonly ILogger<PlantController> _logger;
public PlantController(IPlantManager plantManager, ILogger<PlantController> logger)
{
_plantManager = plantManager;
_logger = logger;
}
[HttpPost]
[Consumes("multipart/form-data")]
public async Task<IActionResult> AddPlant([FromForm] PlantCreateRequest request)
{
try
{
var plantResponse = await _plantManager.AddPlantAsync(request);
return CreatedAtAction(nameof(GetPlantById), new { plantId = plantResponse.PlantId }, plantResponse);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, "User not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error adding plant.");
return BadRequest(ex.Message);
}
}
[HttpGet("{plantId}")]
public async Task<IActionResult> GetPlantById(int plantId)
{
try
{
var plantResponse = await _plantManager.GetPlantByIdAsync(plantId);
return Ok(plantResponse);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"Plant with ID {plantId} not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving plant with ID {plantId}.");
return BadRequest(ex.Message);
}
}
[HttpPut("{plantId}")]
public async Task<IActionResult> UpdatePlant(int plantId, [FromBody] PlantUpdateRequest request)
{
try
{
var plantResponse = await _plantManager.UpdatePlantAsync(plantId, request);
return Ok(plantResponse);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"Plant with ID {plantId} not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error updating plant with ID {plantId}.");
return BadRequest(ex.Message);
}
}
[HttpDelete("{plantId}")]
public async Task<IActionResult> DeletePlant(int plantId)
{
try
{
await _plantManager.DeletePlantAsync(plantId);
return NoContent();
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"Plant with ID {plantId} not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error deleting plant with ID {plantId}.");
return BadRequest(ex.Message);
}
}
[HttpGet("/api/users/{userId}/plants")]
public async Task<IActionResult> GetPlantsByUserId(int userId)
{
try
{
var plantResponses = await _plantManager.GetPlantsByUserIdAsync(userId);
return Ok(plantResponses);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving plants for user with ID {userId}.");
return BadRequest(ex.Message);
}
}
}
}
//ReportController
namespace Cuttr.Api.Controllers
{
[ApiController]
[Route("api/reports")]
public class ReportController : ControllerBase
{
private readonly IReportManager _reportManager;
private readonly ILogger<ReportController> _logger;
public ReportController(IReportManager reportManager, ILogger<ReportController> logger)
{
_reportManager = reportManager;
_logger = logger;
}
[HttpPost]
[Authorize]
public async Task<IActionResult> CreateReport([FromBody] ReportRequest request)
{
try
{
int reporterUserId = GetAuthenticatedUserId();
var reportResponse = await _reportManager.CreateReportAsync(request, reporterUserId);
return Ok(reportResponse);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, "Reported user not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error creating report.");
return BadRequest(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "Unexpected error creating report.");
return StatusCode(500, "An unexpected error occurred.");
}
}
private int GetAuthenticatedUserId()
{
return int.Parse(User.FindFirst("sub")?.Value);
}
}
}
//SwipeController
namespace Cuttr.Api.Controllers
{
[ApiController]
[Route("api/swipes")]
public class SwipeController : ControllerBase
{
private readonly ISwipeManager _swipeManager;
private readonly ILogger<SwipeController> _logger;
public SwipeController(ISwipeManager swipeManager, ILogger<SwipeController> logger)
{
_swipeManager = swipeManager;
_logger = logger;
}
[HttpPost]
public async Task<IActionResult> RecordSwipe([FromBody] List<SwipeRequest> requests)
{
try
{
var swipeResponses = await _swipeManager.RecordSwipesAsync(requests);
return Ok(swipeResponses);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, "One or more plants not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error recording swipes.");
return BadRequest(ex.Message);
}
}
[HttpGet("likable-plants")]
[Authorize]
public async Task<IActionResult> GetLikablePlants()
{
try
{
var userId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
var likablePlants = await _swipeManager.GetLikablePlantsAsync(userId);
return Ok(likablePlants);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error retrieving likable plants.");
return BadRequest(ex.Message);
}
}
}
}
//UserController
namespace Cuttr.Api.Controllers
{
[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
private readonly IUserManager _userManager;
private readonly ILogger<UserController> _logger;
public UserController(IUserManager userManager, ILogger<UserController> logger)
{
_userManager = userManager;
_logger = logger;
}
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
{
try
{
var userResponse = await _userManager.RegisterUserAsync(request);
return CreatedAtAction(nameof(GetUserById), new { userId = userResponse.UserId }, userResponse);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error registering user.");
return BadRequest(ex.Message);
}
}
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
{
try
{
var response = await _userManager.AuthenticateUserAsync(request);
return Ok(response);
}
catch (AuthenticationException ex)
{
_logger.LogWarning(ex, "Authentication failed.");
return Unauthorized(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error during authentication.");
return BadRequest(ex.Message);
}
}
[HttpGet("{userId}")]
public async Task<IActionResult> GetUserById(int userId)
{
try
{
var userResponse = await _userManager.GetUserByIdAsync(userId);
return Ok(userResponse);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"User with ID {userId} not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving user with ID {userId}.");
return BadRequest(ex.Message);
}
}
[HttpPut("{userId}")]
public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserUpdateRequest request)
{
try
{
var userResponse = await _userManager.UpdateUserAsync(userId, request);
return Ok(userResponse);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"User with ID {userId} not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error updating user with ID {userId}.");
return BadRequest(ex.Message);
}
}
[HttpDelete("{userId}")]
public async Task<IActionResult> DeleteUser(int userId)
{
try
{
await _userManager.DeleteUserAsync(userId);
return NoContent();
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"User with ID {userId} not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error deleting user with ID {userId}.");
return BadRequest(ex.Message);
}
}
[HttpPut("{userId}/profile-picture")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UpdateProfilePicture(int userId, [FromForm] UserProfileImageUpdateRequest request)
{
try
{
var userResponse = await _userManager.UpdateUserProfileImageAsync(userId, request);
return Ok(userResponse);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"User with ID {userId} not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error updating profile picture for user with ID {userId}.");
return BadRequest(ex.Message);
}
}
[HttpPut("{userId}/location")]
public async Task<IActionResult> UpdateLocation(int userId, [FromBody] UpdateLocationRequest request)
{
try
{
await _userManager.UpdateUserLocationAsync(userId, request.Latitude, request.Longitude);
return NoContent();
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"User with ID {userId} not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error updating location for user with ID {userId}.");
return BadRequest(ex.Message);
}
}
}
}
//UserPreferencesController
namespace Cuttr.Api.Controllers
{
[ApiController]
[Route("api/userpreferences")]
[Authorize]
public class UserPreferencesController : ControllerBase
{
private readonly IUserPreferencesManager _userPreferencesManager;
private readonly ILogger<UserPreferencesController> _logger;
public UserPreferencesController(IUserPreferencesManager userPreferencesManager, ILogger<UserPreferencesController> logger)
{
_userPreferencesManager = userPreferencesManager;
_logger = logger;
}
[HttpGet]
public async Task<IActionResult> GetUserPreferences()
{
try
{
int userId = GetAuthenticatedUserId();
var preferences = await _userPreferencesManager.GetUserPreferencesAsync(userId);
return Ok(preferences);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"User preferences for user ID {GetAuthenticatedUserId()} not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error retrieving user preferences.");
return BadRequest(ex.Message);
}
}
[HttpPost]
public async Task<IActionResult> CreateOrUpdateUserPreferences([FromBody] UserPreferencesRequest request)
{
try
{
int userId = GetAuthenticatedUserId();
var preferences = await _userPreferencesManager.CreateOrUpdateUserPreferencesAsync(userId, request);
return Ok(preferences);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error creating or updating user preferences.");
return BadRequest(ex.Message);
}
}
private int GetAuthenticatedUserId()
{
return int.Parse(User.FindFirst("sub")?.Value);
}
}
}
//ExceptionHandlingMiddleware
namespace Cuttr.Api.Middleware
{
public class ExceptionHandlingMiddleware
{
private readonly RequestDelegate _next;
private readonly ILogger<ExceptionHandlingMiddleware> _logger;
public ExceptionHandlingMiddleware(
RequestDelegate next,
ILogger<ExceptionHandlingMiddleware> logger)
{
_next = next;
_logger = logger;
}
public async Task Invoke(HttpContext context)
{
try
{
await _next(context);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unhandled exception occurred while processing the request. Correlation ID: {CorrelationId}", context.TraceIdentifier);
await HandleExceptionAsync(context, ex);
}
}
private static Task HandleExceptionAsync(
HttpContext context, Exception exception)
{
context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
context.Response.ContentType = "application/json";
var errorResponse = new
{
message = "An unexpected error occurred.",
correlationId = context.TraceIdentifier
};
var errorJson = JsonConvert.SerializeObject(errorResponse);
return context.Response.WriteAsync(errorJson);
}
}
}
//LoggingMiddleware
﻿namespace Cuttr.Api.Middleware
{
public class LoggingMiddleware
{
private readonly RequestDelegate _next;
private readonly ILogger<LoggingMiddleware> _logger;
public LoggingMiddleware(
RequestDelegate next,
ILogger<LoggingMiddleware> logger)
{
_next = next;
_logger = logger;
}
public async Task Invoke(HttpContext context)
{
await LogRequest(context);
var originalBodyStream = context.Response.Body;
{
context.Response.Body = responseBody;
await _next(context);
await LogResponse(context);
await responseBody.CopyToAsync(originalBodyStream);
}
}
private async Task LogRequest(HttpContext context)
{
context.Request.EnableBuffering();
var bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();
context.Request.Body.Position = 0;
_logger.LogInformation("HTTP Request Information: {Method} {Path} {QueryString} {Body}",
context.Request.Method,
context.Request.Path,
context.Request.QueryString,
bodyAsText);
}
private async Task LogResponse(HttpContext context)
{
context.Response.Body.Seek(0, SeekOrigin.Begin);
var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
context.Response.Body.Seek(0, SeekOrigin.Begin);
_logger.LogInformation("HTTP Response Information: {StatusCode} {Body}",
context.Response.StatusCode,
text);
}
}
}
//.NETCoreApp,Version=v8.0.AssemblyAttributes
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//Cuttr.Api.AssemblyInfo
[assembly: System.Reflection.AssemblyCompanyAttribute("Cuttr.Api")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0")]
[assembly: System.Reflection.AssemblyProductAttribute("Cuttr.Api")]
[assembly: System.Reflection.AssemblyTitleAttribute("Cuttr.Api")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]
//Cuttr.Api.GlobalUsings.g

//Cuttr.Api.MvcApplicationPartsAssemblyInfo
[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartAttribute("Microsoft.AspNetCore.OpenApi")]
[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartAttribute("Swashbuckle.AspNetCore.SwaggerGen")]
//MessageRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class MessageRequest
{
public int MatchId { get; set; }
public string MessageText { get; set; }
}
}
//PlantCreateRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class PlantCreateRequest
{
public PlantRequest PlantDetails { get; set; }
public IFormFile Image { get; set; }
}
}
//PlantRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class PlantRequest
{
public int UserId { get; set; }
public string SpeciesName { get; set; }
public string CareRequirements { get; set; }
public string Description { get; set; }
public string Category { get; set; }
}
}
//ReportRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class ReportRequest
{
public int ReportedUserId { get; set; }
public string Reason { get; set; }
public string Comments { get; set; }
}
}
//SwipeRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class SwipeRequest
{
public int SwiperPlantId { get; set; }
public int SwipedPlantId { get; set; }
public bool IsLike { get; set; }
}
}
//UpdateUserLocationRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class UpdateLocationRequest
{
public double Latitude { get; set; }
public double Longitude { get; set; }
}
}
//UserLoginRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class UserLoginRequest
{
public string Email { get; set; }
public string Password { get; set; }
}
}
//UserPreferencesRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class UserPreferencesRequest
{
public int SearchRadius { get; set; }
public List<string> PreferredCategories { get; set; }
}
}
//UserProfileImageUpdateRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class UserProfileImageUpdateRequest
{
public IFormFile Image { get; set; }
}
}
//UserRegistrationRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class UserRegistrationRequest
{
public string Email { get; set; }
public string Password { get; set; }
public string Name { get; set; }
}
}
//UserUpdateRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class UserUpdateRequest
{
public string Name { get; set; }
public string Bio { get; set; }
}
}
//MatchResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class MatchResponse
{
public int MatchId { get; set; }
public PlantResponse Plant1 { get; set; }
public PlantResponse Plant2 { get; set; }
public UserResponse User1 { get; set; }
public UserResponse User2 { get; set; }
}
}
//MessageResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class MessageResponse
{
public int MessageId { get; set; }
public int MatchId { get; set; }
public int SenderUserId { get; set; }
public string MessageText { get; set; }
public DateTime SentAt { get; set; }
public bool IsRead { get; set; }
}
}
//PlantResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class PlantResponse
{
public int PlantId { get; set; }
public int UserId { get; set; }
public string SpeciesName { get; set; }
public string CareRequirements { get; set; }
public string Description { get; set; }
public string Category { get; set; }
public string ImageUrl { get; set; }
}
}
//ReportResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class ReportResponse
{
public int ReportId { get; set; }
public int ReporterUserId { get; set; }
public int ReportedUserId { get; set; }
public string Reason { get; set; }
public string Comments { get; set; }
public DateTime CreatedAt { get; set; }
public bool IsResolved { get; set; }
}
}
//SwipeResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class SwipeResponse
{
public bool IsMatch { get; set; }
public MatchResponse Match { get; set; }
}
}
//UserLoginResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class UserLoginResponse
{
public string Token { get; set; }
public UserResponse User { get; set; }
}
}
//UserPreferencesResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class UserPreferencesResponse
{
public int UserId { get; set; }
public double SearchRadius { get; set; }
public List<string> PreferredCategories { get; set; }
}
}
//UserResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class UserResponse
{
public int UserId { get; set; }
public string Email { get; set; }
public string Name { get; set; }
public string ProfilePictureUrl { get; set; }
public string Bio { get; set; }
public double? LocationLatitude { get; set; }
public double? LocationLongitude { get; set; }
}
}
//Match
namespace Cuttr.Business.Entities
{
public class Match
{
public int MatchId { get; set; }
public int PlantId1 { get; set; }
public int PlantId2 { get; set; }
public int UserId1 { get; set; }
public int UserId2 { get; set; }
public DateTime CreatedAt { get; set; }
public Plant Plant1 { get; set; }
public Plant Plant2 { get; set; }
public User User1 { get; set; }
public User User2 { get; set; }
public List<Message> Messages { get; set; }
}
}
//Message
namespace Cuttr.Business.Entities
{
public class Message
{
public int MessageId { get; set; }
public int MatchId { get; set; }
public int SenderUserId { get; set; }
public string MessageText { get; set; }
public DateTime SentAt { get; set; }
public bool IsRead { get; set; }
public Match Match { get; set; }
public User SenderUser { get; set; }
}
}
//Plant
namespace Cuttr.Business.Entities
{
public class Plant
{
public int PlantId { get; set; }
public int UserId { get; set; }
public string SpeciesName { get; set; }
public string? Description { get; set; }
public PlantStage PlantStage { get; set; }
public PlantCategory PlantCategory { get; set; }
public WateringNeed WateringNeed { get; set; }
public LightRequirement LightRequirement { get; set; }
public Size? Size { get; set; }
public IndoorOutdoor? IndoorOutdoor { get; set; }
public PropagationEase? PropagationEase { get; set; }
public PetFriendly? PetFriendly { get; set; }
public List<Extras>? Extras { get; set; }
public string? ImageUrl { get; set; }
public User User { get; set; }
}
}
//Report
namespace Cuttr.Business.Entities
{
public class Report
{
public int ReportId { get; set; }
public int ReporterUserId { get; set; }
public int ReportedUserId { get; set; }
public string Reason { get; set; }
public string Comments { get; set; }
public DateTime CreatedAt { get; set; }
public bool IsResolved { get; set; }
public User ReporterUser { get; set; }
public User ReportedUser { get; set; }
}
}
//Swipe
namespace Cuttr.Business.Entities
{
public class Swipe
{
public int SwipeId { get; set; }
public int SwiperPlantId { get; set; }
public int SwipedPlantId { get; set; }
public bool IsLike { get; set; }
public Plant SwiperPlant { get; set; }
public Plant SwipedPlant { get; set; }
}
}
//User
namespace Cuttr.Business.Entities
{
public class User
{
public int UserId { get; set; }
public string Email { get; set; }
public string PasswordHash { get; set; }
public string Name { get; set; }
public string ProfilePictureUrl { get; set; }
public string Bio { get; set; }
public double? LocationLatitude { get; set; }
public double? LocationLongitude { get; set; }
public List<Plant> Plants { get; set; }
public UserPreferences Preferences { get; set; }
}
}
//UserPreferences
namespace Cuttr.Business.Entities
{
public class UserPreferences
{
public int UserId { get; set; }
public int SearchRadius { get; set; }
public List<string> PreferredCategories { get; set; }
public User User { get; set; }
}
}
//PlantProperties
namespace Cuttr.Business.Enums
{
public enum PlantStage
{
Cutting,
GrownPlantTree
}
public enum PlantCategory
{
Succulent,
Cactus,
Fern,
Orchid,
Herb,
Palm,
LeafyHouseplant,
FloweringHouseplant,
Other
}
public enum WateringNeed
{
VeryLowWater,
LowWater,
ModerateWater,
HighWater,
VeryHighWater
}
public enum LightRequirement
{
FullSun,
PartialShade,
Shade
}
public enum Size
{
Small,
Medium,
Large
}
public enum IndoorOutdoor
{
Indoor,
Outdoor,
Both
}
public enum PropagationEase
{
Easy,
Moderate,
Difficult
}
public enum PetFriendly
{
Yes,
No
}
public enum Extras
{
Fragrant,
Edible,
Medicinal,
AirPurifying,
Decorative,
Flowering,
TropicalVibe,
FoliageHeavy,
DroughtTolerant,
HumidityLoving,
LowMaintenance,
WinterHardy,
BeginnerFriendly,
Fruiting,
PollinatorFriendly,
FastGrowing,
VariegatedFoliage,
Climbing,
GroundCover,
Rare
}
}
//AuthenticationException
namespace Cuttr.Business.Exceptions
{
public class AuthenticationException : Exception
{
public AuthenticationException() { }
public AuthenticationException(string message)
: base(message) { }
public AuthenticationException(string message, Exception innerException)
: base(message, innerException) { }
}
}
//BusinessException
namespace Cuttr.Business.Exceptions
{
public class BusinessException : Exception
{
public BusinessException() { }
public BusinessException(string message)
: base(message) { }
public BusinessException(string message, Exception innerException)
: base(message, innerException) { }
}
}
//NotFoundException
namespace Cuttr.Business.Exceptions
{
public class NotFoundException : Exception
{
public NotFoundException() { }
public NotFoundException(string message)
: base(message) { }
public NotFoundException(string message, Exception innerException)
: base(message, innerException) { }
}
}
//UnauthorizedAccessException
namespace Cuttr.Business.Exceptions
{
public class UnauthorizedAccessException : Exception
{
public UnauthorizedAccessException() { }
public UnauthorizedAccessException(string message)
: base(message) { }
public UnauthorizedAccessException(string message, Exception innerException)
: base(message, innerException) { }
}
}
//IMatchManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface IMatchManager
{
Task<IEnumerable<MatchResponse>> GetMatchesByUserIdAsync(int userId);
Task<MatchResponse> GetMatchByIdAsync(int matchId);
}
}
//IMessageManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface IMessageManager
{
Task<MessageResponse> SendMessageAsync(MessageRequest request, int senderUserId);
Task<IEnumerable<MessageResponse>> GetMessagesByMatchIdAsync(int matchId, int userId);
}
}
//IPlantManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface IPlantManager
{
Task<PlantResponse> AddPlantAsync(PlantCreateRequest request);
Task<PlantResponse> GetPlantByIdAsync(int plantId);
Task<PlantResponse> UpdatePlantAsync(int plantId, int userId, PlantUpdateRequest request);
Task DeletePlantAsync(int plantId);
Task<IEnumerable<PlantResponse>> GetPlantsByUserIdAsync(int userId);
}
}
//IReportManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface IReportManager
{
Task<ReportResponse> CreateReportAsync(ReportRequest request, int reporterUserId);
}
}
//ISwipeManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface ISwipeManager
{
Task<List<SwipeResponse>> RecordSwipesAsync(List<SwipeRequest> requests);
Task<List<PlantResponse>> GetLikablePlantsAsync(int userId);
}
}
//IUserManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface IUserManager
{
Task<UserResponse> RegisterUserAsync(UserRegistrationRequest request);
Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request);
Task<UserResponse> GetUserByIdAsync(int userId);
Task<UserResponse> UpdateUserAsync(int userId, UserUpdateRequest request);
Task DeleteUserAsync(int userId);
Task<UserResponse> UpdateUserProfileImageAsync(int userId, UserProfileImageUpdateRequest request);
Task UpdateUserLocationAsync(int userId, double latitude, double longitude);
}
}
//IUserPreferencesManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface IUserPreferencesManager
{
Task<UserPreferencesResponse> GetUserPreferencesAsync(int userId);
Task<UserPreferencesResponse> CreateOrUpdateUserPreferencesAsync(int userId, UserPreferencesRequest request);
}
}
//IMatchRepository
namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
public interface IMatchRepository
{
Task<IEnumerable<Match>> GetMatchesByUserIdAsync(int userId);
Task<Match> GetMatchByIdAsync(int matchId);
Task<Match> AddMatchAsync(Match match);
}
}
//IMessageRepository
namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
public interface IMessageRepository
{
Task<Message> AddMessageAsync(Message message);
Task<IEnumerable<Message>> GetMessagesByMatchIdAsync(int matchId);
}
}
//IPlantRepository
namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
public interface IPlantRepository
{
Task<Plant> AddPlantAsync(Plant plant);
Task<Plant> GetPlantByIdAsync(int plantId);
Task UpdatePlantAsync(Plant plant);
Task DeletePlantAsync(int plantId);
Task<IEnumerable<Plant>> GetPlantsByUserIdAsync(int userId);
Task<IEnumerable<Plant>> GetAllPlantsAsync();
Task<IEnumerable<Plant>> GetPlantsWithinRadiusAsync(double originLat, double originLon, double radiusKm);
}
}
//IReportRepository
namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
public interface IReportRepository
{
Task<Report> AddReportAsync(Report report);
}
}
//ISwipeRepository
namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
public interface ISwipeRepository
{
Task AddSwipeAsync(Swipe swipe);
Task<Swipe> GetSwipeAsync(int swiperPlantId, int swipedPlantId, bool isLike);
Task<bool> HasSwipeAsync(int swiperPlantId, int swipedPlantId);
}
}
//IUserPreferencesRepository
namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
public interface IUserPreferencesRepository
{
Task<UserPreferences> GetUserPreferencesAsync(int userId);
Task<UserPreferences> AddUserPreferencesAsync(UserPreferences preferences);
Task UpdateUserPreferencesAsync(UserPreferences preferences);
}
}
//IUserRepository
namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
public interface IUserRepository
{
Task<User> CreateUserAsync(User user);
Task<User> GetUserByIdAsync(int userId);
Task<User> GetUserByEmailAsync(string email);
Task UpdateUserAsync(User user);
Task DeleteUserAsync(int userId);
Task UpdateUserLocationAsync(int userId, double latitude, double longitude);
}
}
//IBlobStorageService
namespace Cuttr.Business.Interfaces.Services
{
public interface IBlobStorageService
{
Task<string> UploadFileAsync(IFormFile file, string containerName);
Task DeleteFileAsync(string fileUrl, string containerName);
}
}
//MatchManager
namespace Cuttr.Business.Managers
{
public class MatchManager : IMatchManager
{
private readonly IMatchRepository _matchRepository;
private readonly ILogger<MatchManager> _logger;
public MatchManager(IMatchRepository matchRepository, ILogger<MatchManager> logger)
{
_matchRepository = matchRepository;
_logger = logger;
}
public async Task<IEnumerable<MatchResponse>> GetMatchesByUserIdAsync(int userId)
{
try
{
var matches = await _matchRepository.GetMatchesByUserIdAsync(userId);
return BusinessToContractMapper.MapToMatchResponse(matches);
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error retrieving matches for user with ID {userId}.");
throw new BusinessException("Error retrieving matches.", ex);
}
}
public async Task<MatchResponse> GetMatchByIdAsync(int matchId)
{
try
{
var match = await _matchRepository.GetMatchByIdAsync(matchId);
if (match == null)
{
throw new NotFoundException($"Match with ID {matchId} not found.");
}
return BusinessToContractMapper.MapToMatchResponse(match);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error retrieving match with ID {matchId}.");
throw new BusinessException("Error retrieving match.", ex);
}
}
}
}
//MessageManager
namespace Cuttr.Business.Managers
{
public class MessageManager : IMessageManager
{
private readonly IMessageRepository _messageRepository;
private readonly IMatchRepository _matchRepository;
private readonly ILogger<MessageManager> _logger;
public MessageManager(
IMessageRepository messageRepository,
IMatchRepository matchRepository,
ILogger<MessageManager> logger)
{
_messageRepository = messageRepository;
_matchRepository = matchRepository;
_logger = logger;
}
public async Task<MessageResponse> SendMessageAsync(MessageRequest request, int senderUserId)
{
try
{
var match = await _matchRepository.GetMatchByIdAsync(request.MatchId);
if (match == null)
throw new NotFoundException($"Match with ID {request.MatchId} not found.");
if (match.UserId1 != senderUserId && match.UserId2 != senderUserId)
throw new BusinessException("Sender user is not part of the match.");
var message = ContractToBusinessMapper.MapToMessage(request, senderUserId);
var createdMessage = await _messageRepository.AddMessageAsync(message);
return BusinessToContractMapper.MapToMessageResponse(createdMessage);
}
catch (NotFoundException)
{
throw;
}
catch (BusinessException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "Error sending message.");
throw new BusinessException("Error sending message.", ex);
}
}
public async Task<IEnumerable<MessageResponse>> GetMessagesByMatchIdAsync(int matchId, int userId)
{
try
{
var match = await _matchRepository.GetMatchByIdAsync(matchId);
if (match == null)
throw new NotFoundException($"Match with ID {matchId} not found.");
if (match.UserId1 != userId && match.UserId2 != userId)
throw new Exceptions.UnauthorizedAccessException("User is not part of the match.");
var messages = await _messageRepository.GetMessagesByMatchIdAsync(matchId);
return BusinessToContractMapper.MapToMessageResponse(messages);
}
catch (NotFoundException)
{
throw;
}
catch (Exceptions.UnauthorizedAccessException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error retrieving messages for match with ID {matchId}.");
throw new BusinessException("Error retrieving messages.", ex);
}
}
}
}
//PlantManager
namespace Cuttr.Business.Managers
{
public class PlantManager : IPlantManager
{
private readonly IPlantRepository _plantRepository;
private readonly IUserRepository _userRepository;
private readonly ILogger<PlantManager> _logger;
private readonly IBlobStorageService _blobStorageService;
private const string PlantImagesContainer = "plant-images";
public PlantManager(IPlantRepository plantRepository, IUserRepository userRepository, ILogger<PlantManager> logger, IBlobStorageService blobStorageService)
{
_plantRepository = plantRepository;
_userRepository = userRepository;
_logger = logger;
_blobStorageService = blobStorageService;
}
public async Task<PlantResponse> AddPlantAsync(PlantCreateRequest request)
{
try
{
var user = await _userRepository.GetUserByIdAsync(request.PlantDetails.UserId);
if (user == null)
{
throw new NotFoundException($"User with ID {request.PlantDetails.UserId} not found.");
}
string imageUrl = null;
if (request.Image != null && request.Image.Length > 0)
{
imageUrl = await _blobStorageService.UploadFileAsync(request.Image, PlantImagesContainer);
}
var plant = ContractToBusinessMapper.MapToPlant(request.PlantDetails);
plant.ImageUrl = imageUrl;
var createdPlant = await _plantRepository.AddPlantAsync(plant);
return BusinessToContractMapper.MapToPlantResponse(createdPlant);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "Error adding plant.");
throw new BusinessException("Error adding plant.", ex);
}
}
public async Task<PlantResponse> GetPlantByIdAsync(int plantId)
{
try
{
var plant = await _plantRepository.GetPlantByIdAsync(plantId);
if (plant == null)
{
throw new NotFoundException($"Plant with ID {plantId} not found.");
}
return BusinessToContractMapper.MapToPlantResponse(plant);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error retrieving plant with ID {plantId}.");
throw new BusinessException("Error retrieving plant.", ex);
}
}
public async Task<PlantResponse> UpdatePlantAsync(int plantId,int userId, PlantUpdateRequest request)
{
try
{
var plant = await _plantRepository.GetPlantByIdAsync(plantId);
if (plant == null)
{
throw new NotFoundException($"Plant with ID {plantId} not found.");
}
if (plant.UserId != userId)
{
throw new BusinessException("Plant does not belong to user.");
}
ContractToBusinessMapper.MapToPlant(request, plant);
await _plantRepository.UpdatePlantAsync(plant);
return BusinessToContractMapper.MapToPlantResponse(plant);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error updating plant with ID {plantId}.");
throw new BusinessException("Error updating plant.", ex);
}
}
public async Task DeletePlantAsync(int plantId)
{
try
{
var plant = await _plantRepository.GetPlantByIdAsync(plantId);
if (plant == null)
{
throw new NotFoundException($"Plant with ID {plantId} not found.");
}
await _plantRepository.DeletePlantAsync(plantId);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error deleting plant with ID {plantId}.");
throw new BusinessException("Error deleting plant.", ex);
}
}
public async Task<IEnumerable<PlantResponse>> GetPlantsByUserIdAsync(int userId)
{
try
{
var user = await _userRepository.GetUserByIdAsync(userId);
if (user == null)
{
throw new NotFoundException($"User with ID {userId} not found.");
}
var plants = await _plantRepository.GetPlantsByUserIdAsync(userId);
return BusinessToContractMapper.MapToPlantResponse(plants);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error retrieving plants for user with ID {userId}.");
throw new BusinessException("Error retrieving plants.", ex);
}
}
}
}
//ReportManager
namespace Cuttr.Business.Managers
{
public class ReportManager : IReportManager
{
private readonly IReportRepository _reportRepository;
private readonly IUserRepository _userRepository;
private readonly ILogger<ReportManager> _logger;
public ReportManager(
IReportRepository reportRepository,
IUserRepository userRepository,
ILogger<ReportManager> logger)
{
_reportRepository = reportRepository;
_userRepository = userRepository;
_logger = logger;
}
public async Task<ReportResponse> CreateReportAsync(ReportRequest request, int reporterUserId)
{
try
{
var reportedUser = await _userRepository.GetUserByIdAsync(request.ReportedUserId);
if (reportedUser == null)
{
throw new NotFoundException($"Reported user with ID {request.ReportedUserId} not found.");
}
var report = new Report
{
ReporterUserId = reporterUserId,
ReportedUserId = request.ReportedUserId,
Reason = request.Reason,
Comments = request.Comments,
CreatedAt = DateTime.UtcNow,
IsResolved = false
};
var createdReport = await _reportRepository.AddReportAsync(report);
var response = BusinessToContractMapper.MapToReportResponse(createdReport);
return response;
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "Error creating report.");
throw new BusinessException("Error creating report.", ex);
}
}
}
}
//SwipeManager
namespace Cuttr.Business.Managers
{
public class SwipeManager : ISwipeManager
{
private readonly ISwipeRepository _swipeRepository;
private readonly IPlantRepository _plantRepository;
private readonly ILogger<SwipeManager> _logger;
private readonly IUserRepository _userRepository;
public SwipeManager(
ISwipeRepository swipeRepository,
IPlantRepository plantRepository,
IUserRepository userRepository,
ILogger<SwipeManager> logger)
{
_swipeRepository = swipeRepository;
_plantRepository = plantRepository;
_logger = logger;
_userRepository = userRepository;
}
public async Task<List<SwipeResponse>> RecordSwipesAsync(List<SwipeRequest> requests)
{
var responses = new List<SwipeResponse>();
foreach (var request in requests)
{
try
{
var swiperPlant = await _plantRepository.GetPlantByIdAsync(request.SwiperPlantId);
if (swiperPlant == null)
throw new NotFoundException($"Swiper plant with ID {request.SwiperPlantId} not found.");
var swipedPlant = await _plantRepository.GetPlantByIdAsync(request.SwipedPlantId);
if (swipedPlant == null)
throw new NotFoundException($"Swiped plant with ID {request.SwipedPlantId} not found.");
var swipe = ContractToBusinessMapper.MapToSwipe(request);
await _swipeRepository.AddSwipeAsync(swipe);
Swipe oppositeSwipe = null;
if (request.IsLike)
{
oppositeSwipe = await _swipeRepository.GetSwipeAsync(
request.SwipedPlantId,
request.SwiperPlantId,
true);
}
var response = new SwipeResponse { IsMatch = oppositeSwipe != null };
if (response.IsMatch)
{
bool isSwiperUserFirst = swiperPlant.UserId < swipedPlant.UserId;
var match = new Match
{
PlantId1 = isSwiperUserFirst ? swiperPlant.PlantId : swipedPlant.PlantId,
PlantId2 = isSwiperUserFirst ? swipedPlant.PlantId : swiperPlant.PlantId,
UserId1 = isSwiperUserFirst ? swiperPlant.UserId : swipedPlant.UserId,
UserId2 = isSwiperUserFirst ? swipedPlant.UserId : swiperPlant.UserId,
CreatedAt = DateTime.UtcNow
};
}
responses.Add(response);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "Error recording swipe.");
throw new BusinessException("Error recording swipe.", ex);
}
}
return responses;
}
public async Task<List<PlantResponse>> GetLikablePlantsAsync(int userId)
{
try
{
var user = await _userRepository.GetUserByIdAsync(userId);
if (user == null)
throw new BusinessException("User not found.");
if (user.Preferences == null)
throw new BusinessException("User preferences not found.");
int radius;
if (user.Preferences.SearchRadius == null)
{
radius = 9999;
} else
{
radius = user.Preferences.SearchRadius;
}
if (user.LocationLatitude == null || user.LocationLongitude == null)
throw new BusinessException("User location not set.");
double userLat = user.LocationLatitude.Value;
double userLon = user.LocationLongitude.Value;
var candidatePlants = await _plantRepository.GetPlantsWithinRadiusAsync(userLat, userLon, radius);
candidatePlants = candidatePlants.Where(p => p.UserId != userId);
var userPlants = await _plantRepository.GetPlantsByUserIdAsync(userId);
var likablePlants = new List<PlantResponse>();
foreach (var plant in candidatePlants)
{
bool hasUninteractedPlant = (await Task.WhenAll(userPlants.Select(async up =>
!await _swipeRepository.HasSwipeAsync(up.PlantId, plant.PlantId)
))).Any(result => result);
if (hasUninteractedPlant)
{
likablePlants.Add(BusinessToContractMapper.MapToPlantResponse(plant));
}
}
return likablePlants;
}
catch (Exception ex)
{
_logger.LogError(ex, "Error retrieving likable plants.");
throw new BusinessException("Error retrieving likable plants.", ex);
}
}
}
}
//UserManager
namespace Cuttr.Business.Managers
{
public class UserManager : IUserManager
{
private readonly IUserRepository _userRepository;
private readonly ILogger<UserManager> _logger;
private readonly JwtTokenGenerator _jwtTokenGenerator;
private readonly IBlobStorageService _blobStorageService;
private const string ProfileImagesContainer = "profile-images";
public UserManager(IUserRepository userRepository, ILogger<UserManager> logger, JwtTokenGenerator jwtTokenGenerator, IBlobStorageService blobStorageService)
{
_userRepository = userRepository;
_logger = logger;
_jwtTokenGenerator = jwtTokenGenerator;
_blobStorageService = blobStorageService;
}
public async Task<UserResponse> RegisterUserAsync(UserRegistrationRequest request)
{
try
{
if (await _userRepository.GetUserByEmailAsync(request.Email) != null)
{
throw new BusinessException("Email already registered.");
}
var user = ContractToBusinessMapper.MapToUser(request);
user.PasswordHash = PasswordHasher.HashPassword(user.PasswordHash);
var createdUser = await _userRepository.CreateUserAsync(user);
return BusinessToContractMapper.MapToUserResponse(createdUser);
}
catch (Exception ex)
{
_logger.LogError(ex, "Error registering user.");
throw new BusinessException("Error registering user.", ex);
}
}
public async Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request)
{
try
{
var user = await _userRepository.GetUserByEmailAsync(request.Email);
if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
{
throw new AuthenticationException("Invalid email or password.");
}
string token = _jwtTokenGenerator.GenerateToken(user);
return BusinessToContractMapper.MapToUserLoginResponse(user, token);
}
catch (AuthenticationException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "Error authenticating user.");
throw new BusinessException("Error authenticating user.", ex);
}
}
public async Task<UserResponse> GetUserByIdAsync(int userId)
{
try
{
var user = await _userRepository.GetUserByIdAsync(userId);
if (user == null)
{
throw new NotFoundException($"User with ID {userId} not found.");
}
return BusinessToContractMapper.MapToUserResponse(user);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error retrieving user with ID {userId}.");
throw new BusinessException("Error retrieving user.", ex);
}
}
public async Task<UserResponse> UpdateUserAsync(int userId, UserUpdateRequest request)
{
try
{
var user = await _userRepository.GetUserByIdAsync(userId);
if (user == null)
{
throw new NotFoundException($"User with ID {userId} not found.");
}
ContractToBusinessMapper.MapToUser(request, user);
await _userRepository.UpdateUserAsync(user);
return BusinessToContractMapper.MapToUserResponse(user);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error updating user with ID {userId}.");
throw new BusinessException("Error updating user.", ex);
}
}
public async Task DeleteUserAsync(int userId)
{
try
{
var user = await _userRepository.GetUserByIdAsync(userId);
if (user == null)
{
throw new NotFoundException($"User with ID {userId} not found.");
}
await _userRepository.DeleteUserAsync(userId);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error deleting user with ID {userId}.");
throw new BusinessException("Error deleting user.", ex);
}
}
public async Task<UserResponse> UpdateUserProfileImageAsync(int userId, UserProfileImageUpdateRequest request)
{
try
{
var user = await _userRepository.GetUserByIdAsync(userId);
if (user == null)
{
throw new NotFoundException($"User with ID {userId} not found.");
}
string imageUrl = null;
if (request.Image != null && request.Image.Length > 0)
{
imageUrl = await _blobStorageService.UploadFileAsync(request.Image, ProfileImagesContainer);
if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
{
await _blobStorageService.DeleteFileAsync(user.ProfilePictureUrl, ProfileImagesContainer);
}
user.ProfilePictureUrl = imageUrl;
}
await _userRepository.UpdateUserAsync(user);
return BusinessToContractMapper.MapToUserResponse(user);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error updating profile picture for user with ID {userId}.");
throw new BusinessException("Error updating profile picture.", ex);
}
}
public async Task UpdateUserLocationAsync(int userId, double latitude, double longitude)
{
try
{
var user = await _userRepository.GetUserByIdAsync(userId);
if (user == null)
throw new NotFoundException($"User with ID {userId} not found.");
await _userRepository.UpdateUserLocationAsync(userId, latitude, longitude);
} catch (NotFoundException)
{
throw;
} catch (Exception ex)
{
_logger.LogError(ex, $"Error updating location for user with ID {userId}.");
throw new BusinessException("Error updating location.", ex);
}
}
}
}
//UserPreferencesManager
namespace Cuttr.Business.Managers
{
public class UserPreferencesManager : IUserPreferencesManager
{
private readonly IUserPreferencesRepository _userPreferencesRepository;
private readonly IUserRepository _userRepository;
private readonly ILogger<UserPreferencesManager> _logger;
public UserPreferencesManager(
IUserPreferencesRepository userPreferencesRepository,
IUserRepository userRepository,
ILogger<UserPreferencesManager> logger)
{
_userPreferencesRepository = userPreferencesRepository;
_userRepository = userRepository;
_logger = logger;
}
public async Task<UserPreferencesResponse> GetUserPreferencesAsync(int userId)
{
try
{
var preferences = await _userPreferencesRepository.GetUserPreferencesAsync(userId);
if (preferences == null)
{
throw new NotFoundException($"User preferences for user ID {userId} not found.");
}
return BusinessToContractMapper.MapToUserPreferencesResponse(preferences);
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error retrieving user preferences for user ID {userId}.");
throw new BusinessException("Error retrieving user preferences.", ex);
}
}
public async Task<UserPreferencesResponse> CreateOrUpdateUserPreferencesAsync(int userId, UserPreferencesRequest request)
{
try
{
var user = await _userRepository.GetUserByIdAsync(userId);
if (user == null)
{
throw new NotFoundException($"User with ID {userId} not found.");
}
var preferences = await _userPreferencesRepository.GetUserPreferencesAsync(userId);
if (preferences == null)
{
preferences = ContractToBusinessMapper.MapToUserPreferences(request);
preferences.UserId = userId;
var createdPreferences = await _userPreferencesRepository.AddUserPreferencesAsync(preferences);
return BusinessToContractMapper.MapToUserPreferencesResponse(createdPreferences);
}
else
{
ContractToBusinessMapper.MapToUserPreferences(request, preferences);
await _userPreferencesRepository.UpdateUserPreferencesAsync(preferences);
return BusinessToContractMapper.MapToUserPreferencesResponse(preferences);
}
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error creating or updating user preferences for user ID {userId}.");
throw new BusinessException("Error creating or updating user preferences.", ex);
}
}
}
}
//BusinessToContractMapper
namespace Cuttr.Business.Mappers
{
public static class BusinessToContractMapper
{
public static UserResponse MapToUserResponse(User user)
{
if (user == null)
return null;
return new UserResponse
{
UserId = user.UserId,
Email = user.Email,
Name = user.Name,
ProfilePictureUrl = user.ProfilePictureUrl,
Bio = user.Bio,
LocationLatitude = user.LocationLatitude,
LocationLongitude = user.LocationLongitude
};
}
public static UserLoginResponse MapToUserLoginResponse(User user, string token)
{
return new UserLoginResponse
{
Token = token,
User = MapToUserResponse(user)
};
}
public static PlantResponse MapToPlantResponse(Plant plant)
{
if (plant == null)
return null;
return new PlantResponse
{
PlantId = plant.PlantId,
UserId = plant.UserId,
SpeciesName = plant.SpeciesName,
CareRequirements = plant.CareRequirements,
Description = plant.Description,
Category = plant.Category,
ImageUrl = plant.ImageUrl
};
}
public static IEnumerable<PlantResponse> MapToPlantResponse(IEnumerable<Plant> plants)
{
return plants?.Select(MapToPlantResponse);
}
public static MatchResponse MapToMatchResponse(Match match)
{
if (match == null)
return null;
return new MatchResponse
{
MatchId = match.MatchId,
Plant1 = MapToPlantResponse(match.Plant1),
Plant2 = MapToPlantResponse(match.Plant2),
User1 = MapToUserResponse(match.User1),
User2 = MapToUserResponse(match.User2)
};
}
public static IEnumerable<MatchResponse> MapToMatchResponse(IEnumerable<Match> matches)
{
return matches?.Select(MapToMatchResponse);
}
public static MessageResponse MapToMessageResponse(Message message)
{
if (message == null)
return null;
return new MessageResponse
{
MessageId = message.MessageId,
MatchId = message.MatchId,
SenderUserId = message.SenderUserId,
MessageText = message.MessageText,
SentAt = message.SentAt,
IsRead = message.IsRead
};
}
public static IEnumerable<MessageResponse> MapToMessageResponse(IEnumerable<Message> messages)
{
return messages?.Select(MapToMessageResponse);
}
public static ReportResponse MapToReportResponse(Report report)
{
if (report == null)
return null;
return new ReportResponse
{
ReportId = report.ReportId,
ReporterUserId = report.ReporterUserId,
ReportedUserId = report.ReportedUserId,
Reason = report.Reason,
Comments = report.Comments,
CreatedAt = report.CreatedAt,
IsResolved = report.IsResolved
};
}
public static UserPreferencesResponse MapToUserPreferencesResponse(UserPreferences preferences)
{
if (preferences == null)
return null;
return new UserPreferencesResponse
{
UserId = preferences.UserId,
SearchRadius = preferences.SearchRadius,
PreferredCategories = preferences.PreferredCategories
};
}
}
}
//ContractToBusinessMapper
namespace Cuttr.Business.Mappers
{
public static class ContractToBusinessMapper
{
public static User MapToUser(UserRegistrationRequest request)
{
if (request == null)
return null;
return new User
{
Email = request.Email,
PasswordHash = request.Password,
Name = request.Name
};
}
public static void MapToUser(UserUpdateRequest request, User user)
{
if (request == null || user == null)
return;
user.Name = request.Name ?? user.Name;
user.Bio = request.Bio ?? user.Bio;
}
public static Plant MapToPlant(PlantRequest request)
{
if (request == null)
return null;
return new Plant
{
UserId = request.UserId,
SpeciesName = request.SpeciesName,
Description = request.Description,
};
}
public static void MapToPlantForUpdate(PlantRequest request, Plant plant)
{
if (request == null || plant == null)
return;
plant.SpeciesName = request.SpeciesName ?? plant.SpeciesName;
plant.Description = request.Description ?? plant.Description;
plant.PlantStage = request.PlantStage ?? plant.PlantStage;
}
public static Swipe MapToSwipe(SwipeRequest request)
{
if (request == null)
return null;
return new Swipe
{
SwiperPlantId = request.SwiperPlantId,
SwipedPlantId = request.SwipedPlantId,
IsLike = request.IsLike
};
}
public static Message MapToMessage(MessageRequest request, int senderUserId)
{
if (request == null)
return null;
return new Message
{
MatchId = request.MatchId,
SenderUserId = senderUserId,
MessageText = request.MessageText,
SentAt = DateTime.UtcNow,
IsRead = false
};
}
public static UserPreferences MapToUserPreferences(UserPreferencesRequest request)
{
if (request == null)
return null;
return new UserPreferences
{
SearchRadius = request.SearchRadius,
PreferredCategories = request.PreferredCategories
};
}
public static void MapToUserPreferences(UserPreferencesRequest request, UserPreferences preferences)
{
if (request == null || preferences == null)
return;
preferences.SearchRadius = request.SearchRadius;
preferences.PreferredCategories = request.PreferredCategories;
}
}
}
//.NETCoreApp,Version=v8.0.AssemblyAttributes
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//Cuttr.Business.AssemblyInfo
[assembly: System.Reflection.AssemblyCompanyAttribute("Cuttr.Business")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0")]
[assembly: System.Reflection.AssemblyProductAttribute("Cuttr.Business")]
[assembly: System.Reflection.AssemblyTitleAttribute("Cuttr.Business")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]
//Cuttr.Business.GlobalUsings.g

//BlobStorageService
namespace Cuttr.Business.Services
{
public class BlobStorageService : IBlobStorageService
{
private readonly IConfiguration _configuration;
private readonly ILogger<BlobStorageService> _logger;
public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
{
_configuration = configuration;
_logger = logger;
}
public async Task<string> UploadFileAsync(IFormFile file, string containerName)
{
if (file == null || file.Length == 0)
throw new ArgumentException("File is empty.");
var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
var extension = Path.GetExtension(file.FileName).ToLower();
if (!allowedExtensions.Contains(extension))
throw new ArgumentException("Unsupported file type.");
const long maxFileSize = 5 * 1024 * 1024;
if (file.Length > maxFileSize)
throw new ArgumentException("File size exceeds the limit.");
var blobName = Guid.NewGuid().ToString() + extension;
BlobContainerClient containerClient = GetContainerClient(containerName);
BlobClient blobClient = containerClient.GetBlobClient(blobName);
try
{
{
await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
}
return blobClient.Uri.ToString();
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error uploading file to Azure Blob Storage in container '{containerName}'.");
throw new BusinessException("Error uploading image.", ex);
}
}
public async Task DeleteFileAsync(string fileUrl, string containerName)
{
try
{
if (string.IsNullOrEmpty(fileUrl))
return;
Uri uri = new Uri(fileUrl);
string blobName = Path.GetFileName(uri.LocalPath);
BlobContainerClient containerClient = GetContainerClient(containerName);
BlobClient blobClient = containerClient.GetBlobClient(blobName);
await blobClient.DeleteIfExistsAsync();
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error deleting file from Azure Blob Storage. URL: {fileUrl}");
throw new BusinessException("Error deleting image.", ex);
}
}
private BlobContainerClient GetContainerClient(string containerName)
{
string connectionString = _configuration.GetConnectionString("AzureBlobStorage");
var containerClient = new BlobContainerClient(connectionString, containerName);
containerClient.CreateIfNotExists(PublicAccessType.Blob);
return containerClient;
}
}
}
//JwtTokenGenerator
namespace Cuttr.Business.Utilities
{
public class JwtTokenGenerator
{
private readonly IConfiguration _configuration;
public JwtTokenGenerator(IConfiguration configuration)
{
_configuration = configuration;
}
public string GenerateToken(User user)
{
var tokenHandler = new JwtSecurityTokenHandler();
var secretKey = _configuration["Jwt:Secret"];
var key = Encoding.ASCII.GetBytes(secretKey);
var tokenDescriptor = new SecurityTokenDescriptor
{
Subject = new ClaimsIdentity(new[] {
new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
new Claim(ClaimTypes.Email, user.Email),
new Claim(ClaimTypes.Name, user.Name)
}),
Expires = DateTime.UtcNow.AddHours(1),
SigningCredentials = new SigningCredentials(
new SymmetricSecurityKey(key),
SecurityAlgorithms.HmacSha256Signature)
};
var token = tokenHandler.CreateToken(tokenDescriptor);
return tokenHandler.WriteToken(token);
}
}
}
//PasswordHasher
namespace Cuttr.Business.Utilities
{
public static class PasswordHasher
{
private const int WorkFactor = 12;
public static string HashPassword(string password)
{
return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
}
public static bool VerifyPassword(string password, string hashedPassword)
{
return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
}
}
}
//CuttrDbContext
namespace Cuttr.Infrastructure
{
public class CuttrDbContext : DbContext
{
public CuttrDbContext(DbContextOptions<CuttrDbContext> options)
: base(options)
{
}
public DbSet<UserEF> Users { get; set; }
public DbSet<PlantEF> Plants { get; set; }
public DbSet<SwipeEF> Swipes { get; set; }
public DbSet<MatchEF> Matches { get; set; }
public DbSet<MessageEF> Messages { get; set; }
public DbSet<ReportEF> Reports { get; set; }
public DbSet<UserPreferencesEF> UserPreferences { get; set; }
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
base.OnModelCreating(modelBuilder);
modelBuilder.Entity<UserEF>(entity =>
{
entity.HasIndex(u => u.Email)
.IsUnique();
entity.Property(u => u.Email)
.IsRequired()
.HasMaxLength(256);
entity.Property(u => u.Name)
.IsRequired()
.HasMaxLength(100);
entity.Property(u => u.Bio)
.HasMaxLength(500);
entity.HasMany(u => u.Plants)
.WithOne(p => p.User)
.HasForeignKey(p => p.UserId)
.OnDelete(DeleteBehavior.Cascade);
entity.HasOne(u => u.Preferences)
.WithOne(p => p.User)
.HasForeignKey<UserPreferencesEF>(p => p.UserId)
.OnDelete(DeleteBehavior.Cascade);
entity.HasMany(u => u.ReportsMade)
.WithOne(r => r.ReporterUser)
.HasForeignKey(r => r.ReporterUserId)
.OnDelete(DeleteBehavior.Restrict);
entity.HasMany(u => u.ReportsReceived)
.WithOne(r => r.ReportedUser)
.HasForeignKey(r => r.ReportedUserId)
.OnDelete(DeleteBehavior.Restrict);
entity.HasMany(u => u.SentMessages)
.WithOne(m => m.SenderUser)
.HasForeignKey(m => m.SenderUserId)
.OnDelete(DeleteBehavior.Restrict);
entity.Property(u => u.Location)
.HasColumnType("geography");
});
modelBuilder.Entity<PlantEF>(entity =>
{
entity.Property(p => p.SpeciesName)
.IsRequired()
.HasMaxLength(200);
entity.Property(p => p.Category)
.HasMaxLength(100);
entity.HasOne(p => p.User)
.WithMany(u => u.Plants)
.HasForeignKey(p => p.UserId)
.OnDelete(DeleteBehavior.Cascade);
});
modelBuilder.Entity<SwipeEF>(entity =>
{
entity.HasKey(s => s.SwipeId);
entity.HasOne(s => s.SwiperPlant)
.WithMany()
.HasForeignKey(s => s.SwiperPlantId)
.OnDelete(DeleteBehavior.Restrict);
entity.HasOne(s => s.SwipedPlant)
.WithMany()
.HasForeignKey(s => s.SwipedPlantId)
.OnDelete(DeleteBehavior.Restrict);
entity.HasIndex(s => new { s.SwiperPlantId, s.SwipedPlantId })
.IsUnique();
});
modelBuilder.Entity<MatchEF>(entity =>
{
entity.HasKey(m => m.MatchId);
entity.HasOne(m => m.Plant1)
.WithMany()
.HasForeignKey(m => m.PlantId1)
.OnDelete(DeleteBehavior.Restrict);
entity.HasOne(m => m.Plant2)
.WithMany()
.HasForeignKey(m => m.PlantId2)
.OnDelete(DeleteBehavior.Restrict);
entity.HasOne(m => m.User1)
.WithMany()
.HasForeignKey(m => m.UserId1)
.OnDelete(DeleteBehavior.Restrict);
entity.HasOne(m => m.User2)
.WithMany()
.HasForeignKey(m => m.UserId2)
.OnDelete(DeleteBehavior.Restrict);
entity.HasIndex(m => new { m.PlantId1, m.PlantId2 })
.IsUnique();
entity.HasCheckConstraint("CK_MatchEF_PlantIdOrder", "[PlantId1] < [PlantId2]");
});
modelBuilder.Entity<MessageEF>(entity =>
{
entity.HasKey(m => m.MessageId);
entity.Property(m => m.MessageText)
.IsRequired();
entity.HasOne(m => m.Match)
.WithMany(mt => mt.Messages)
.HasForeignKey(m => m.MatchId)
.OnDelete(DeleteBehavior.Cascade);
entity.HasOne(m => m.SenderUser)
.WithMany(u => u.SentMessages)
.HasForeignKey(m => m.SenderUserId)
.OnDelete(DeleteBehavior.Restrict);
});
modelBuilder.Entity<ReportEF>(entity =>
{
entity.HasKey(r => r.ReportId);
entity.Property(r => r.Reason)
.IsRequired();
entity.HasOne(r => r.ReporterUser)
.WithMany(u => u.ReportsMade)
.HasForeignKey(r => r.ReporterUserId)
.OnDelete(DeleteBehavior.Restrict);
entity.HasOne(r => r.ReportedUser)
.WithMany(u => u.ReportsReceived)
.HasForeignKey(r => r.ReportedUserId)
.OnDelete(DeleteBehavior.Restrict);
});
modelBuilder.Entity<UserPreferencesEF>(entity =>
{
entity.HasKey(p => p.UserId);
entity.HasOne(p => p.User)
.WithOne(u => u.Preferences)
.HasForeignKey<UserPreferencesEF>(p => p.UserId)
.OnDelete(DeleteBehavior.Cascade);
entity.Property(p => p.PreferredCategories)
.HasConversion(
v => v,
v => v);
});
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
{
var clrType = entityType.ClrType;
if (typeof(ICreatedAt).IsAssignableFrom(clrType))
{
modelBuilder.Entity(clrType)
.Property("CreatedAt")
.HasDefaultValueSql("GETUTCDATE()");
}
if (typeof(IUpdatedAt).IsAssignableFrom(clrType))
{
modelBuilder.Entity(clrType)
.Property("UpdatedAt")
.HasDefaultValueSql("GETUTCDATE()");
}
}
}
public override int SaveChanges()
{
UpdateTimestamps();
return base.SaveChanges();
}
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
UpdateTimestamps();
return await base.SaveChangesAsync(cancellationToken);
}
private void UpdateTimestamps()
{
var entries = ChangeTracker.Entries()
.Where(e => e.Entity is IUpdatedAt && (e.State == EntityState.Added || e.State == EntityState.Modified));
foreach (var entry in entries)
{
((IUpdatedAt)entry.Entity).UpdatedAt = DateTime.UtcNow;
if (entry.State == EntityState.Added && entry.Entity is ICreatedAt)
{
((ICreatedAt)entry.Entity).CreatedAt = DateTime.UtcNow;
}
}
}
}
}
//ICreatedAt
namespace Cuttr.Infrastructure.Common
{
public interface ICreatedAt
{
DateTime CreatedAt { get; set; }
}
}
//IUpdatedAt
namespace Cuttr.Infrastructure.Common
{
public interface IUpdatedAt
{
DateTime UpdatedAt { get; set; }
}
}
//MatchEF
namespace Cuttr.Infrastructure.Entities
{
public class MatchEF : ICreatedAt
{
[Key]
public int MatchId { get; set; }
[Required]
public int PlantId1 { get; set; }
[Required]
public int PlantId2 { get; set; }
[Required]
public int UserId1 { get; set; }
[Required]
public int UserId2 { get; set; }
public DateTime CreatedAt { get; set; }
[ForeignKey("PlantId1")]
public virtual PlantEF Plant1 { get; set; }
[ForeignKey("PlantId2")]
public virtual PlantEF Plant2 { get; set; }
[ForeignKey("UserId1")]
public virtual UserEF User1 { get; set; }
[ForeignKey("UserId2")]
public virtual UserEF User2 { get; set; }
public virtual ICollection<MessageEF> Messages { get; set; }
}
}
//MessageEF
namespace Cuttr.Infrastructure.Entities
{
public class MessageEF : ICreatedAt
{
[Key]
public int MessageId { get; set; }
[Required]
public int MatchId { get; set; }
[Required]
public int SenderUserId { get; set; }
[Required]
public string MessageText { get; set; }
public DateTime CreatedAt { get; set; }
public bool IsRead { get; set; }
[ForeignKey("MatchId")]
public virtual MatchEF Match { get; set; }
[ForeignKey("SenderUserId")]
public virtual UserEF SenderUser { get; set; }
}
}
//PlantEF
namespace Cuttr.Infrastructure.Entities
{
public class PlantEF : ICreatedAt, IUpdatedAt
{
[Key]
public int PlantId { get; set; }
[Required]
public int UserId { get; set; }
[Required]
[MaxLength(200)]
public string SpeciesName { get; set; }
public string Description { get; set; }
[Required]
[MaxLength(50)]
public string PlantStage { get; set; }
[Required]
[MaxLength(50)]
public string PlantCategory { get; set; }
[Required]
[MaxLength(50)]
public string WateringNeed { get; set; }
[Required]
[MaxLength(50)]
public string LightRequirement { get; set; }
[MaxLength(50)]
public string Size { get; set; }
[MaxLength(50)]
public string IndoorOutdoor { get; set; }
[MaxLength(50)]
public string PropagationEase { get; set; }
[MaxLength(50)]
public string PetFriendly { get; set; }
public string Extras { get; set; }
public string ImageUrl { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }
[ForeignKey("UserId")]
public virtual UserEF User { get; set; }
}
}
//ReportEF
namespace Cuttr.Infrastructure.Entities
{
public class ReportEF : ICreatedAt
{
[Key]
public int ReportId { get; set; }
[Required]
public int ReporterUserId { get; set; }
[Required]
public int ReportedUserId { get; set; }
[Required]
public string Reason { get; set; }
public string Comments { get; set; }
public DateTime CreatedAt { get; set; }
public bool IsResolved { get; set; }
[ForeignKey("ReporterUserId")]
public virtual UserEF ReporterUser { get; set; }
[ForeignKey("ReportedUserId")]
public virtual UserEF ReportedUser { get; set; }
}
}
//SwipeEF
namespace Cuttr.Infrastructure.Entities
{
public class SwipeEF : ICreatedAt
{
[Key]
public int SwipeId { get; set; }
[Required]
public int SwiperPlantId { get; set; }
[Required]
public int SwipedPlantId { get; set; }
[Required]
public bool IsLike { get; set; }
public DateTime CreatedAt { get; set; }
[ForeignKey("SwiperPlantId")]
public virtual PlantEF SwiperPlant { get; set; }
[ForeignKey("SwipedPlantId")]
public virtual PlantEF SwipedPlant { get; set; }
}
}
//UserEF
namespace Cuttr.Infrastructure.Entities
{
public class UserEF : ICreatedAt, IUpdatedAt
{
[Key]
public int UserId { get; set; }
[Required]
[EmailAddress]
[MaxLength(256)]
public string Email { get; set; }
[Required]
public string PasswordHash { get; set; }
[Required]
[MaxLength(100)]
public string Name { get; set; }
public string ProfilePictureUrl { get; set; }
[MaxLength(500)]
public string Bio { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }
public Point Location { get; set; }
public virtual ICollection<PlantEF> Plants { get; set; }
public virtual UserPreferencesEF Preferences { get; set; }
public virtual ICollection<MessageEF> SentMessages { get; set; }
public virtual ICollection<ReportEF> ReportsMade { get; set; }
public virtual ICollection<ReportEF> ReportsReceived { get; set; }
}
}
//UserPreferencesEF
namespace Cuttr.Infrastructure.Entities
{
public class UserPreferencesEF
{
[Key, ForeignKey("User")]
public int UserId { get; set; }
public int SearchRadius { get; set; }
public List<string> PreferedPlantStage { get; set; }
public List<string> PreferedPlantCategory { get; set; }
public List<string> PreferedWateringNeed { get; set; }
public List<string> PreferedLightRequirement { get; set; }
public List<string> PreferedSize { get; set; }
public List<string> PreferedIndoorOutdoor { get; set; }
public List<string> PreferedPropagationEase { get; set; }
public List<string> PreferedPetFriendly { get; set; }
public List<string> PreferedExtras { get; set; }
public virtual UserEF User { get; set; }
}
}
//RepositoryException
namespace Cuttr.Infrastructure.Exceptions
{
public class RepositoryException : Exception
{
public RepositoryException() { }
public RepositoryException(string message)
: base(message) { }
public RepositoryException(string message, Exception innerException)
: base(message, innerException) { }
}
}
//BusinessToEFMapper
namespace Cuttr.Infrastructure.Mappers
{
public static class BusinessToEFMapper
{
public static UserEF MapToUserEF(User user)
{
if (user == null)
return null;
return new UserEF
{
UserId = user.UserId,
Email = user.Email,
PasswordHash = user.PasswordHash,
Name = user.Name,
ProfilePictureUrl = user.ProfilePictureUrl,
Bio = user.Bio,
Plants = user.Plants?.Select(MapToPlantEFWithoutUser).ToList(),
Preferences = MapToUserPreferencesEF(user.Preferences),
};
}
public static PlantEF MapToPlantEF(Plant plant)
{
if (plant == null)
return null;
return new PlantEF
{
PlantId = plant.PlantId,
UserId = plant.UserId,
SpeciesName = plant.SpeciesName,
Description = plant.Description,
PlantStage = plant.PlantStage.ToString(),
PlantCategory = plant.PlantCategory.ToString(),
WateringNeed = plant.WateringNeed.ToString(),
LightRequirement = plant.LightRequirement.ToString(),
Size = plant.Size.ToString(),
IndoorOutdoor = plant.IndoorOutdoor.ToString(),
PropagationEase = plant.PropagationEase.ToString(),
PetFriendly = plant.PetFriendly.ToString(),
Extras = plant.Extras != null ? SerializeExtras(plant.Extras) : null,
ImageUrl = plant.ImageUrl,
User = MapToUserEFWithoutPlants(plant.User),
};
}
private static PlantEF MapToPlantEFWithoutUser(Plant plant)
{
if (plant == null)
return null;
return new PlantEF
{
PlantId = plant.PlantId,
UserId = plant.UserId,
SpeciesName = plant.SpeciesName,
Description = plant.Description,
PlantStage = plant.PlantStage.ToString(),
PlantCategory = plant.PlantCategory.ToString(),
WateringNeed = plant.WateringNeed.ToString(),
LightRequirement = plant.LightRequirement.ToString(),
Size = plant.Size.ToString(),
IndoorOutdoor = plant.IndoorOutdoor.ToString(),
PropagationEase = plant.PropagationEase.ToString(),
PetFriendly = plant.PetFriendly.ToString(),
Extras = plant.Extras != null ? SerializeExtras(plant.Extras) : null,
ImageUrl = plant.ImageUrl,
};
}
private static UserEF MapToUserEFWithoutPlants(User user)
{
if (user == null)
return null;
return new UserEF
{
UserId = user.UserId,
Email = user.Email,
PasswordHash = user.PasswordHash,
Name = user.Name,
ProfilePictureUrl = user.ProfilePictureUrl,
Bio = user.Bio,
Preferences = MapToUserPreferencesEF(user.Preferences),
};
}
public static SwipeEF MapToSwipeEF(Swipe swipe)
{
if (swipe == null)
return null;
return new SwipeEF
{
SwipeId = swipe.SwipeId,
SwiperPlantId = swipe.SwiperPlantId,
SwipedPlantId = swipe.SwipedPlantId,
IsLike = swipe.IsLike,
SwiperPlant = MapToPlantEFWithoutUser(swipe.SwiperPlant),
SwipedPlant = MapToPlantEFWithoutUser(swipe.SwipedPlant),
};
}
public static MatchEF MapToMatchEF(Match match)
{
if (match == null)
return null;
return new MatchEF
{
MatchId = match.MatchId,
PlantId1 = match.PlantId1,
PlantId2 = match.PlantId2,
UserId1 = match.UserId1,
UserId2 = match.UserId2,
Plant1 = MapToPlantEFWithoutUser(match.Plant1),
Plant2 = MapToPlantEFWithoutUser(match.Plant2),
User1 = MapToUserEFWithoutPlants(match.User1),
User2 = MapToUserEFWithoutPlants(match.User2),
Messages = match.Messages?.Select(MapToMessageEF).ToList(),
CreatedAt = match.CreatedAt,
};
}
public static MessageEF MapToMessageEF(Message message)
{
if (message == null)
return null;
return new MessageEF
{
MessageId = message.MessageId,
MatchId = message.MatchId,
SenderUserId = message.SenderUserId,
MessageText = message.MessageText,
IsRead = message.IsRead,
CreatedAt = message.SentAt,
SenderUser = MapToUserEFWithoutPlants(message.SenderUser),
};
}
public static ReportEF MapToReportEF(Report report)
{
if (report == null)
return null;
return new ReportEF
{
ReportId = report.ReportId,
ReporterUserId = report.ReporterUserId,
ReportedUserId = report.ReportedUserId,
Reason = report.Reason,
Comments = report.Comments,
IsResolved = report.IsResolved,
ReporterUser = MapToUserEFWithoutPlants(report.ReporterUser),
ReportedUser = MapToUserEFWithoutPlants(report.ReportedUser),
CreatedAt = report.CreatedAt,
};
}
public static UserPreferencesEF MapToUserPreferencesEF(UserPreferences preferences)
{
if (preferences == null)
return null;
return new UserPreferencesEF
{
UserId = preferences.UserId,
SearchRadius = preferences.SearchRadius,
PreferredCategories = SerializeCategories(preferences.PreferredCategories),
};
}
public static string SerializeCategories(List<string> categories)
{
if (categories == null || !categories.Any())
return null;
return System.Text.Json.JsonSerializer.Serialize(categories);
}
public static string SerializeExtras(List<Extras> extras)
{
if (extras == null || !extras.Any())
return null;
return System.Text.Json.JsonSerializer.Serialize(extras);
}
}
}
//EFToBusinessMapper
namespace Cuttr.Infrastructure.Mappers
{
public static class EFToBusinessMapper
{
public static User MapToUser(UserEF efUser)
{
if (efUser == null)
return null;
return new User
{
UserId = efUser.UserId,
Email = efUser.Email,
PasswordHash = efUser.PasswordHash,
Name = efUser.Name,
ProfilePictureUrl = efUser.ProfilePictureUrl,
Bio = efUser.Bio,
LocationLatitude = efUser.Location?.Y,
LocationLongitude = efUser.Location?.X,
Plants = efUser.Plants?.Select(MapToPlantWithoutUser).ToList(),
Preferences = MapToUserPreferences(efUser.Preferences),
};
}
public static Plant MapToPlant(PlantEF efPlant)
{
if (efPlant == null)
return null;
return new Plant
{
PlantId = efPlant.PlantId,
UserId = efPlant.UserId,
SpeciesName = efPlant.SpeciesName,
Description = efPlant.Description,
PlantStage = Enum.Parse<PlantStage>(efPlant.PlantStage),
PlantCategory = Enum.Parse<PlantCategory>(efPlant.PlantCategory),
WateringNeed = Enum.Parse<WateringNeed>(efPlant.WateringNeed),
LightRequirement = Enum.Parse<LightRequirement>(efPlant.LightRequirement),
Size = Enum.Parse<Size>(efPlant.Size),
IndoorOutdoor = Enum.Parse<IndoorOutdoor>(efPlant.IndoorOutdoor),
PropagationEase = Enum.Parse<PropagationEase>(efPlant.PropagationEase),
PetFriendly = Enum.Parse<PetFriendly>(efPlant.PetFriendly),
Extras = efPlant.Extras != null ? DeserializeExtras(efPlant.Extras) : null,
ImageUrl = efPlant.ImageUrl,
User = MapToUserWithoutPlants(efPlant.User),
};
}
private static Plant MapToPlantWithoutUser(PlantEF efPlant)
{
if (efPlant == null)
return null;
return new Plant
{
PlantId = efPlant.PlantId,
UserId = efPlant.UserId,
SpeciesName = efPlant.SpeciesName,
Description = efPlant.Description,
PlantStage = Enum.Parse<PlantStage>(efPlant.PlantStage),
PlantCategory = Enum.Parse<PlantCategory>(efPlant.PlantCategory),
WateringNeed = Enum.Parse<WateringNeed>(efPlant.WateringNeed),
LightRequirement = Enum.Parse<LightRequirement>(efPlant.LightRequirement),
Size = Enum.Parse<Size>(efPlant.Size),
IndoorOutdoor = Enum.Parse<IndoorOutdoor>(efPlant.IndoorOutdoor),
PropagationEase = Enum.Parse<PropagationEase>(efPlant.PropagationEase),
PetFriendly = Enum.Parse<PetFriendly>(efPlant.PetFriendly),
Extras = efPlant.Extras != null ? DeserializeExtras(efPlant.Extras) : null,
ImageUrl = efPlant.ImageUrl,
};
}
private static User MapToUserWithoutPlants(UserEF efUser)
{
if (efUser == null)
return null;
return new User
{
UserId = efUser.UserId,
Email = efUser.Email,
PasswordHash = efUser.PasswordHash,
Name = efUser.Name,
ProfilePictureUrl = efUser.ProfilePictureUrl,
Bio = efUser.Bio,
Preferences = MapToUserPreferences(efUser.Preferences),
};
}
public static Swipe MapToSwipe(SwipeEF efSwipe)
{
if (efSwipe == null)
return null;
return new Swipe
{
SwipeId = efSwipe.SwipeId,
SwiperPlantId = efSwipe.SwiperPlantId,
SwipedPlantId = efSwipe.SwipedPlantId,
IsLike = efSwipe.IsLike,
SwiperPlant = MapToPlantWithoutUser(efSwipe.SwiperPlant),
SwipedPlant = MapToPlantWithoutUser(efSwipe.SwipedPlant),
};
}
public static Match MapToMatch(MatchEF efMatch)
{
if (efMatch == null)
return null;
return new Match
{
MatchId = efMatch.MatchId,
PlantId1 = efMatch.PlantId1,
PlantId2 = efMatch.PlantId2,
UserId1 = efMatch.UserId1,
UserId2 = efMatch.UserId2,
Plant1 = MapToPlantWithoutUser(efMatch.Plant1),
Plant2 = MapToPlantWithoutUser(efMatch.Plant2),
User1 = MapToUserWithoutPlants(efMatch.User1),
User2 = MapToUserWithoutPlants(efMatch.User2),
Messages = efMatch.Messages?.Select(MapToMessage).ToList(),
CreatedAt = efMatch.CreatedAt,
};
}
public static Message MapToMessage(MessageEF efMessage)
{
if (efMessage == null)
return null;
return new Message
{
MessageId = efMessage.MessageId,
MatchId = efMessage.MatchId,
SenderUserId = efMessage.SenderUserId,
MessageText = efMessage.MessageText,
IsRead = efMessage.IsRead,
SentAt = efMessage.CreatedAt,
SenderUser = MapToUserWithoutPlants(efMessage.SenderUser),
};
}
public static Report MapToReport(ReportEF efReport)
{
if (efReport == null)
return null;
return new Report
{
ReportId = efReport.ReportId,
ReporterUserId = efReport.ReporterUserId,
ReportedUserId = efReport.ReportedUserId,
Reason = efReport.Reason,
Comments = efReport.Comments,
IsResolved = efReport.IsResolved,
ReporterUser = MapToUserWithoutPlants(efReport.ReporterUser),
ReportedUser = MapToUserWithoutPlants(efReport.ReportedUser),
CreatedAt = efReport.CreatedAt,
};
}
public static UserPreferences MapToUserPreferences(UserPreferencesEF efPreferences)
{
if (efPreferences == null)
return null;
return new UserPreferences
{
UserId = efPreferences.UserId,
SearchRadius = efPreferences.SearchRadius,
PreferredCategories = DeserializeCategories(efPreferences.PreferredCategories),
};
}
private static List<string> DeserializeCategories(string serializedCategories)
{
if (string.IsNullOrEmpty(serializedCategories))
return new List<string>();
return System.Text.Json.JsonSerializer.Deserialize<List<string>>(serializedCategories);
}
private static List<Extras> DeserializeExtras(string serializedExtras)
{
if (string.IsNullOrEmpty(serializedExtras))
return new List<Extras>();
return System.Text.Json.JsonSerializer.Deserialize<List<Extras>>(serializedExtras);
}
}
}
//20241205083810_InitialCreate
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
public partial class InitialCreate : Migration
{
protected override void Up(MigrationBuilder migrationBuilder)
{
migrationBuilder.CreateTable(
name: "Users",
columns: table => new
{
UserId = table.Column<int>(type: "int", nullable: false)
.Annotation("SqlServer:Identity", "1, 1"),
Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
ProfilePictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
LocationLatitude = table.Column<double>(type: "float", nullable: true),
LocationLongitude = table.Column<double>(type: "float", nullable: true),
CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
},
constraints: table =>
{
table.PrimaryKey("PK_Users", x => x.UserId);
});
migrationBuilder.CreateTable(
name: "Plants",
columns: table => new
{
PlantId = table.Column<int>(type: "int", nullable: false)
.Annotation("SqlServer:Identity", "1, 1"),
UserId = table.Column<int>(type: "int", nullable: false),
SpeciesName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
CareRequirements = table.Column<string>(type: "nvarchar(max)", nullable: false),
Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
},
constraints: table =>
{
table.PrimaryKey("PK_Plants", x => x.PlantId);
table.ForeignKey(
name: "FK_Plants_Users_UserId",
column: x => x.UserId,
principalTable: "Users",
principalColumn: "UserId",
onDelete: ReferentialAction.Cascade);
});
migrationBuilder.CreateTable(
name: "Reports",
columns: table => new
{
ReportId = table.Column<int>(type: "int", nullable: false)
.Annotation("SqlServer:Identity", "1, 1"),
ReporterUserId = table.Column<int>(type: "int", nullable: false),
ReportedUserId = table.Column<int>(type: "int", nullable: false),
Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
IsResolved = table.Column<bool>(type: "bit", nullable: false)
},
constraints: table =>
{
table.PrimaryKey("PK_Reports", x => x.ReportId);
table.ForeignKey(
name: "FK_Reports_Users_ReportedUserId",
column: x => x.ReportedUserId,
principalTable: "Users",
principalColumn: "UserId",
onDelete: ReferentialAction.Restrict);
table.ForeignKey(
name: "FK_Reports_Users_ReporterUserId",
column: x => x.ReporterUserId,
principalTable: "Users",
principalColumn: "UserId",
onDelete: ReferentialAction.Restrict);
});
migrationBuilder.CreateTable(
name: "UserPreferences",
columns: table => new
{
UserId = table.Column<int>(type: "int", nullable: false),
SearchRadius = table.Column<double>(type: "float", nullable: false),
PreferredCategories = table.Column<string>(type: "nvarchar(max)", nullable: false)
},
constraints: table =>
{
table.PrimaryKey("PK_UserPreferences", x => x.UserId);
table.ForeignKey(
name: "FK_UserPreferences_Users_UserId",
column: x => x.UserId,
principalTable: "Users",
principalColumn: "UserId",
onDelete: ReferentialAction.Cascade);
});
migrationBuilder.CreateTable(
name: "Matches",
columns: table => new
{
MatchId = table.Column<int>(type: "int", nullable: false)
.Annotation("SqlServer:Identity", "1, 1"),
PlantId1 = table.Column<int>(type: "int", nullable: false),
PlantId2 = table.Column<int>(type: "int", nullable: false),
UserId1 = table.Column<int>(type: "int", nullable: false),
UserId2 = table.Column<int>(type: "int", nullable: false),
CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
},
constraints: table =>
{
table.PrimaryKey("PK_Matches", x => x.MatchId);
table.CheckConstraint("CK_MatchEF_PlantIdOrder", "[PlantId1] < [PlantId2]");
table.ForeignKey(
name: "FK_Matches_Plants_PlantId1",
column: x => x.PlantId1,
principalTable: "Plants",
principalColumn: "PlantId",
onDelete: ReferentialAction.Restrict);
table.ForeignKey(
name: "FK_Matches_Plants_PlantId2",
column: x => x.PlantId2,
principalTable: "Plants",
principalColumn: "PlantId",
onDelete: ReferentialAction.Restrict);
table.ForeignKey(
name: "FK_Matches_Users_UserId1",
column: x => x.UserId1,
principalTable: "Users",
principalColumn: "UserId",
onDelete: ReferentialAction.Restrict);
table.ForeignKey(
name: "FK_Matches_Users_UserId2",
column: x => x.UserId2,
principalTable: "Users",
principalColumn: "UserId",
onDelete: ReferentialAction.Restrict);
});
migrationBuilder.CreateTable(
name: "Swipes",
columns: table => new
{
SwipeId = table.Column<int>(type: "int", nullable: false)
.Annotation("SqlServer:Identity", "1, 1"),
SwiperPlantId = table.Column<int>(type: "int", nullable: false),
SwipedPlantId = table.Column<int>(type: "int", nullable: false),
IsLike = table.Column<bool>(type: "bit", nullable: false),
CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
},
constraints: table =>
{
table.PrimaryKey("PK_Swipes", x => x.SwipeId);
table.ForeignKey(
name: "FK_Swipes_Plants_SwipedPlantId",
column: x => x.SwipedPlantId,
principalTable: "Plants",
principalColumn: "PlantId",
onDelete: ReferentialAction.Restrict);
table.ForeignKey(
name: "FK_Swipes_Plants_SwiperPlantId",
column: x => x.SwiperPlantId,
principalTable: "Plants",
principalColumn: "PlantId",
onDelete: ReferentialAction.Restrict);
});
migrationBuilder.CreateTable(
name: "Messages",
columns: table => new
{
MessageId = table.Column<int>(type: "int", nullable: false)
.Annotation("SqlServer:Identity", "1, 1"),
MatchId = table.Column<int>(type: "int", nullable: false),
SenderUserId = table.Column<int>(type: "int", nullable: false),
MessageText = table.Column<string>(type: "nvarchar(max)", nullable: false),
CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
IsRead = table.Column<bool>(type: "bit", nullable: false)
},
constraints: table =>
{
table.PrimaryKey("PK_Messages", x => x.MessageId);
table.ForeignKey(
name: "FK_Messages_Matches_MatchId",
column: x => x.MatchId,
principalTable: "Matches",
principalColumn: "MatchId",
onDelete: ReferentialAction.Cascade);
table.ForeignKey(
name: "FK_Messages_Users_SenderUserId",
column: x => x.SenderUserId,
principalTable: "Users",
principalColumn: "UserId",
onDelete: ReferentialAction.Restrict);
});
migrationBuilder.CreateIndex(
name: "IX_Matches_PlantId1_PlantId2",
table: "Matches",
columns: new[] { "PlantId1", "PlantId2" },
unique: true);
migrationBuilder.CreateIndex(
name: "IX_Matches_PlantId2",
table: "Matches",
column: "PlantId2");
migrationBuilder.CreateIndex(
name: "IX_Matches_UserId1",
table: "Matches",
column: "UserId1");
migrationBuilder.CreateIndex(
name: "IX_Matches_UserId2",
table: "Matches",
column: "UserId2");
migrationBuilder.CreateIndex(
name: "IX_Messages_MatchId",
table: "Messages",
column: "MatchId");
migrationBuilder.CreateIndex(
name: "IX_Messages_SenderUserId",
table: "Messages",
column: "SenderUserId");
migrationBuilder.CreateIndex(
name: "IX_Plants_UserId",
table: "Plants",
column: "UserId");
migrationBuilder.CreateIndex(
name: "IX_Reports_ReportedUserId",
table: "Reports",
column: "ReportedUserId");
migrationBuilder.CreateIndex(
name: "IX_Reports_ReporterUserId",
table: "Reports",
column: "ReporterUserId");
migrationBuilder.CreateIndex(
name: "IX_Swipes_SwipedPlantId",
table: "Swipes",
column: "SwipedPlantId");
migrationBuilder.CreateIndex(
name: "IX_Swipes_SwiperPlantId_SwipedPlantId",
table: "Swipes",
columns: new[] { "SwiperPlantId", "SwipedPlantId" },
unique: true);
migrationBuilder.CreateIndex(
name: "IX_Users_Email",
table: "Users",
column: "Email",
unique: true);
migrationBuilder.CreateIndex(
name: "IX_Users_LocationLatitude_LocationLongitude",
table: "Users",
columns: new[] { "LocationLatitude", "LocationLongitude" });
}
protected override void Down(MigrationBuilder migrationBuilder)
{
migrationBuilder.DropTable(
name: "Messages");
migrationBuilder.DropTable(
name: "Reports");
migrationBuilder.DropTable(
name: "Swipes");
migrationBuilder.DropTable(
name: "UserPreferences");
migrationBuilder.DropTable(
name: "Matches");
migrationBuilder.DropTable(
name: "Plants");
migrationBuilder.DropTable(
name: "Users");
}
}
}
//20241205083810_InitialCreate.Designer
﻿
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
[DbContext(typeof(CuttrDbContext))]
[Migration("20241205083810_InitialCreate")]
partial class InitialCreate
{
protected override void BuildTargetModel(ModelBuilder modelBuilder)
{
#pragma warning disable 612, 618
modelBuilder
.HasAnnotation("ProductVersion", "8.0.11")
.HasAnnotation("Relational:MaxIdentifierLength", 128);
SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MatchEF", b =>
{
b.Property<int>("MatchId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MatchId"));
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<int>("PlantId1")
.HasColumnType("int");
b.Property<int>("PlantId2")
.HasColumnType("int");
b.Property<int>("UserId1")
.HasColumnType("int");
b.Property<int>("UserId2")
.HasColumnType("int");
b.HasKey("MatchId");
b.HasIndex("PlantId2");
b.HasIndex("UserId1");
b.HasIndex("UserId2");
b.HasIndex("PlantId1", "PlantId2")
.IsUnique();
b.ToTable("Matches", t =>
{
t.HasCheckConstraint("CK_MatchEF_PlantIdOrder", "[PlantId1] < [PlantId2]");
});
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MessageEF", b =>
{
b.Property<int>("MessageId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MessageId"));
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<bool>("IsRead")
.HasColumnType("bit");
b.Property<int>("MatchId")
.HasColumnType("int");
b.Property<string>("MessageText")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("SenderUserId")
.HasColumnType("int");
b.HasKey("MessageId");
b.HasIndex("MatchId");
b.HasIndex("SenderUserId");
b.ToTable("Messages");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.PlantEF", b =>
{
b.Property<int>("PlantId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PlantId"));
b.Property<string>("CareRequirements")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("Category")
.IsRequired()
.HasMaxLength(100)
.HasColumnType("nvarchar(100)");
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Description")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ImageUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("SpeciesName")
.IsRequired()
.HasMaxLength(200)
.HasColumnType("nvarchar(200)");
b.Property<DateTime>("UpdatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<int>("UserId")
.HasColumnType("int");
b.HasKey("PlantId");
b.HasIndex("UserId");
b.ToTable("Plants");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.ReportEF", b =>
{
b.Property<int>("ReportId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ReportId"));
b.Property<string>("Comments")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<bool>("IsResolved")
.HasColumnType("bit");
b.Property<string>("Reason")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("ReportedUserId")
.HasColumnType("int");
b.Property<int>("ReporterUserId")
.HasColumnType("int");
b.HasKey("ReportId");
b.HasIndex("ReportedUserId");
b.HasIndex("ReporterUserId");
b.ToTable("Reports");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.SwipeEF", b =>
{
b.Property<int>("SwipeId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SwipeId"));
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<bool>("IsLike")
.HasColumnType("bit");
b.Property<int>("SwipedPlantId")
.HasColumnType("int");
b.Property<int>("SwiperPlantId")
.HasColumnType("int");
b.HasKey("SwipeId");
b.HasIndex("SwipedPlantId");
b.HasIndex("SwiperPlantId", "SwipedPlantId")
.IsUnique();
b.ToTable("Swipes");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserEF", b =>
{
b.Property<int>("UserId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));
b.Property<string>("Bio")
.IsRequired()
.HasMaxLength(500)
.HasColumnType("nvarchar(500)");
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Email")
.IsRequired()
.HasMaxLength(256)
.HasColumnType("nvarchar(256)");
b.Property<double?>("LocationLatitude")
.HasColumnType("float");
b.Property<double?>("LocationLongitude")
.HasColumnType("float");
b.Property<string>("Name")
.IsRequired()
.HasMaxLength(100)
.HasColumnType("nvarchar(100)");
b.Property<string>("PasswordHash")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ProfilePictureUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<DateTime>("UpdatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.HasKey("UserId");
b.HasIndex("Email")
.IsUnique();
b.HasIndex("LocationLatitude", "LocationLongitude");
b.ToTable("Users");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.Property<int>("UserId")
.HasColumnType("int");
b.Property<string>("PreferredCategories")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<double>("SearchRadius")
.HasColumnType("float");
b.HasKey("UserId");
b.ToTable("UserPreferences");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MatchEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "Plant1")
.WithMany()
.HasForeignKey("PlantId1")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "Plant2")
.WithMany()
.HasForeignKey("PlantId2")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User1")
.WithMany()
.HasForeignKey("UserId1")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User2")
.WithMany()
.HasForeignKey("UserId2")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("Plant1");
b.Navigation("Plant2");
b.Navigation("User1");
b.Navigation("User2");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MessageEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.MatchEF", "Match")
.WithMany("Messages")
.HasForeignKey("MatchId")
.OnDelete(DeleteBehavior.Cascade)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "SenderUser")
.WithMany("SentMessages")
.HasForeignKey("SenderUserId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("Match");
b.Navigation("SenderUser");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.PlantEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User")
.WithMany("Plants")
.HasForeignKey("UserId")
.OnDelete(DeleteBehavior.Cascade)
.IsRequired();
b.Navigation("User");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.ReportEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "ReportedUser")
.WithMany("ReportsReceived")
.HasForeignKey("ReportedUserId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "ReporterUser")
.WithMany("ReportsMade")
.HasForeignKey("ReporterUserId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("ReportedUser");
b.Navigation("ReporterUser");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.SwipeEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "SwipedPlant")
.WithMany()
.HasForeignKey("SwipedPlantId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "SwiperPlant")
.WithMany()
.HasForeignKey("SwiperPlantId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("SwipedPlant");
b.Navigation("SwiperPlant");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User")
.WithOne("Preferences")
.HasForeignKey("Cuttr.Infrastructure.Entities.UserPreferencesEF", "UserId")
.OnDelete(DeleteBehavior.Cascade)
.IsRequired();
b.Navigation("User");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MatchEF", b =>
{
b.Navigation("Messages");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserEF", b =>
{
b.Navigation("Plants");
b.Navigation("Preferences")
.IsRequired();
b.Navigation("ReportsMade");
b.Navigation("ReportsReceived");
b.Navigation("SentMessages");
});
#pragma warning restore 612, 618
}
}
}
//20241211100233_AddUserLocationGeographyColumn
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
public partial class AddUserLocationGeographyColumn : Migration
{
protected override void Up(MigrationBuilder migrationBuilder)
{
migrationBuilder.AddColumn<Point>(
name: "Location",
table: "Users",
type: "geography",
nullable: false);
}
protected override void Down(MigrationBuilder migrationBuilder)
{
migrationBuilder.DropColumn(
name: "Location",
table: "Users");
}
}
}
//20241211100233_AddUserLocationGeographyColumn.Designer
﻿
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
[DbContext(typeof(CuttrDbContext))]
[Migration("20241211100233_AddUserLocationGeographyColumn")]
partial class AddUserLocationGeographyColumn
{
protected override void BuildTargetModel(ModelBuilder modelBuilder)
{
#pragma warning disable 612, 618
modelBuilder
.HasAnnotation("ProductVersion", "9.0.0")
.HasAnnotation("Relational:MaxIdentifierLength", 128);
SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MatchEF", b =>
{
b.Property<int>("MatchId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MatchId"));
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<int>("PlantId1")
.HasColumnType("int");
b.Property<int>("PlantId2")
.HasColumnType("int");
b.Property<int>("UserId1")
.HasColumnType("int");
b.Property<int>("UserId2")
.HasColumnType("int");
b.HasKey("MatchId");
b.HasIndex("PlantId2");
b.HasIndex("UserId1");
b.HasIndex("UserId2");
b.HasIndex("PlantId1", "PlantId2")
.IsUnique();
b.ToTable("Matches", t =>
{
t.HasCheckConstraint("CK_MatchEF_PlantIdOrder", "[PlantId1] < [PlantId2]");
});
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MessageEF", b =>
{
b.Property<int>("MessageId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MessageId"));
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<bool>("IsRead")
.HasColumnType("bit");
b.Property<int>("MatchId")
.HasColumnType("int");
b.Property<string>("MessageText")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("SenderUserId")
.HasColumnType("int");
b.HasKey("MessageId");
b.HasIndex("MatchId");
b.HasIndex("SenderUserId");
b.ToTable("Messages");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.PlantEF", b =>
{
b.Property<int>("PlantId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PlantId"));
b.Property<string>("CareRequirements")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("Category")
.IsRequired()
.HasMaxLength(100)
.HasColumnType("nvarchar(100)");
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Description")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ImageUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("SpeciesName")
.IsRequired()
.HasMaxLength(200)
.HasColumnType("nvarchar(200)");
b.Property<DateTime>("UpdatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<int>("UserId")
.HasColumnType("int");
b.HasKey("PlantId");
b.HasIndex("UserId");
b.ToTable("Plants");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.ReportEF", b =>
{
b.Property<int>("ReportId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ReportId"));
b.Property<string>("Comments")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<bool>("IsResolved")
.HasColumnType("bit");
b.Property<string>("Reason")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("ReportedUserId")
.HasColumnType("int");
b.Property<int>("ReporterUserId")
.HasColumnType("int");
b.HasKey("ReportId");
b.HasIndex("ReportedUserId");
b.HasIndex("ReporterUserId");
b.ToTable("Reports");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.SwipeEF", b =>
{
b.Property<int>("SwipeId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SwipeId"));
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<bool>("IsLike")
.HasColumnType("bit");
b.Property<int>("SwipedPlantId")
.HasColumnType("int");
b.Property<int>("SwiperPlantId")
.HasColumnType("int");
b.HasKey("SwipeId");
b.HasIndex("SwipedPlantId");
b.HasIndex("SwiperPlantId", "SwipedPlantId")
.IsUnique();
b.ToTable("Swipes");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserEF", b =>
{
b.Property<int>("UserId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));
b.Property<string>("Bio")
.IsRequired()
.HasMaxLength(500)
.HasColumnType("nvarchar(500)");
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Email")
.IsRequired()
.HasMaxLength(256)
.HasColumnType("nvarchar(256)");
b.Property<Point>("Location")
.IsRequired()
.HasColumnType("geography");
b.Property<double?>("LocationLatitude")
.HasColumnType("float");
b.Property<double?>("LocationLongitude")
.HasColumnType("float");
b.Property<string>("Name")
.IsRequired()
.HasMaxLength(100)
.HasColumnType("nvarchar(100)");
b.Property<string>("PasswordHash")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ProfilePictureUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<DateTime>("UpdatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.HasKey("UserId");
b.HasIndex("Email")
.IsUnique();
b.HasIndex("LocationLatitude", "LocationLongitude");
b.ToTable("Users");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.Property<int>("UserId")
.HasColumnType("int");
b.Property<string>("PreferredCategories")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<double>("SearchRadius")
.HasColumnType("float");
b.HasKey("UserId");
b.ToTable("UserPreferences");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MatchEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "Plant1")
.WithMany()
.HasForeignKey("PlantId1")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "Plant2")
.WithMany()
.HasForeignKey("PlantId2")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User1")
.WithMany()
.HasForeignKey("UserId1")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User2")
.WithMany()
.HasForeignKey("UserId2")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("Plant1");
b.Navigation("Plant2");
b.Navigation("User1");
b.Navigation("User2");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MessageEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.MatchEF", "Match")
.WithMany("Messages")
.HasForeignKey("MatchId")
.OnDelete(DeleteBehavior.Cascade)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "SenderUser")
.WithMany("SentMessages")
.HasForeignKey("SenderUserId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("Match");
b.Navigation("SenderUser");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.PlantEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User")
.WithMany("Plants")
.HasForeignKey("UserId")
.OnDelete(DeleteBehavior.Cascade)
.IsRequired();
b.Navigation("User");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.ReportEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "ReportedUser")
.WithMany("ReportsReceived")
.HasForeignKey("ReportedUserId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "ReporterUser")
.WithMany("ReportsMade")
.HasForeignKey("ReporterUserId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("ReportedUser");
b.Navigation("ReporterUser");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.SwipeEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "SwipedPlant")
.WithMany()
.HasForeignKey("SwipedPlantId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "SwiperPlant")
.WithMany()
.HasForeignKey("SwiperPlantId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("SwipedPlant");
b.Navigation("SwiperPlant");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User")
.WithOne("Preferences")
.HasForeignKey("Cuttr.Infrastructure.Entities.UserPreferencesEF", "UserId")
.OnDelete(DeleteBehavior.Cascade)
.IsRequired();
b.Navigation("User");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MatchEF", b =>
{
b.Navigation("Messages");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserEF", b =>
{
b.Navigation("Plants");
b.Navigation("Preferences")
.IsRequired();
b.Navigation("ReportsMade");
b.Navigation("ReportsReceived");
b.Navigation("SentMessages");
});
#pragma warning restore 612, 618
}
}
}
//CuttrDbContextModelSnapshot
﻿
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
[DbContext(typeof(CuttrDbContext))]
partial class CuttrDbContextModelSnapshot : ModelSnapshot
{
protected override void BuildModel(ModelBuilder modelBuilder)
{
#pragma warning disable 612, 618
modelBuilder
.HasAnnotation("ProductVersion", "9.0.0")
.HasAnnotation("Relational:MaxIdentifierLength", 128);
SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MatchEF", b =>
{
b.Property<int>("MatchId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MatchId"));
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<int>("PlantId1")
.HasColumnType("int");
b.Property<int>("PlantId2")
.HasColumnType("int");
b.Property<int>("UserId1")
.HasColumnType("int");
b.Property<int>("UserId2")
.HasColumnType("int");
b.HasKey("MatchId");
b.HasIndex("PlantId2");
b.HasIndex("UserId1");
b.HasIndex("UserId2");
b.HasIndex("PlantId1", "PlantId2")
.IsUnique();
b.ToTable("Matches", t =>
{
t.HasCheckConstraint("CK_MatchEF_PlantIdOrder", "[PlantId1] < [PlantId2]");
});
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MessageEF", b =>
{
b.Property<int>("MessageId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MessageId"));
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<bool>("IsRead")
.HasColumnType("bit");
b.Property<int>("MatchId")
.HasColumnType("int");
b.Property<string>("MessageText")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("SenderUserId")
.HasColumnType("int");
b.HasKey("MessageId");
b.HasIndex("MatchId");
b.HasIndex("SenderUserId");
b.ToTable("Messages");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.PlantEF", b =>
{
b.Property<int>("PlantId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PlantId"));
b.Property<string>("CareRequirements")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("Category")
.IsRequired()
.HasMaxLength(100)
.HasColumnType("nvarchar(100)");
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Description")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ImageUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("SpeciesName")
.IsRequired()
.HasMaxLength(200)
.HasColumnType("nvarchar(200)");
b.Property<DateTime>("UpdatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<int>("UserId")
.HasColumnType("int");
b.HasKey("PlantId");
b.HasIndex("UserId");
b.ToTable("Plants");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.ReportEF", b =>
{
b.Property<int>("ReportId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ReportId"));
b.Property<string>("Comments")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<bool>("IsResolved")
.HasColumnType("bit");
b.Property<string>("Reason")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("ReportedUserId")
.HasColumnType("int");
b.Property<int>("ReporterUserId")
.HasColumnType("int");
b.HasKey("ReportId");
b.HasIndex("ReportedUserId");
b.HasIndex("ReporterUserId");
b.ToTable("Reports");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.SwipeEF", b =>
{
b.Property<int>("SwipeId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SwipeId"));
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<bool>("IsLike")
.HasColumnType("bit");
b.Property<int>("SwipedPlantId")
.HasColumnType("int");
b.Property<int>("SwiperPlantId")
.HasColumnType("int");
b.HasKey("SwipeId");
b.HasIndex("SwipedPlantId");
b.HasIndex("SwiperPlantId", "SwipedPlantId")
.IsUnique();
b.ToTable("Swipes");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserEF", b =>
{
b.Property<int>("UserId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));
b.Property<string>("Bio")
.IsRequired()
.HasMaxLength(500)
.HasColumnType("nvarchar(500)");
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Email")
.IsRequired()
.HasMaxLength(256)
.HasColumnType("nvarchar(256)");
b.Property<Point>("Location")
.IsRequired()
.HasColumnType("geography");
b.Property<double?>("LocationLatitude")
.HasColumnType("float");
b.Property<double?>("LocationLongitude")
.HasColumnType("float");
b.Property<string>("Name")
.IsRequired()
.HasMaxLength(100)
.HasColumnType("nvarchar(100)");
b.Property<string>("PasswordHash")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ProfilePictureUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<DateTime>("UpdatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.HasKey("UserId");
b.HasIndex("Email")
.IsUnique();
b.HasIndex("LocationLatitude", "LocationLongitude");
b.ToTable("Users");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.Property<int>("UserId")
.HasColumnType("int");
b.Property<string>("PreferredCategories")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<double>("SearchRadius")
.HasColumnType("float");
b.HasKey("UserId");
b.ToTable("UserPreferences");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MatchEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "Plant1")
.WithMany()
.HasForeignKey("PlantId1")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "Plant2")
.WithMany()
.HasForeignKey("PlantId2")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User1")
.WithMany()
.HasForeignKey("UserId1")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User2")
.WithMany()
.HasForeignKey("UserId2")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("Plant1");
b.Navigation("Plant2");
b.Navigation("User1");
b.Navigation("User2");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MessageEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.MatchEF", "Match")
.WithMany("Messages")
.HasForeignKey("MatchId")
.OnDelete(DeleteBehavior.Cascade)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "SenderUser")
.WithMany("SentMessages")
.HasForeignKey("SenderUserId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("Match");
b.Navigation("SenderUser");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.PlantEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User")
.WithMany("Plants")
.HasForeignKey("UserId")
.OnDelete(DeleteBehavior.Cascade)
.IsRequired();
b.Navigation("User");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.ReportEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "ReportedUser")
.WithMany("ReportsReceived")
.HasForeignKey("ReportedUserId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "ReporterUser")
.WithMany("ReportsMade")
.HasForeignKey("ReporterUserId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("ReportedUser");
b.Navigation("ReporterUser");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.SwipeEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "SwipedPlant")
.WithMany()
.HasForeignKey("SwipedPlantId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.HasOne("Cuttr.Infrastructure.Entities.PlantEF", "SwiperPlant")
.WithMany()
.HasForeignKey("SwiperPlantId")
.OnDelete(DeleteBehavior.Restrict)
.IsRequired();
b.Navigation("SwipedPlant");
b.Navigation("SwiperPlant");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User")
.WithOne("Preferences")
.HasForeignKey("Cuttr.Infrastructure.Entities.UserPreferencesEF", "UserId")
.OnDelete(DeleteBehavior.Cascade)
.IsRequired();
b.Navigation("User");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.MatchEF", b =>
{
b.Navigation("Messages");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserEF", b =>
{
b.Navigation("Plants");
b.Navigation("Preferences")
.IsRequired();
b.Navigation("ReportsMade");
b.Navigation("ReportsReceived");
b.Navigation("SentMessages");
});
#pragma warning restore 612, 618
}
}
}
//.NETCoreApp,Version=v8.0.AssemblyAttributes
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//Cuttr.Infrastructure.AssemblyInfo
[assembly: System.Reflection.AssemblyCompanyAttribute("Cuttr.Infrastructure")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0")]
[assembly: System.Reflection.AssemblyProductAttribute("Cuttr.Infrastructure")]
[assembly: System.Reflection.AssemblyTitleAttribute("Cuttr.Infrastructure")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]
//Cuttr.Infrastructure.GlobalUsings.g

//EFCoreSqlServerNetTopologySuite
[assembly: Microsoft.EntityFrameworkCore.Design.DesignTimeServicesReferenceAttribute("Microsoft.EntityFrameworkCore.SqlServer.Design.Internal.SqlServerNetTopologySuite" +
"DesignTimeServices, Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite", "Microsoft.EntityFrameworkCore.SqlServer")]
//MatchRepository
namespace Cuttr.Infrastructure.Repositories
{
public class MatchRepository : IMatchRepository
{
private readonly CuttrDbContext _context;
private readonly ILogger<MatchRepository> _logger;
public MatchRepository(CuttrDbContext context, ILogger<MatchRepository> logger)
{
_context = context;
_logger = logger;
}
public async Task<IEnumerable<Match>> GetMatchesByUserIdAsync(int userId)
{
try
{
var efMatches = await _context.Matches
.Include(m => m.Plant1)
.ThenInclude(p => p.User)
.Include(m => m.Plant2)
.ThenInclude(p => p.User)
.Where(m => m.UserId1 == userId || m.UserId2 == userId)
.ToListAsync();
return efMatches.Select(EFToBusinessMapper.MapToMatch);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while retrieving matches for user with ID {userId}.");
throw new RepositoryException("An error occurred while retrieving matches.", ex);
}
}
public async Task<Match> GetMatchByIdAsync(int matchId)
{
try
{
var efMatch = await _context.Matches
.Include(m => m.Plant1)
.ThenInclude(p => p.User)
.Include(m => m.Plant2)
.ThenInclude(p => p.User)
.FirstOrDefaultAsync(m => m.MatchId == matchId);
return EFToBusinessMapper.MapToMatch(efMatch);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while retrieving match with ID {matchId}.");
throw new RepositoryException("An error occurred while retrieving match.", ex);
}
}
public async Task<Match> AddMatchAsync(Match match)
{
try
{
var efMatch = BusinessToEFMapper.MapToMatchEF(match);
efMatch.MatchId = 0;
await _context.Matches.AddAsync(efMatch);
await _context.SaveChangesAsync();
return EFToBusinessMapper.MapToMatch(efMatch);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while adding a match.");
throw new RepositoryException("An error occurred while adding a match.", ex);
}
}
}
}
//MessageRepository
namespace Cuttr.Infrastructure.Repositories
{
public class MessageRepository : IMessageRepository
{
private readonly CuttrDbContext _context;
private readonly ILogger<MessageRepository> _logger;
public MessageRepository(CuttrDbContext context, ILogger<MessageRepository> logger)
{
_context = context;
_logger = logger;
}
public async Task<Message> AddMessageAsync(Message message)
{
try
{
var efMessage = BusinessToEFMapper.MapToMessageEF(message);
efMessage.MessageId = 0;
await _context.Messages.AddAsync(efMessage);
await _context.SaveChangesAsync();
return EFToBusinessMapper.MapToMessage(efMessage);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while adding a message.");
throw new RepositoryException("An error occurred while adding a message.", ex);
}
}
public async Task<IEnumerable<Message>> GetMessagesByMatchIdAsync(int matchId)
{
try
{
var efMessages = await _context.Messages
.Where(m => m.MatchId == matchId)
.OrderBy(m => m.CreatedAt)
.ToListAsync();
return efMessages.Select(EFToBusinessMapper.MapToMessage);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while retrieving messages for match with ID {matchId}.");
throw new RepositoryException("An error occurred while retrieving messages.", ex);
}
}
}
}
//PlantRepository
namespace Cuttr.Infrastructure.Repositories
{
public class PlantRepository : IPlantRepository
{
private readonly CuttrDbContext _context;
private readonly ILogger<PlantRepository> _logger;
public PlantRepository(CuttrDbContext context, ILogger<PlantRepository> logger)
{
_context = context;
_logger = logger;
}
public async Task<Plant> AddPlantAsync(Plant plant)
{
try
{
var efPlant = BusinessToEFMapper.MapToPlantEF(plant);
efPlant.PlantId = 0;
await _context.Plants.AddAsync(efPlant);
await _context.SaveChangesAsync();
return EFToBusinessMapper.MapToPlant(efPlant);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while adding a plant.");
throw new RepositoryException("An error occurred while adding a plant.", ex);
}
}
public async Task<Plant> GetPlantByIdAsync(int plantId)
{
try
{
var efPlant = await _context.Plants
.Include(p => p.User)
.FirstOrDefaultAsync(p => p.PlantId == plantId);
if (efPlant == null)
{
_logger.LogWarning($"Plant with ID {plantId} not found.");
return null;
}
return EFToBusinessMapper.MapToPlant(efPlant);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while retrieving plant with ID {plantId}.");
throw new RepositoryException("An error occurred while retrieving plant.", ex);
}
}
public async Task UpdatePlantAsync(Plant plant)
{
try
{
var efPlant = BusinessToEFMapper.MapToPlantEF(plant);
_context.Plants.Update(efPlant);
await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
_logger.LogError(ex, $"A concurrency error occurred while updating plant with ID {plant.PlantId}.");
throw new RepositoryException("A concurrency error occurred while updating plant.", ex);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while updating plant with ID {plant.PlantId}.");
throw new RepositoryException("An error occurred while updating plant.", ex);
}
}
public async Task DeletePlantAsync(int plantId)
{
try
{
var efPlant = await _context.Plants.FindAsync(plantId);
if (efPlant == null)
{
_logger.LogWarning($"Plant with ID {plantId} not found.");
return;
}
_context.Plants.Remove(efPlant);
await _context.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
_logger.LogError(ex, $"A database error occurred while deleting plant with ID {plantId}.");
throw new RepositoryException("A database error occurred while deleting plant.", ex);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while deleting plant with ID {plantId}.");
throw new RepositoryException("An error occurred while deleting plant.", ex);
}
}
public async Task<IEnumerable<Plant>> GetPlantsByUserIdAsync(int userId)
{
try
{
var efPlants = await _context.Plants
.Where(p => p.UserId == userId)
.Include(p => p.User)
.ToListAsync();
return efPlants.Select(EFToBusinessMapper.MapToPlant);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while retrieving plants for user with ID {userId}.");
throw new RepositoryException("An error occurred while retrieving plants.", ex);
}
}
public async Task<IEnumerable<Plant>> GetAllPlantsAsync()
{
try
{
var efPlants = await _context.Plants
.Include(p => p.User)
.ToListAsync();
return efPlants.Select(EFToBusinessMapper.MapToPlant);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while retrieving all plants.");
throw new RepositoryException("An error occurred while retrieving all plants.", ex);
}
}
public async Task<IEnumerable<Plant>> GetPlantsWithinRadiusAsync(double originLat, double originLon, double radiusKm)
{
double radiusMeters = radiusKm * 1000;
var origin = new Point(originLon, originLat) { SRID = 4326 };
var efPlants = await _context.Plants
.Include(p => p.User)
.Where(p => p.User.Location != null && p.User.Location.Distance(origin) <= radiusMeters)
.ToListAsync();
return efPlants.Select(EFToBusinessMapper.MapToPlant);
}
}
}
//ReportRepository
namespace Cuttr.Infrastructure.Repositories
{
public class ReportRepository : IReportRepository
{
private readonly CuttrDbContext _context;
private readonly ILogger<ReportRepository> _logger;
public ReportRepository(CuttrDbContext context, ILogger<ReportRepository> logger)
{
_context = context;
_logger = logger;
}
public async Task<Report> AddReportAsync(Report report)
{
try
{
var efReport = BusinessToEFMapper.MapToReportEF(report);
efReport.ReportId = 0;
await _context.Reports.AddAsync(efReport);
await _context.SaveChangesAsync();
return EFToBusinessMapper.MapToReport(efReport);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while adding a report.");
throw new RepositoryException("An error occurred while adding a report.", ex);
}
}
}
}
//SwipeRepository
namespace Cuttr.Infrastructure.Repositories
{
public class SwipeRepository : ISwipeRepository
{
private readonly CuttrDbContext _context;
private readonly ILogger<SwipeRepository> _logger;
public SwipeRepository(CuttrDbContext context, ILogger<SwipeRepository> logger)
{
_context = context;
_logger = logger;
}
public async Task AddSwipeAsync(Swipe swipe)
{
try
{
var efSwipe = BusinessToEFMapper.MapToSwipeEF(swipe);
efSwipe.SwipeId = 0;
await _context.Swipes.AddAsync(efSwipe);
await _context.SaveChangesAsync();
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while adding a swipe.");
throw new RepositoryException("An error occurred while adding a swipe.", ex);
}
}
public async Task<Swipe> GetSwipeAsync(int swiperPlantId, int swipedPlantId, bool isLike)
{
try
{
var efSwipe = await _context.Swipes.FirstOrDefaultAsync(s =>
s.SwiperPlantId == swiperPlantId &&
s.SwipedPlantId == swipedPlantId &&
s.IsLike == isLike);
return EFToBusinessMapper.MapToSwipe(efSwipe);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while retrieving a swipe.");
throw new RepositoryException("An error occurred while retrieving a swipe.", ex);
}
}
public async Task<bool> HasSwipeAsync(int swiperPlantId, int swipedPlantId)
{
try
{
var exists = await _context.Swipes.AnyAsync(s =>
s.SwiperPlantId == swiperPlantId &&
s.SwipedPlantId == swipedPlantId);
return exists;
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while checking swipe existence.");
throw new RepositoryException("An error occurred while checking swipe existence.", ex);
}
}
}
}
//UserPreferencesRepository
namespace Cuttr.Infrastructure.Repositories
{
public class UserPreferencesRepository : IUserPreferencesRepository
{
private readonly CuttrDbContext _context;
private readonly ILogger<UserPreferencesRepository> _logger;
public UserPreferencesRepository(CuttrDbContext context, ILogger<UserPreferencesRepository> logger)
{
_context = context;
_logger = logger;
}
public async Task<UserPreferences> GetUserPreferencesAsync(int userId)
{
try
{
var efPreferences = await _context.UserPreferences
.Include(up => up.PreferredCategories)
.FirstOrDefaultAsync(up => up.UserId == userId);
return EFToBusinessMapper.MapToUserPreferences(efPreferences);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while retrieving user preferences for user ID {userId}.");
throw new RepositoryException("An error occurred while retrieving user preferences.", ex);
}
}
public async Task<UserPreferences> AddUserPreferencesAsync(UserPreferences preferences)
{
try
{
var efPreferences = BusinessToEFMapper.MapToUserPreferencesEF(preferences);
await _context.UserPreferences.AddAsync(efPreferences);
await _context.SaveChangesAsync();
return EFToBusinessMapper.MapToUserPreferences(efPreferences);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while adding user preferences.");
throw new RepositoryException("An error occurred while adding user preferences.", ex);
}
}
public async Task UpdateUserPreferencesAsync(UserPreferences preferences)
{
try
{
var efPreferences = await _context.UserPreferences
.FirstOrDefaultAsync(up => up.UserId == preferences.UserId);
if (efPreferences == null)
{
throw new RepositoryException($"User preferences for user ID {preferences.UserId} not found.");
}
efPreferences.SearchRadius = preferences.SearchRadius;
efPreferences.PreferredCategories = BusinessToEFMapper.SerializeCategories(preferences.PreferredCategories);
await _context.SaveChangesAsync();
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while updating user preferences for user ID {preferences.UserId}.");
throw new RepositoryException("An error occurred while updating user preferences.", ex);
}
}
}
}
//UserRepository
namespace Cuttr.Infrastructure.Repositories
{
public class UserRepository : IUserRepository
{
private readonly CuttrDbContext _context;
private readonly ILogger<UserRepository> _logger;
public UserRepository(CuttrDbContext context, ILogger<UserRepository> logger)
{
_context = context;
_logger = logger;
}
public async Task<User> CreateUserAsync(User user)
{
try
{
var efUser = BusinessToEFMapper.MapToUserEF(user);
efUser.UserId = 0;
await _context.Users.AddAsync(efUser);
await _context.SaveChangesAsync();
return EFToBusinessMapper.MapToUser(efUser);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while creating a user.");
throw new RepositoryException("An error occurred while creating a user.", ex);
}
}
public async Task<User> GetUserByIdAsync(int userId)
{
try
{
var efUser = await _context.Users
.Include(u => u.Plants)
.Include(u => u.Preferences)
.FirstOrDefaultAsync(u => u.UserId == userId);
if (efUser == null)
{
_logger.LogWarning($"User with ID {userId} not found.");
return null;
}
return EFToBusinessMapper.MapToUser(efUser);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while retrieving user with ID {userId}.");
throw new RepositoryException("An error occurred while retrieving user.", ex);
}
}
public async Task<User> GetUserByEmailAsync(string email)
{
try
{
var efUser = await _context.Users
.Include(u => u.Plants)
.Include(u => u.Preferences)
.FirstOrDefaultAsync(u => u.Email == email);
if (efUser == null)
{
_logger.LogWarning($"User with email {email} not found.");
return null;
}
return EFToBusinessMapper.MapToUser(efUser);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while retrieving user with email {email}.");
throw new RepositoryException("An error occurred while retrieving user.", ex);
}
}
public async Task UpdateUserAsync(User user)
{
try
{
var efUser = BusinessToEFMapper.MapToUserEF(user);
_context.Users.Update(efUser);
await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
_logger.LogError(ex, $"A concurrency error occurred while updating user with ID {user.UserId}.");
throw new RepositoryException("A concurrency error occurred while updating user.", ex);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while updating user with ID {user.UserId}.");
throw new RepositoryException("An error occurred while updating user.", ex);
}
}
public async Task DeleteUserAsync(int userId)
{
try
{
var efUser = await _context.Users.FindAsync(userId);
if (efUser == null)
{
_logger.LogWarning($"User with ID {userId} not found.");
return;
}
_context.Users.Remove(efUser);
await _context.SaveChangesAsync();
}
catch (DbUpdateException ex)
{
_logger.LogError(ex, $"A database error occurred while deleting user with ID {userId}.");
throw new RepositoryException("A database error occurred while deleting user.", ex);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while deleting user with ID {userId}.");
throw new RepositoryException("An error occurred while deleting user.", ex);
}
}
public async Task UpdateUserLocationAsync(int userId, double latitude, double longitude)
{
var point = new NetTopologySuite.Geometries.Point(longitude, latitude) { SRID = 4326 };
var efUser = await _context.Users.FindAsync(userId);
if (efUser == null)
throw new RepositoryException($"User with ID {userId} not found.");
efUser.Location = point;
await _context.SaveChangesAsync();
}
}
}