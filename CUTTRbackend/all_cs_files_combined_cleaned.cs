
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
builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, services, configuration) =>
{
configuration
.ReadFrom.Configuration(context.Configuration)
.ReadFrom.Services(services);
});
builder.Services.AddControllers(options =>
{
var policy = new AuthorizationPolicyBuilder()
.RequireAuthenticatedUser()
.Build();
options.Filters.Add(new AuthorizeFilter(policy));
})
.AddJsonOptions(options =>
{
options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddCors(options =>
{
options.AddDefaultPolicy(
builder =>
{
builder.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader();
});
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
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlantRepository, PlantRepository>();
builder.Services.AddScoped<ISwipeRepository, SwipeRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
{
Title = "Cuttr API",
Version = "v1"
});
options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
{
In = Microsoft.OpenApi.Models.ParameterLocation.Header,
Description = "Please enter JWT with Bearer into field. Example: \"Bearer {token}\"",
Name = "Authorization",
Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
Scheme = "Bearer",
BearerFormat = "JWT"
});
options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
{
{
new Microsoft.OpenApi.Models.OpenApiSecurityScheme
{
Reference = new Microsoft.OpenApi.Models.OpenApiReference
{
Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
Id = "Bearer"
}
},
new string[] {}
}
});
});
var secretKey = builder.Configuration["Jwt:Secret"];
var testforconfig = builder.Configuration["ConnectionStrings:CuttrDb"];
var key = Encoding.UTF8.GetBytes(secretKey);
builder.Services
.AddAuthentication(options =>
{
options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(cfg =>
{
cfg.TokenValidationParameters = new TokenValidationParameters
{
ValidateIssuer = false,
ValidateAudience = false,
ValidateLifetime = true,
ValidateIssuerSigningKey = true,
IssuerSigningKey = new SymmetricSecurityKey(key),
};
});
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
app.UseCors();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseSerilogRequestLogging();
app.Urls.Add("http:
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
//GetInfoFromClaims
namespace Cuttr.Api.Common
{
public static class GetInfoFromClaims
{
public static int GetUserId(this ClaimsPrincipal user)
{
var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
if (int.TryParse(userIdClaim, out int userId))
{
return userId;
}
throw new Business.Exceptions.UnauthorizedAccessException("Invalid token: User ID Claim is not valid.");
}
}
}
//AuthController
namespace Cuttr.Api.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
private readonly IAuthManager _authManager;
private readonly ILogger<AuthController> _logger;
public AuthController(IAuthManager authManager, ILogger<AuthController> logger)
{
_authManager = authManager;
_logger = logger;
}
[AllowAnonymous]
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
{
try
{
var response = await _authManager.AuthenticateUserAsync(request);
return Ok(response);
}
catch (AuthenticationException ex)
{
_logger.LogWarning(ex, "Authentication failed for email: {Email}", request.Email);
return Unauthorized(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Business error occurred while logging in user with email: {Email}", request.Email);
return BadRequest(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while logging in user with email: {Email}", request.Email);
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
}
}
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
int userId = 0;
try
{
userId = User.GetUserId();
await _authManager.LogoutUserAsync(userId);
return NoContent();
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, "User with ID {UserId} not found when attempting to log out.", userId);
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Business error occurred while logging out user with ID {UserId}.", userId);
return BadRequest(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while logging out user with ID {UserId}.", userId);
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
}
}
[AllowAnonymous]
[HttpPost("refresh")]
public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
{
try
{
var response = await _authManager.RefreshTokenAsync(request.RefreshToken);
return Ok(response);
}
catch (AuthenticationException ex)
{
_logger.LogWarning(ex, "Invalid or expired refresh token used.");
return Unauthorized(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Business error occurred while refreshing the token.");
return BadRequest(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while refreshing the token.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
}
}
}
}
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
[HttpGet("me")]
public async Task<IActionResult> GetMatches()
{
int userId = 0;
try
{
userId = User.GetUserId();
var matches = await _matchManager.GetMatchesByUserIdAsync(userId);
return Ok(matches);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving matches for user.");
return BadRequest(ex.Message);
}
catch (Business.Exceptions.UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while retrieving matches.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
}
}
[AllowAnonymous]
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
[HttpPost("/me")]
public async Task<IActionResult> SendMessage([FromBody] MessageRequest request)
{
int senderUserId = 0;
try
{
senderUserId = User.GetUserId();
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
catch (Business.Exceptions.UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while sending message.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
}
}
[HttpGet("/api/matches/{matchId}/messages")]
public async Task<IActionResult> GetMessages(int matchId)
{
int userId = 0;
try
{
userId = User.GetUserId();
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
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while accessing messages.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
}
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
[HttpPost("me")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> AddPlant([FromForm] PlantCreateRequest request)
{
int userId = 0;
try
{
userId = User.GetUserId();
var plantResponse = await _plantManager.AddPlantAsync(request, userId);
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
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while adding the plant.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
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
[HttpPut("me/{plantId}")]
public async Task<IActionResult> UpdatePlant(int plantId, [FromBody] PlantRequest request)
{
int userId = 0;
try
{
userId = User.GetUserId();
var plantResponse = await _plantManager.UpdatePlantAsync(plantId, userId, request);
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
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while updating the plant.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
}
}
[HttpDelete("me/{plantId}")]
public async Task<IActionResult> DeletePlant(int plantId)
{
int userId = 0;
try
{
userId = User.GetUserId();
await _plantManager.DeletePlantAsync(plantId, userId);
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
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while deleting the plant.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
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
[HttpGet("/api/users/me/plants")]
public async Task<IActionResult> GetPlantsOfUser()
{
int userId = 0;
try
{
userId = User.GetUserId();
var plantResponses = await _plantManager.GetPlantsByUserIdAsync(userId);
return Ok(plantResponses);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving plants for user with ID {userId}.");
return BadRequest(ex.Message);
}
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while retrieving the plants.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
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
int reporterUserId = 0;
try
{
reporterUserId = User.GetUserId();
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
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "Unexpected error creating report.");
return StatusCode(500, "An unexpected error occurred.");
}
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
[HttpPost("me")]
public async Task<IActionResult> RecordSwipe([FromBody] List<SwipeRequest> requests)
{
int userId = 0;
try
{
userId = User.GetUserId();
var swipeResponses = await _swipeManager.RecordSwipesAsync(requests, userId);
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
[HttpGet("me/likable-plants")]
public async Task<IActionResult> GetLikablePlants(int maxCount = 10)
{
int userId = 0;
try
{
userId = User.GetUserId();
var likablePlants = await _swipeManager.GetLikablePlantsAsync(userId, maxCount);
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
private readonly IAuthManager _authManager;
public UserController(IUserManager userManager, ILogger<UserController> logger, IAuthManager authManager)
{
_userManager = userManager;
_logger = logger;
_authManager = authManager;
}
[AllowAnonymous]
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
{
try
{
Console.WriteLine("Registering user...");
var userResponse = await _userManager.RegisterUserAsync(request);
var loginresponse = await _authManager.AuthenticateUserAsync(new UserLoginRequest { Email = request.Email, Password = request.Password });
return CreatedAtAction(nameof(GetUserById), new { userId = userResponse.UserId }, loginresponse);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error registering user.");
return BadRequest(ex.InnerException.Message);
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
[HttpGet("me")]
public async Task<IActionResult> GetMeByToken()
{
try
{
var userId = User.GetUserId();
var userResponse = await _userManager.GetUserByIdAsync(userId);
return Ok(userResponse);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, "User not found.");
return NotFound(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error retrieving user.");
return BadRequest(ex.Message);
}
catch (AuthenticationException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
}
[HttpPut("me")]
public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest request)
{
int userId = 0;
try
{
userId = User.GetUserId();
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
catch (Business.Exceptions.UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while updating the user.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
}
}
[HttpDelete("me")]
public async Task<IActionResult> DeleteUser()
{
int userId = 0;
try
{
userId = User.GetUserId();
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
catch (Business.Exceptions.UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An unexpected error occurred while deleting the user with ID {userId}.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
}
}
[HttpPut("me/profile-picture")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UpdateProfilePicture([FromForm] UserProfileImageUpdateRequest request)
{
int userId = 0;
try
{
userId = User.GetUserId();
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
catch (Business.Exceptions.UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while updating the profile picture.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
}
}
[HttpPut("me/location")]
public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
{
int userId = 0;
try
{
userId = User.GetUserId();
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
catch (Business.Exceptions.UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while updating the location.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
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
int userId = User.GetUserId();
var preferences = await _userPreferencesManager.GetUserPreferencesAsync(userId);
return Ok(preferences);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"User preferences for user ID {User.GetUserId} not found.");
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
int userId = User.GetUserId();
var preferences = await _userPreferencesManager.CreateOrUpdateUserPreferencesAsync(userId, request);
return Ok(preferences);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error creating or updating user preferences.");
return BadRequest(ex.Message);
}
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
namespace Cuttr.Api.Middleware
{
public class LoggingMiddleware
{
private readonly RequestDelegate _next;
private readonly ILogger<LoggingMiddleware> _logger;
private const string LoggingMiddlewareInvoked = "LoggingMiddlewareInvoked";
public LoggingMiddleware(
RequestDelegate next,
ILogger<LoggingMiddleware> logger)
{
_next = next;
_logger = logger;
}
public async Task Invoke(HttpContext context)
{
if (!context.Items.ContainsKey(LoggingMiddlewareInvoked))
{
context.Items[LoggingMiddlewareInvoked] = true;
await LogRequest(context);
var originalBodyStream = context.Response.Body;
{
context.Response.Body = responseBody;
try
{
await _next(context);
}
finally
{
await LogResponse(context);
context.Response.Body.Seek(0, SeekOrigin.Begin);
await responseBody.CopyToAsync(originalBodyStream);
context.Response.Body = originalBodyStream;
}
}
}
else
{
await _next(context);
}
}
private async Task LogRequest(HttpContext context)
{
context.Request.EnableBuffering();
var bodyAsText = string.Empty;
if (context.Request.ContentLength > 0 &&
context.Request.Body.CanSeek)
{
context.Request.Body.Seek(0, SeekOrigin.Begin);
context.Request.Body,
encoding: System.Text.Encoding.UTF8,
detectEncodingFromByteOrderMarks: false,
bufferSize: 8192,
leaveOpen: true))
{
bodyAsText = await reader.ReadToEndAsync();
}
context.Request.Body.Seek(0, SeekOrigin.Begin);
}
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
public string SpeciesName { get; set; }
public string? Description { get; set; }
public PlantStage PlantStage { get; set; }
public PlantCategory? PlantCategory { get; set; }
public WateringNeed? WateringNeed { get; set; }
public LightRequirement? LightRequirement { get; set; }
public Size? Size { get; set; }
public IndoorOutdoor? IndoorOutdoor { get; set; }
public PropagationEase? PropagationEase { get; set; }
public PetFriendly? PetFriendly { get; set; }
public List<Extras>? Extras { get; set; } = new List<Extras>();
}
}
//PlantUpdateRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class PlantUpdateRequest
{
public string SpeciesName { get; set; }
public string CareRequirements { get; set; }
public string Description { get; set; }
public string Category { get; set; }
}
}
//RefreshTokenRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class RefreshTokenRequest
{
public string RefreshToken { get; set; }
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
public List<PlantStage> PreferedPlantStage { get; set; }
public List<PlantCategory> PreferedPlantCategory { get; set; }
public List<WateringNeed> PreferedWateringNeed { get; set; }
public List<LightRequirement> PreferedLightRequirement { get; set; }
public List<Size> PreferedSize { get; set; }
public List<IndoorOutdoor> PreferedIndoorOutdoor { get; set; }
public List<PropagationEase> PreferedPropagationEase { get; set; }
public List<PetFriendly> PreferedPetFriendly { get; set; }
public List<Extras> PreferedExtras { get; set; }
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
//AuthTokenResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class AuthTokenResponse
{
public string AccessToken { get; set; }
public string RefreshToken { get; set; }
public string TokenType { get; set; } = "Bearer";
public int ExpiresIn { get; set; }
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
public string? Description { get; set; }
public PlantStage PlantStage { get; set; }
public PlantCategory? PlantCategory { get; set; }
public WateringNeed? WateringNeed { get; set; }
public LightRequirement? LightRequirement { get; set; }
public Size? Size { get; set; }
public IndoorOutdoor? IndoorOutdoor { get; set; }
public PropagationEase? PropagationEase { get; set; }
public PetFriendly? PetFriendly { get; set; }
public List<Extras>? Extras { get; set; }
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
public int UserId { get; set; }
public string Email { get; set; }
public AuthTokenResponse Tokens { get; set; }
}
}
//UserPreferencesResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class UserPreferencesResponse
{
public int UserId { get; set; }
public double SearchRadius { get; set; }
public List<PlantStage> PreferedPlantStage { get; set; }
public List<PlantCategory> PreferedPlantCategory { get; set; }
public List<WateringNeed> PreferedWateringNeed { get; set; }
public List<LightRequirement> PreferedLightRequirement { get; set; }
public List<Size> PreferedSize { get; set; }
public List<IndoorOutdoor> PreferedIndoorOutdoor { get; set; }
public List<PropagationEase> PreferedPropagationEase { get; set; }
public List<PetFriendly> PreferedPetFriendly { get; set; }
public List<Extras> PreferedExtras { get; set; }
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
public PlantCategory? PlantCategory { get; set; }
public WateringNeed? WateringNeed { get; set; }
public LightRequirement? LightRequirement { get; set; }
public Size? Size { get; set; }
public IndoorOutdoor? IndoorOutdoor { get; set; }
public PropagationEase? PropagationEase { get; set; }
public PetFriendly? PetFriendly { get; set; }
public List<Extras> Extras { get; set; }
public string? ImageUrl { get; set; }
public User User { get; set; }
}
}
//RefreshToken
namespace Cuttr.Business.Entities
{
public class RefreshToken
{
public int RefreshTokenId { get; set; }
public int UserId { get; set; }
public string TokenHash { get; set; }
public DateTime ExpiresAt { get; set; }
public bool IsRevoked { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime? RevokedAt { get; set; }
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
public List<PlantStage> PreferedPlantStage { get; set; }
public List<PlantCategory> PreferedPlantCategory { get; set; }
public List<WateringNeed> PreferedWateringNeed { get; set; }
public List<LightRequirement> PreferedLightRequirement { get; set; }
public List<Size> PreferedSize { get; set; }
public List<IndoorOutdoor> PreferedIndoorOutdoor { get; set; }
public List<PropagationEase> PreferedPropagationEase { get; set; }
public List<PetFriendly> PreferedPetFriendly { get; set; }
public List<Extras> PreferedExtras { get; set; }
public User User { get; set; }
}
}
//PlantProperties
namespace Cuttr.Business.Enums
{
public enum PlantStage
{
Seedling,
Cutting,
Mature
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
AquaticPlant,
ClimbingPlant,
Tree,
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
PartialSun,
BrightIndirectLight,
LowLight
}
public enum Size
{
SmallSize,
MediumSize,
LargeSize
}
public enum IndoorOutdoor
{
Indoor,
Outdoor,
IndoorAndOutdoor
}
public enum PropagationEase
{
EasyPropagation,
ModeratePropagation,
DifficultPropagation
}
public enum PetFriendly
{
PetFriendly,
NotPetFriendly
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
//IAuthManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface IAuthManager
{
Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request);
Task<AuthTokenResponse> RefreshTokenAsync(string refreshToken);
Task LogoutUserAsync(int userId);
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
Task<PlantResponse> AddPlantAsync(PlantCreateRequest request, int userId);
Task<PlantResponse> GetPlantByIdAsync(int plantId);
Task<PlantResponse> UpdatePlantAsync(int plantId, int userId, PlantRequest request);
Task DeletePlantAsync(int plantId, int userId);
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
Task<List<SwipeResponse>> RecordSwipesAsync(List<SwipeRequest> requests, int userId);
Task<List<PlantResponse>> GetLikablePlantsAsync(int userId, int maxCount);
}
}
//IUserManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface IUserManager
{
Task<UserResponse> RegisterUserAsync(UserRegistrationRequest request);
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
//IRefreshTokenRepository
namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
public interface IRefreshTokenRepository
{
Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token);
Task<RefreshToken> GetRefreshTokenAsync(string tokenHash);
Task RevokeRefreshTokenAsync(string tokenHash);
Task DeleteRefreshTokensForUserAsync(int userId);
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
Task UpdateUserNameAndBioAsync(User user);
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
//AuthManager
namespace Cuttr.Business.Managers
{
public class AuthManager : IAuthManager
{
private readonly IUserRepository _userRepository;
private readonly IRefreshTokenRepository _refreshTokenRepository;
private readonly JwtTokenGenerator _jwtTokenGenerator;
private readonly ILogger<AuthManager> _logger;
public AuthManager(
IUserRepository userRepository,
IRefreshTokenRepository refreshTokenRepository,
JwtTokenGenerator jwtTokenGenerator,
ILogger<AuthManager> logger)
{
_userRepository = userRepository;
_refreshTokenRepository = refreshTokenRepository;
_jwtTokenGenerator = jwtTokenGenerator;
_logger = logger;
}
public async Task<UserLoginResponse> AuthenticateUserAsync(UserLoginRequest request)
{
try
{
var user = await _userRepository.GetUserByEmailAsync(request.Email);
if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
{
_logger.LogWarning("Invalid login attempt for email: {Email}", request.Email);
throw new AuthenticationException("Invalid email or password.");
}
var accessToken = _jwtTokenGenerator.GenerateToken(user, out int expiresIn);
var refreshToken = GenerateRefreshToken();
var tokenHash = HashToken(refreshToken);
var refreshTokenEntity = new RefreshToken
{
UserId = user.UserId,
TokenHash = tokenHash,
ExpiresAt = DateTime.UtcNow.AddDays(30),
IsRevoked = false,
CreatedAt = DateTime.UtcNow
};
await _refreshTokenRepository.CreateRefreshTokenAsync(refreshTokenEntity);
return new UserLoginResponse
{
UserId = user.UserId,
Email = user.Email,
Tokens = new AuthTokenResponse
{
AccessToken = accessToken,
RefreshToken = refreshToken,
ExpiresIn = expiresIn
}
};
}
catch (AuthenticationException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while authenticating user with email: {Email}", request.Email);
throw new BusinessException("An error occurred while authenticating the user.", ex);
}
}
public async Task<AuthTokenResponse> RefreshTokenAsync(string refreshToken)
{
try
{
var tokenHash = HashToken(refreshToken);
var existingToken = await _refreshTokenRepository.GetRefreshTokenAsync(tokenHash);
if (existingToken == null)
{
_logger.LogWarning("Invalid or expired refresh token encountered.");
throw new AuthenticationException("Invalid or expired refresh token.");
}
await _refreshTokenRepository.RevokeRefreshTokenAsync(tokenHash);
var user = await _userRepository.GetUserByIdAsync(existingToken.UserId);
if (user == null)
{
_logger.LogWarning("User with ID {UserId} not found when refreshing token.", existingToken.UserId);
throw new NotFoundException("User not found.");
}
var accessToken = _jwtTokenGenerator.GenerateToken(user, out int expiresIn);
var newRefreshToken = GenerateRefreshToken();
var newTokenHash = HashToken(newRefreshToken);
await _refreshTokenRepository.CreateRefreshTokenAsync(new RefreshToken
{
UserId = user.UserId,
TokenHash = newTokenHash,
ExpiresAt = DateTime.UtcNow.AddDays(30),
IsRevoked = false,
CreatedAt = DateTime.UtcNow
});
return new AuthTokenResponse
{
AccessToken = accessToken,
RefreshToken = newRefreshToken,
ExpiresIn = expiresIn
};
}
catch (AuthenticationException)
{
throw;
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while refreshing the token.");
throw new BusinessException("An error occurred while refreshing the token.", ex);
}
}
public async Task LogoutUserAsync(int userId)
{
try
{
await _refreshTokenRepository.DeleteRefreshTokensForUserAsync(userId);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while logging out user with ID {UserId}.", userId);
throw new BusinessException("An error occurred while logging out.", ex);
}
}
private string GenerateRefreshToken()
{
var randomNumber = new byte[32];
rng.GetBytes(randomNumber);
return Convert.ToBase64String(randomNumber);
}
private string HashToken(string token)
{
var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
return Convert.ToBase64String(hashBytes);
}
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
public async Task<PlantResponse> AddPlantAsync(PlantCreateRequest request, int userId)
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
imageUrl = await _blobStorageService.UploadFileAsync(request.Image, PlantImagesContainer);
}
var plant = ContractToBusinessMapper.MapToPlant(request.PlantDetails);
plant.ImageUrl = imageUrl;
plant.UserId = userId;
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
public async Task<PlantResponse> UpdatePlantAsync(int plantId,int userId, PlantRequest request)
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
ContractToBusinessMapper.MapToPlantForUpdate(request, plant);
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
public async Task DeletePlantAsync(int plantId, int userId)
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
public async Task<List<SwipeResponse>> RecordSwipesAsync(List<SwipeRequest> requests, int userId)
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
var user = await _userRepository.GetUserByIdAsync(userId);
if (swiperPlant.UserId != userId)
throw new Exceptions.UnauthorizedAccessException("Swiper plant does not belong to the user.");
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
public async Task<List<PlantResponse>> GetLikablePlantsAsync(int userId, int maxCount)
{
try
{
var user = await _userRepository.GetUserByIdAsync(userId);
if (user == null)
throw new BusinessException("User not found.");
if (user.Preferences == null)
throw new BusinessException("User preferences not found.");
if (user.LocationLatitude == null || user.LocationLongitude == null)
throw new BusinessException("User location not set.");
int radius = user.Preferences.SearchRadius > 0
? user.Preferences.SearchRadius
: 10000;
var candidatePlants = await _plantRepository.GetPlantsWithinRadiusAsync(
user.LocationLatitude.Value,
user.LocationLongitude.Value,
radius);
candidatePlants = candidatePlants.Where(p => p.UserId != userId);
if (user.Preferences.PreferedPlantStage != null && user.Preferences.PreferedPlantStage.Any())
{
candidatePlants = candidatePlants
.Where(p => user.Preferences.PreferedPlantStage.Contains(p.PlantStage));
}
if (user.Preferences.PreferedPlantCategory != null && user.Preferences.PreferedPlantCategory.Any())
{
candidatePlants = candidatePlants
.Where(p => p.PlantCategory.HasValue
&& user.Preferences.PreferedPlantCategory.Contains(p.PlantCategory.Value));
}
if (user.Preferences.PreferedWateringNeed != null && user.Preferences.PreferedWateringNeed.Any())
{
candidatePlants = candidatePlants
.Where(p => p.WateringNeed.HasValue
&& user.Preferences.PreferedWateringNeed.Contains(p.WateringNeed.Value));
}
if (user.Preferences.PreferedLightRequirement != null && user.Preferences.PreferedLightRequirement.Any())
{
candidatePlants = candidatePlants
.Where(p => p.LightRequirement.HasValue
&& user.Preferences.PreferedLightRequirement.Contains(p.LightRequirement.Value));
}
if (user.Preferences.PreferedSize != null && user.Preferences.PreferedSize.Any())
{
candidatePlants = candidatePlants
.Where(p => p.Size.HasValue
&& user.Preferences.PreferedSize.Contains(p.Size.Value));
}
if (user.Preferences.PreferedIndoorOutdoor != null && user.Preferences.PreferedIndoorOutdoor.Any())
{
candidatePlants = candidatePlants
.Where(p => p.IndoorOutdoor.HasValue
&& user.Preferences.PreferedIndoorOutdoor.Contains(p.IndoorOutdoor.Value));
}
if (user.Preferences.PreferedPropagationEase != null && user.Preferences.PreferedPropagationEase.Any())
{
candidatePlants = candidatePlants
.Where(p => p.PropagationEase.HasValue
&& user.Preferences.PreferedPropagationEase.Contains(p.PropagationEase.Value));
}
if (user.Preferences.PreferedPetFriendly != null && user.Preferences.PreferedPetFriendly.Any())
{
candidatePlants = candidatePlants
.Where(p => p.PetFriendly.HasValue
&& user.Preferences.PreferedPetFriendly.Contains(p.PetFriendly.Value));
}
if (user.Preferences.PreferedExtras != null && user.Preferences.PreferedExtras.Any())
{
candidatePlants = candidatePlants
.Where(p => p.Extras != null && p.Extras.Any(e => user.Preferences.PreferedExtras.Contains(e)));
}
var userPlants = await _plantRepository.GetPlantsByUserIdAsync(userId);
var likablePlants = new List<PlantResponse>();
foreach (var plant in candidatePlants)
{
bool hasUninteractedPlant = false;
foreach (var up in userPlants)
{
bool hasSwipe = await _swipeRepository.HasSwipeAsync(up.PlantId, plant.PlantId);
if (!hasSwipe)
{
hasUninteractedPlant = true;
break;
}
}
if (hasUninteractedPlant)
{
likablePlants.Add(BusinessToContractMapper.MapToPlantResponse(plant));
}
if (likablePlants.Count >= maxCount)
break;
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
private readonly IUserPreferencesRepository _userPreferencesRepository;
private const string ProfileImagesContainer = "profile-images";
public UserManager(IUserRepository userRepository, ILogger<UserManager> logger, JwtTokenGenerator jwtTokenGenerator, IBlobStorageService blobStorageService, IUserPreferencesRepository userPreferencesRepository)
{
_userRepository = userRepository;
_logger = logger;
_jwtTokenGenerator = jwtTokenGenerator;
_blobStorageService = blobStorageService;
_userPreferencesRepository = userPreferencesRepository;
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
var defaultPreferences = new UserPreferences
{
UserId = createdUser.UserId,
SearchRadius = 10000,
PreferedPlantStage = new List<PlantStage>(),
PreferedPlantCategory = new List<PlantCategory>(),
PreferedWateringNeed = new List<WateringNeed>(),
PreferedLightRequirement = new List<LightRequirement>(),
PreferedSize = new List<Size>(),
PreferedIndoorOutdoor = new List<IndoorOutdoor>(),
PreferedPropagationEase = new List<PropagationEase>(),
PreferedPetFriendly = new List<PetFriendly>(),
PreferedExtras = new List<Extras>()
};
await _userPreferencesRepository.AddUserPreferencesAsync(defaultPreferences);
return BusinessToContractMapper.MapToUserResponse(createdUser);
}
catch (Exception ex)
{
_logger.LogError(ex, "Error registering user.");
throw new BusinessException("Error registering user.", ex);
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
await _userRepository.UpdateUserNameAndBioAsync(user);
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
await _userRepository.UpdateUserNameAndBioAsync(user);
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
UserPreferences userpref = ContractToBusinessMapper.MapToUserPreferences(request);
userpref.UserId = userId;
await _userPreferencesRepository.UpdateUserPreferencesAsync(userpref);
return BusinessToContractMapper.MapToUserPreferencesResponse(userpref);
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
public static PlantResponse MapToPlantResponse(Plant plant)
{
if (plant == null)
return null;
return new PlantResponse
{
PlantId = plant.PlantId,
UserId = plant.UserId,
SpeciesName = plant.SpeciesName,
Description = plant.Description,
ImageUrl = plant.ImageUrl,
PlantStage = plant.PlantStage,
PlantCategory = plant.PlantCategory,
WateringNeed = plant.WateringNeed,
LightRequirement = plant.LightRequirement,
Size = plant.Size,
IndoorOutdoor = plant.IndoorOutdoor,
PropagationEase = plant.PropagationEase,
PetFriendly = plant.PetFriendly,
Extras = plant.Extras
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
PreferedPlantStage = preferences.PreferedPlantStage,
PreferedPlantCategory = preferences.PreferedPlantCategory,
PreferedWateringNeed = preferences.PreferedWateringNeed,
PreferedLightRequirement = preferences.PreferedLightRequirement,
PreferedSize = preferences.PreferedSize,
PreferedIndoorOutdoor = preferences.PreferedIndoorOutdoor,
PreferedPropagationEase = preferences.PreferedPropagationEase,
PreferedPetFriendly = preferences.PreferedPetFriendly,
PreferedExtras = preferences.PreferedExtras
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
SpeciesName = request.SpeciesName,
Description = request.Description,
PlantStage = request.PlantStage,
PlantCategory = request.PlantCategory,
WateringNeed = request.WateringNeed,
LightRequirement = request.LightRequirement,
Size = request.Size,
IndoorOutdoor = request.IndoorOutdoor,
PropagationEase = request.PropagationEase,
PetFriendly = request.PetFriendly,
Extras = request.Extras
};
}
public static void MapToPlantForUpdate(PlantRequest request, Plant plant)
{
if (request == null || plant == null)
return;
plant.SpeciesName = request.SpeciesName ?? plant.SpeciesName;
plant.Description = request.Description ?? plant.Description;
plant.PlantStage = request.PlantStage;
plant.PlantCategory = request.PlantCategory;
plant.WateringNeed = request.WateringNeed;
plant.LightRequirement = request.LightRequirement;
plant.Size = request.Size;
plant.IndoorOutdoor = request.IndoorOutdoor;
plant.PropagationEase = request.PropagationEase;
plant.PetFriendly = request.PetFriendly;
plant.Extras = request.Extras;
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
PreferedPlantStage = request.PreferedPlantStage,
PreferedPlantCategory = request.PreferedPlantCategory,
PreferedWateringNeed = request.PreferedWateringNeed,
PreferedLightRequirement = request.PreferedLightRequirement,
PreferedSize = request.PreferedSize,
PreferedIndoorOutdoor = request.PreferedIndoorOutdoor,
PreferedPropagationEase = request.PreferedPropagationEase,
PreferedPetFriendly = request.PreferedPetFriendly,
PreferedExtras = request.PreferedExtras
};
}
}
}
//.NETCoreApp,Version=v8.0.AssemblyAttributes
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//Cuttr.Business.AssemblyInfo
[assembly: System.Reflection.AssemblyCompanyAttribute("Cuttr.Business")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+c8c7fba12c66504407d5c072b603348b48e662ce")]
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
public string GenerateToken(User user, out int expiresIn)
{
var tokenHandler = new JwtSecurityTokenHandler();
var secretKey = _configuration["Jwt:Secret"];
var key = Encoding.UTF8.GetBytes(secretKey);
var tokenDescriptor = new SecurityTokenDescriptor
{
Subject = new ClaimsIdentity(new[] {
new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
new Claim(ClaimTypes.Email, user.Email),
new Claim(ClaimTypes.Name, user.Name)
}),
Expires = DateTime.UtcNow.AddMinutes(15),
SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
};
var token = tokenHandler.CreateToken(tokenDescriptor);
var jwt = tokenHandler.WriteToken(token);
expiresIn = (int)((tokenDescriptor.Expires.Value - DateTime.UtcNow).TotalSeconds);
return jwt;
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
public DbSet<RefreshTokenEF> RefreshTokens { get; set; }
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
public string? Description { get; set; }
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
//RefreshTokenEF
namespace Cuttr.Infrastructure.Entities
{
public class RefreshTokenEF
{
[Key]
public int RefreshTokenId { get; set; }
public int UserId { get; set; }
public string TokenHash { get; set; }
public DateTime ExpiresAt { get; set; }
public bool IsRevoked { get; set; }
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public DateTime? RevokedAt { get; set; }
public UserEF User { get; set; }
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
public string? ProfilePictureUrl { get; set; }
[MaxLength(500)]
public string? Bio { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime UpdatedAt { get; set; }
public Point? Location { get; set; }
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
public string PreferedPlantStage { get; set; }
public string PreferedPlantCategory { get; set; }
public string PreferedWateringNeed { get; set; }
public string PreferedLightRequirement { get; set; }
public string PreferedSize { get; set; }
public string PreferedIndoorOutdoor { get; set; }
public string PreferedPropagationEase { get; set; }
public string PreferedPetFriendly { get; set; }
public string PreferedExtras { get; set; }
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
NetTopologySuite.Geometries.Point location = null;
if (user.LocationLongitude.HasValue && user.LocationLatitude.HasValue)
{
location = new NetTopologySuite.Geometries.Point(
user.LocationLongitude.Value,
user.LocationLatitude.Value
)
{
SRID = 4326
};
}
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
Location = location,
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
PreferedPlantStage = SerializePreferedPlantStages(preferences.PreferedPlantStage),
PreferedPlantCategory = SerializePreferedPlantCategories(preferences.PreferedPlantCategory),
PreferedWateringNeed = SerializePreferedWateringNeeds(preferences.PreferedWateringNeed),
PreferedLightRequirement = SerializePreferedLightRequirements(preferences.PreferedLightRequirement),
PreferedSize = SerializePreferedSizes(preferences.PreferedSize),
PreferedIndoorOutdoor = SerializePreferedIndoorOutdoors(preferences.PreferedIndoorOutdoor),
PreferedPropagationEase = SerializePreferedPropagationEases(preferences.PreferedPropagationEase),
PreferedPetFriendly = SerializePreferedPetFriendlies(preferences.PreferedPetFriendly),
PreferedExtras = SerializeExtras(preferences.PreferedExtras),
};
}
public static string SerializePreferedPlantStages(List<PlantStage> plantStages)
{
if (plantStages == null || !plantStages.Any())
return "";
return System.Text.Json.JsonSerializer.Serialize(plantStages);
}
public static string SerializePreferedPlantCategories(List<PlantCategory> plantCategories)
{
if (plantCategories == null || !plantCategories.Any())
return "";
return System.Text.Json.JsonSerializer.Serialize(plantCategories);
}
public static string SerializePreferedWateringNeeds(List<WateringNeed> wateringNeeds)
{
if (wateringNeeds == null || !wateringNeeds.Any())
return "";
return System.Text.Json.JsonSerializer.Serialize(wateringNeeds);
}
public static string SerializePreferedLightRequirements(List<LightRequirement> lightRequirements)
{
if (lightRequirements == null || !lightRequirements.Any())
return "";
return System.Text.Json.JsonSerializer.Serialize(lightRequirements);
}
public static string SerializePreferedSizes(List<Size> sizes)
{
if (sizes == null || !sizes.Any())
return "";
return System.Text.Json.JsonSerializer.Serialize(sizes);
}
public static string SerializePreferedIndoorOutdoors(List<IndoorOutdoor> indoorOutdoors)
{
if (indoorOutdoors == null || !indoorOutdoors.Any())
return "";
return System.Text.Json.JsonSerializer.Serialize(indoorOutdoors);
}
public static string SerializePreferedPropagationEases(List<PropagationEase> propagationEases)
{
if (propagationEases == null || !propagationEases.Any())
return "";
return System.Text.Json.JsonSerializer.Serialize(propagationEases);
}
public static string SerializePreferedPetFriendlies(List<PetFriendly> petFriendlies)
{
if (petFriendlies == null || !petFriendlies.Any())
return "";
return System.Text.Json.JsonSerializer.Serialize(petFriendlies);
}
public static string SerializeExtras(List<Extras> extras)
{
if (extras == null || !extras.Any())
return "";
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
PlantCategory = !string.IsNullOrWhiteSpace(efPlant.PlantCategory)
? Enum.Parse<PlantCategory>(efPlant.PlantCategory)
: null,
WateringNeed = !string.IsNullOrWhiteSpace(efPlant.WateringNeed)
? Enum.Parse<WateringNeed>(efPlant.WateringNeed)
: null,
LightRequirement = !string.IsNullOrWhiteSpace(efPlant.LightRequirement)
? Enum.Parse<LightRequirement>(efPlant.LightRequirement)
: null,
Size = !string.IsNullOrWhiteSpace(efPlant.Size)
? Enum.Parse<Size>(efPlant.Size)
: null,
IndoorOutdoor = !string.IsNullOrWhiteSpace(efPlant.IndoorOutdoor)
? Enum.Parse<IndoorOutdoor>(efPlant.IndoorOutdoor)
: null,
PropagationEase = !string.IsNullOrWhiteSpace(efPlant.PropagationEase)
? Enum.Parse<PropagationEase>(efPlant.PropagationEase)
: null,
PetFriendly = !string.IsNullOrWhiteSpace(efPlant.PetFriendly)
? Enum.Parse<PetFriendly>(efPlant.PetFriendly)
: null,
Extras = efPlant.Extras != null
? DeserializeExtras(efPlant.Extras)
: null,
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
PlantCategory = !string.IsNullOrWhiteSpace(efPlant.PlantCategory)
? Enum.Parse<PlantCategory>(efPlant.PlantCategory)
: null,
WateringNeed = !string.IsNullOrWhiteSpace(efPlant.WateringNeed)
? Enum.Parse<WateringNeed>(efPlant.WateringNeed)
: null,
LightRequirement = !string.IsNullOrWhiteSpace(efPlant.LightRequirement)
? Enum.Parse<LightRequirement>(efPlant.LightRequirement)
: null,
Size = !string.IsNullOrWhiteSpace(efPlant.Size)
? Enum.Parse<Size>(efPlant.Size)
: null,
IndoorOutdoor = !string.IsNullOrWhiteSpace(efPlant.IndoorOutdoor)
? Enum.Parse<IndoorOutdoor>(efPlant.IndoorOutdoor)
: null,
PropagationEase = !string.IsNullOrWhiteSpace(efPlant.PropagationEase)
? Enum.Parse<PropagationEase>(efPlant.PropagationEase)
: null,
PetFriendly = !string.IsNullOrWhiteSpace(efPlant.PetFriendly)
? Enum.Parse<PetFriendly>(efPlant.PetFriendly)
: null,
Extras = efPlant.Extras != null
? DeserializeExtras(efPlant.Extras)
: null,
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
PreferedPlantStage = DeserializePlantStage(efPreferences.PreferedPlantStage),
PreferedPlantCategory = DeserializePlantCategory(efPreferences.PreferedPlantCategory),
PreferedWateringNeed = DeserializeWateringNeed(efPreferences.PreferedWateringNeed),
PreferedLightRequirement = DeserializeLightRequirement(efPreferences.PreferedLightRequirement),
PreferedSize = DeserializeSize(efPreferences.PreferedSize),
PreferedIndoorOutdoor = DeserializeIndoorOutdoor(efPreferences.PreferedIndoorOutdoor),
PreferedPropagationEase = DeserializePropagationEase(efPreferences.PreferedPropagationEase),
PreferedPetFriendly = DeserializePetFriendly(efPreferences.PreferedPetFriendly),
PreferedExtras = DeserializeExtras(efPreferences.PreferedExtras),
};
}
private static List<PlantStage> DeserializePlantStage(string plantstages)
{
if (string.IsNullOrEmpty(plantstages))
return new List<PlantStage>();
return System.Text.Json.JsonSerializer.Deserialize<List<PlantStage>>(plantstages);
}
private static List<PlantCategory> DeserializePlantCategory(string plantcategories)
{
if (string.IsNullOrEmpty(plantcategories))
return new List<PlantCategory>();
return System.Text.Json.JsonSerializer.Deserialize<List<PlantCategory>>(plantcategories);
}
private static List<WateringNeed> DeserializeWateringNeed(string wateringneeds)
{
if (string.IsNullOrEmpty(wateringneeds))
return new List<WateringNeed>();
return System.Text.Json.JsonSerializer.Deserialize<List<WateringNeed>>(wateringneeds);
}
private static List<LightRequirement> DeserializeLightRequirement(string lightrequirements)
{
if (string.IsNullOrEmpty(lightrequirements))
return new List<LightRequirement>();
return System.Text.Json.JsonSerializer.Deserialize<List<LightRequirement>>(lightrequirements);
}
private static List<Size> DeserializeSize(string sizes)
{
if (string.IsNullOrEmpty(sizes))
return new List<Size>();
return System.Text.Json.JsonSerializer.Deserialize<List<Size>>(sizes);
}
private static List<IndoorOutdoor> DeserializeIndoorOutdoor(string indooroutdoors)
{
if (string.IsNullOrEmpty(indooroutdoors))
return new List<IndoorOutdoor>();
return System.Text.Json.JsonSerializer.Deserialize<List<IndoorOutdoor>>(indooroutdoors);
}
private static List<PropagationEase> DeserializePropagationEase(string propagationeases)
{
if (string.IsNullOrEmpty(propagationeases))
return new List<PropagationEase>();
return System.Text.Json.JsonSerializer.Deserialize<List<PropagationEase>>(propagationeases);
}
private static List<PetFriendly> DeserializePetFriendly(string petfriendlies)
{
if (string.IsNullOrEmpty(petfriendlies))
return new List<PetFriendly>();
return System.Text.Json.JsonSerializer.Deserialize<List<PetFriendly>>(petfriendlies);
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
//20241213060227_AddFilters
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
public partial class AddFilters : Migration
{
protected override void Up(MigrationBuilder migrationBuilder)
{
migrationBuilder.DropIndex(
name: "IX_Users_LocationLatitude_LocationLongitude",
table: "Users");
migrationBuilder.DropColumn(
name: "LocationLatitude",
table: "Users");
migrationBuilder.DropColumn(
name: "LocationLongitude",
table: "Users");
migrationBuilder.DropColumn(
name: "Category",
table: "Plants");
migrationBuilder.RenameColumn(
name: "PreferredCategories",
table: "UserPreferences",
newName: "PreferedWateringNeed");
migrationBuilder.RenameColumn(
name: "CareRequirements",
table: "Plants",
newName: "Extras");
migrationBuilder.AlterColumn<int>(
name: "SearchRadius",
table: "UserPreferences",
type: "int",
nullable: false,
oldClrType: typeof(double),
oldType: "float");
migrationBuilder.AddColumn<string>(
name: "PreferedExtras",
table: "UserPreferences",
type: "nvarchar(max)",
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "PreferedIndoorOutdoor",
table: "UserPreferences",
type: "nvarchar(max)",
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "PreferedLightRequirement",
table: "UserPreferences",
type: "nvarchar(max)",
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "PreferedPetFriendly",
table: "UserPreferences",
type: "nvarchar(max)",
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "PreferedPlantCategory",
table: "UserPreferences",
type: "nvarchar(max)",
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "PreferedPlantStage",
table: "UserPreferences",
type: "nvarchar(max)",
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "PreferedPropagationEase",
table: "UserPreferences",
type: "nvarchar(max)",
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "PreferedSize",
table: "UserPreferences",
type: "nvarchar(max)",
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "IndoorOutdoor",
table: "Plants",
type: "nvarchar(50)",
maxLength: 50,
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "LightRequirement",
table: "Plants",
type: "nvarchar(50)",
maxLength: 50,
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "PetFriendly",
table: "Plants",
type: "nvarchar(50)",
maxLength: 50,
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "PlantCategory",
table: "Plants",
type: "nvarchar(50)",
maxLength: 50,
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "PlantStage",
table: "Plants",
type: "nvarchar(50)",
maxLength: 50,
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "PropagationEase",
table: "Plants",
type: "nvarchar(50)",
maxLength: 50,
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "Size",
table: "Plants",
type: "nvarchar(50)",
maxLength: 50,
nullable: false,
defaultValue: "");
migrationBuilder.AddColumn<string>(
name: "WateringNeed",
table: "Plants",
type: "nvarchar(50)",
maxLength: 50,
nullable: false,
defaultValue: "");
}
protected override void Down(MigrationBuilder migrationBuilder)
{
migrationBuilder.DropColumn(
name: "PreferedExtras",
table: "UserPreferences");
migrationBuilder.DropColumn(
name: "PreferedIndoorOutdoor",
table: "UserPreferences");
migrationBuilder.DropColumn(
name: "PreferedLightRequirement",
table: "UserPreferences");
migrationBuilder.DropColumn(
name: "PreferedPetFriendly",
table: "UserPreferences");
migrationBuilder.DropColumn(
name: "PreferedPlantCategory",
table: "UserPreferences");
migrationBuilder.DropColumn(
name: "PreferedPlantStage",
table: "UserPreferences");
migrationBuilder.DropColumn(
name: "PreferedPropagationEase",
table: "UserPreferences");
migrationBuilder.DropColumn(
name: "PreferedSize",
table: "UserPreferences");
migrationBuilder.DropColumn(
name: "IndoorOutdoor",
table: "Plants");
migrationBuilder.DropColumn(
name: "LightRequirement",
table: "Plants");
migrationBuilder.DropColumn(
name: "PetFriendly",
table: "Plants");
migrationBuilder.DropColumn(
name: "PlantCategory",
table: "Plants");
migrationBuilder.DropColumn(
name: "PlantStage",
table: "Plants");
migrationBuilder.DropColumn(
name: "PropagationEase",
table: "Plants");
migrationBuilder.DropColumn(
name: "Size",
table: "Plants");
migrationBuilder.DropColumn(
name: "WateringNeed",
table: "Plants");
migrationBuilder.RenameColumn(
name: "PreferedWateringNeed",
table: "UserPreferences",
newName: "PreferredCategories");
migrationBuilder.RenameColumn(
name: "Extras",
table: "Plants",
newName: "CareRequirements");
migrationBuilder.AddColumn<double>(
name: "LocationLatitude",
table: "Users",
type: "float",
nullable: true);
migrationBuilder.AddColumn<double>(
name: "LocationLongitude",
table: "Users",
type: "float",
nullable: true);
migrationBuilder.AlterColumn<double>(
name: "SearchRadius",
table: "UserPreferences",
type: "float",
nullable: false,
oldClrType: typeof(int),
oldType: "int");
migrationBuilder.AddColumn<string>(
name: "Category",
table: "Plants",
type: "nvarchar(100)",
maxLength: 100,
nullable: false,
defaultValue: "");
migrationBuilder.CreateIndex(
name: "IX_Users_LocationLatitude_LocationLongitude",
table: "Users",
columns: new[] { "LocationLatitude", "LocationLongitude" });
}
}
}
//20241213060227_AddFilters.Designer
﻿
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
[DbContext(typeof(CuttrDbContext))]
[Migration("20241213060227_AddFilters")]
partial class AddFilters
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
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Description")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("Extras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ImageUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("IndoorOutdoor")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("LightRequirement")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PetFriendly")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantCategory")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantStage")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PropagationEase")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("Size")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
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
b.Property<string>("WateringNeed")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
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
b.ToTable("Users");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.Property<int>("UserId")
.HasColumnType("int");
b.Property<string>("PreferedExtras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedIndoorOutdoor")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedLightRequirement")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPetFriendly")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantCategory")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantStage")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPropagationEase")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedSize")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedWateringNeed")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("SearchRadius")
.HasColumnType("int");
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
//20241218113032_AddRefreshTooken
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
public partial class AddRefreshTooken : Migration
{
protected override void Up(MigrationBuilder migrationBuilder)
{
migrationBuilder.CreateTable(
name: "RefreshTokens",
columns: table => new
{
RefreshTokenId = table.Column<int>(type: "int", nullable: false)
.Annotation("SqlServer:Identity", "1, 1"),
UserId = table.Column<int>(type: "int", nullable: false),
TokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
IsRevoked = table.Column<bool>(type: "bit", nullable: false),
CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
},
constraints: table =>
{
table.PrimaryKey("PK_RefreshTokens", x => x.RefreshTokenId);
table.ForeignKey(
name: "FK_RefreshTokens_Users_UserId",
column: x => x.UserId,
principalTable: "Users",
principalColumn: "UserId",
onDelete: ReferentialAction.Cascade);
});
migrationBuilder.CreateIndex(
name: "IX_RefreshTokens_UserId",
table: "RefreshTokens",
column: "UserId");
}
protected override void Down(MigrationBuilder migrationBuilder)
{
migrationBuilder.DropTable(
name: "RefreshTokens");
}
}
}
//20241218113032_AddRefreshTooken.Designer
﻿
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
[DbContext(typeof(CuttrDbContext))]
[Migration("20241218113032_AddRefreshTooken")]
partial class AddRefreshTooken
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
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Description")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("Extras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ImageUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("IndoorOutdoor")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("LightRequirement")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PetFriendly")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantCategory")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantStage")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PropagationEase")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("Size")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
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
b.Property<string>("WateringNeed")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.HasKey("PlantId");
b.HasIndex("UserId");
b.ToTable("Plants");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.RefreshTokenEF", b =>
{
b.Property<int>("RefreshTokenId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RefreshTokenId"));
b.Property<DateTime>("CreatedAt")
.HasColumnType("datetime2");
b.Property<DateTime>("ExpiresAt")
.HasColumnType("datetime2");
b.Property<bool>("IsRevoked")
.HasColumnType("bit");
b.Property<DateTime?>("RevokedAt")
.HasColumnType("datetime2");
b.Property<string>("TokenHash")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("UserId")
.HasColumnType("int");
b.HasKey("RefreshTokenId");
b.HasIndex("UserId");
b.ToTable("RefreshTokens");
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
b.ToTable("Users");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.Property<int>("UserId")
.HasColumnType("int");
b.Property<string>("PreferedExtras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedIndoorOutdoor")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedLightRequirement")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPetFriendly")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantCategory")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantStage")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPropagationEase")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedSize")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedWateringNeed")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("SearchRadius")
.HasColumnType("int");
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
modelBuilder.Entity("Cuttr.Infrastructure.Entities.RefreshTokenEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User")
.WithMany()
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
//20241228121555_updateUserEF
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
public partial class updateUserEF : Migration
{
protected override void Up(MigrationBuilder migrationBuilder)
{
}
protected override void Down(MigrationBuilder migrationBuilder)
{
}
}
}
//20241228121555_updateUserEF.Designer
﻿
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
[DbContext(typeof(CuttrDbContext))]
[Migration("20241228121555_updateUserEF")]
partial class updateUserEF
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
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Description")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("Extras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ImageUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("IndoorOutdoor")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("LightRequirement")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PetFriendly")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantCategory")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantStage")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PropagationEase")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("Size")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
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
b.Property<string>("WateringNeed")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.HasKey("PlantId");
b.HasIndex("UserId");
b.ToTable("Plants");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.RefreshTokenEF", b =>
{
b.Property<int>("RefreshTokenId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RefreshTokenId"));
b.Property<DateTime>("CreatedAt")
.HasColumnType("datetime2");
b.Property<DateTime>("ExpiresAt")
.HasColumnType("datetime2");
b.Property<bool>("IsRevoked")
.HasColumnType("bit");
b.Property<DateTime?>("RevokedAt")
.HasColumnType("datetime2");
b.Property<string>("TokenHash")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("UserId")
.HasColumnType("int");
b.HasKey("RefreshTokenId");
b.HasIndex("UserId");
b.ToTable("RefreshTokens");
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
b.ToTable("Users");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.Property<int>("UserId")
.HasColumnType("int");
b.Property<string>("PreferedExtras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedIndoorOutdoor")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedLightRequirement")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPetFriendly")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantCategory")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantStage")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPropagationEase")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedSize")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedWateringNeed")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("SearchRadius")
.HasColumnType("int");
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
modelBuilder.Entity("Cuttr.Infrastructure.Entities.RefreshTokenEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User")
.WithMany()
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
//20241228121717_updateUserEF2
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
public partial class updateUserEF2 : Migration
{
protected override void Up(MigrationBuilder migrationBuilder)
{
migrationBuilder.AlterColumn<string>(
name: "ProfilePictureUrl",
table: "Users",
type: "nvarchar(max)",
nullable: true,
oldClrType: typeof(string),
oldType: "nvarchar(max)");
migrationBuilder.AlterColumn<Point>(
name: "Location",
table: "Users",
type: "geography",
nullable: true,
oldClrType: typeof(Point),
oldType: "geography");
migrationBuilder.AlterColumn<string>(
name: "Bio",
table: "Users",
type: "nvarchar(500)",
maxLength: 500,
nullable: true,
oldClrType: typeof(string),
oldType: "nvarchar(500)",
oldMaxLength: 500);
}
protected override void Down(MigrationBuilder migrationBuilder)
{
migrationBuilder.AlterColumn<string>(
name: "ProfilePictureUrl",
table: "Users",
type: "nvarchar(max)",
nullable: false,
defaultValue: "",
oldClrType: typeof(string),
oldType: "nvarchar(max)",
oldNullable: true);
migrationBuilder.AlterColumn<Point>(
name: "Location",
table: "Users",
type: "geography",
nullable: false,
oldClrType: typeof(Point),
oldType: "geography",
oldNullable: true);
migrationBuilder.AlterColumn<string>(
name: "Bio",
table: "Users",
type: "nvarchar(500)",
maxLength: 500,
nullable: false,
defaultValue: "",
oldClrType: typeof(string),
oldType: "nvarchar(500)",
oldMaxLength: 500,
oldNullable: true);
}
}
}
//20241228121717_updateUserEF2.Designer
﻿
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
[DbContext(typeof(CuttrDbContext))]
[Migration("20241228121717_updateUserEF2")]
partial class updateUserEF2
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
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Description")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("Extras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ImageUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("IndoorOutdoor")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("LightRequirement")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PetFriendly")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantCategory")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantStage")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PropagationEase")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("Size")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
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
b.Property<string>("WateringNeed")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.HasKey("PlantId");
b.HasIndex("UserId");
b.ToTable("Plants");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.RefreshTokenEF", b =>
{
b.Property<int>("RefreshTokenId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RefreshTokenId"));
b.Property<DateTime>("CreatedAt")
.HasColumnType("datetime2");
b.Property<DateTime>("ExpiresAt")
.HasColumnType("datetime2");
b.Property<bool>("IsRevoked")
.HasColumnType("bit");
b.Property<DateTime?>("RevokedAt")
.HasColumnType("datetime2");
b.Property<string>("TokenHash")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("UserId")
.HasColumnType("int");
b.HasKey("RefreshTokenId");
b.HasIndex("UserId");
b.ToTable("RefreshTokens");
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
.HasColumnType("geography");
b.Property<string>("Name")
.IsRequired()
.HasMaxLength(100)
.HasColumnType("nvarchar(100)");
b.Property<string>("PasswordHash")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ProfilePictureUrl")
.HasColumnType("nvarchar(max)");
b.Property<DateTime>("UpdatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.HasKey("UserId");
b.HasIndex("Email")
.IsUnique();
b.ToTable("Users");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.Property<int>("UserId")
.HasColumnType("int");
b.Property<string>("PreferedExtras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedIndoorOutdoor")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedLightRequirement")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPetFriendly")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantCategory")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantStage")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPropagationEase")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedSize")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedWateringNeed")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("SearchRadius")
.HasColumnType("int");
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
modelBuilder.Entity("Cuttr.Infrastructure.Entities.RefreshTokenEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User")
.WithMany()
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
//20250110130658_updatePlantEF
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
public partial class updatePlantEF : Migration
{
protected override void Up(MigrationBuilder migrationBuilder)
{
migrationBuilder.AlterColumn<string>(
name: "Description",
table: "Plants",
type: "nvarchar(max)",
nullable: true,
oldClrType: typeof(string),
oldType: "nvarchar(max)");
}
protected override void Down(MigrationBuilder migrationBuilder)
{
migrationBuilder.AlterColumn<string>(
name: "Description",
table: "Plants",
type: "nvarchar(max)",
nullable: false,
defaultValue: "",
oldClrType: typeof(string),
oldType: "nvarchar(max)",
oldNullable: true);
}
}
}
//20250110130658_updatePlantEF.Designer
﻿
#nullable disable
namespace Cuttr.Infrastructure.Migrations
{
[DbContext(typeof(CuttrDbContext))]
[Migration("20250110130658_updatePlantEF")]
partial class updatePlantEF
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
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Description")
.HasColumnType("nvarchar(max)");
b.Property<string>("Extras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ImageUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("IndoorOutdoor")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("LightRequirement")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PetFriendly")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantCategory")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantStage")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PropagationEase")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("Size")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
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
b.Property<string>("WateringNeed")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.HasKey("PlantId");
b.HasIndex("UserId");
b.ToTable("Plants");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.RefreshTokenEF", b =>
{
b.Property<int>("RefreshTokenId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RefreshTokenId"));
b.Property<DateTime>("CreatedAt")
.HasColumnType("datetime2");
b.Property<DateTime>("ExpiresAt")
.HasColumnType("datetime2");
b.Property<bool>("IsRevoked")
.HasColumnType("bit");
b.Property<DateTime?>("RevokedAt")
.HasColumnType("datetime2");
b.Property<string>("TokenHash")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("UserId")
.HasColumnType("int");
b.HasKey("RefreshTokenId");
b.HasIndex("UserId");
b.ToTable("RefreshTokens");
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
.HasColumnType("geography");
b.Property<string>("Name")
.IsRequired()
.HasMaxLength(100)
.HasColumnType("nvarchar(100)");
b.Property<string>("PasswordHash")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ProfilePictureUrl")
.HasColumnType("nvarchar(max)");
b.Property<DateTime>("UpdatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.HasKey("UserId");
b.HasIndex("Email")
.IsUnique();
b.ToTable("Users");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.Property<int>("UserId")
.HasColumnType("int");
b.Property<string>("PreferedExtras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedIndoorOutdoor")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedLightRequirement")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPetFriendly")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantCategory")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantStage")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPropagationEase")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedSize")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedWateringNeed")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("SearchRadius")
.HasColumnType("int");
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
modelBuilder.Entity("Cuttr.Infrastructure.Entities.RefreshTokenEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User")
.WithMany()
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
b.Property<DateTime>("CreatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.Property<string>("Description")
.HasColumnType("nvarchar(max)");
b.Property<string>("Extras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ImageUrl")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("IndoorOutdoor")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("LightRequirement")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PetFriendly")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantCategory")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PlantStage")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("PropagationEase")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.Property<string>("Size")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
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
b.Property<string>("WateringNeed")
.IsRequired()
.HasMaxLength(50)
.HasColumnType("nvarchar(50)");
b.HasKey("PlantId");
b.HasIndex("UserId");
b.ToTable("Plants");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.RefreshTokenEF", b =>
{
b.Property<int>("RefreshTokenId")
.ValueGeneratedOnAdd()
.HasColumnType("int");
SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RefreshTokenId"));
b.Property<DateTime>("CreatedAt")
.HasColumnType("datetime2");
b.Property<DateTime>("ExpiresAt")
.HasColumnType("datetime2");
b.Property<bool>("IsRevoked")
.HasColumnType("bit");
b.Property<DateTime?>("RevokedAt")
.HasColumnType("datetime2");
b.Property<string>("TokenHash")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("UserId")
.HasColumnType("int");
b.HasKey("RefreshTokenId");
b.HasIndex("UserId");
b.ToTable("RefreshTokens");
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
.HasColumnType("geography");
b.Property<string>("Name")
.IsRequired()
.HasMaxLength(100)
.HasColumnType("nvarchar(100)");
b.Property<string>("PasswordHash")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("ProfilePictureUrl")
.HasColumnType("nvarchar(max)");
b.Property<DateTime>("UpdatedAt")
.ValueGeneratedOnAdd()
.HasColumnType("datetime2")
.HasDefaultValueSql("GETUTCDATE()");
b.HasKey("UserId");
b.HasIndex("Email")
.IsUnique();
b.ToTable("Users");
});
modelBuilder.Entity("Cuttr.Infrastructure.Entities.UserPreferencesEF", b =>
{
b.Property<int>("UserId")
.HasColumnType("int");
b.Property<string>("PreferedExtras")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedIndoorOutdoor")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedLightRequirement")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPetFriendly")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantCategory")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPlantStage")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedPropagationEase")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedSize")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<string>("PreferedWateringNeed")
.IsRequired()
.HasColumnType("nvarchar(max)");
b.Property<int>("SearchRadius")
.HasColumnType("int");
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
modelBuilder.Entity("Cuttr.Infrastructure.Entities.RefreshTokenEF", b =>
{
b.HasOne("Cuttr.Infrastructure.Entities.UserEF", "User")
.WithMany()
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+c8c7fba12c66504407d5c072b603348b48e662ce")]
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
.AsNoTracking()
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
.AsNoTracking()
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
_context.Entry(efMatch).State = EntityState.Detached;
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
_context.Entry(efMessage).State = EntityState.Detached;
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
.AsNoTracking()
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
await _context.Plants.AddAsync(efPlant);
await _context.SaveChangesAsync();
_context.Entry(efPlant).State = EntityState.Detached;
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
var efPlant = await _context.Plants.AsNoTracking()
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
_context.Entry(efPlant).State = EntityState.Detached;
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
_context.Entry(efPlant).State = EntityState.Detached;
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
var efPlants = await _context.Plants.AsNoTracking()
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
var efPlants = await _context.Plants.AsNoTracking()
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
.AsNoTracking()
.Include(p => p.User)
.Where(p => p.User.Location != null && p.User.Location.Distance(origin) <= radiusMeters)
.ToListAsync();
return efPlants.Select(EFToBusinessMapper.MapToPlant);
}
}
}
//RefreshTokenRepository
namespace Cuttr.Infrastructure.Repositories
{
public class RefreshTokenRepository : IRefreshTokenRepository
{
private readonly CuttrDbContext _context;
private readonly ILogger<RefreshTokenRepository> _logger;
public RefreshTokenRepository(CuttrDbContext context, ILogger<RefreshTokenRepository> logger)
{
_context = context;
_logger = logger;
}
public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken token)
{
try
{
var ef = new RefreshTokenEF
{
UserId = token.UserId,
TokenHash = token.TokenHash,
ExpiresAt = token.ExpiresAt,
IsRevoked = token.IsRevoked,
CreatedAt = token.CreatedAt,
RevokedAt = token.RevokedAt
};
_context.RefreshTokens.Add(ef);
await _context.SaveChangesAsync();
_context.Entry(ef).State = EntityState.Detached;
token.RefreshTokenId = ef.RefreshTokenId;
return token;
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while creating a refresh token for user with ID {UserId}.", token.UserId);
throw new RepositoryException("An error occurred while creating a refresh token.", ex);
}
}
public async Task<RefreshToken> GetRefreshTokenAsync(string tokenHash)
{
try
{
var ef = await _context.RefreshTokens.AsNoTracking()
.FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);
if (ef == null)
{
_logger.LogWarning("No valid refresh token found for the provided token hash.");
return null;
}
return new RefreshToken
{
RefreshTokenId = ef.RefreshTokenId,
UserId = ef.UserId,
TokenHash = ef.TokenHash,
ExpiresAt = ef.ExpiresAt,
IsRevoked = ef.IsRevoked,
CreatedAt = ef.CreatedAt,
RevokedAt = ef.RevokedAt
};
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while retrieving a refresh token by hash.");
throw new RepositoryException("An error occurred while retrieving a refresh token.", ex);
}
}
public async Task RevokeRefreshTokenAsync(string tokenHash)
{
try
{
var ef = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
if (ef != null && !ef.IsRevoked)
{
ef.IsRevoked = true;
ef.RevokedAt = DateTime.UtcNow;
await _context.SaveChangesAsync();
_context.Entry(ef).State = EntityState.Detached;
}
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while revoking the refresh token for token hash: {TokenHash}", tokenHash);
throw new RepositoryException("An error occurred while revoking the refresh token.", ex);
}
}
public async Task DeleteRefreshTokensForUserAsync(int userId)
{
try
{
var tokens = _context.RefreshTokens.Where(t => t.UserId == userId);
_context.RefreshTokens.RemoveRange(tokens);
await _context.SaveChangesAsync();
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while deleting refresh tokens for user with ID {UserId}.", userId);
throw new RepositoryException("An error occurred while deleting refresh tokens.", ex);
}
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
_context.Entry(efReport).State = EntityState.Detached;
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
_context.Entry(efSwipe).State = EntityState.Detached;
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
var efSwipe = await _context.Swipes
.AsNoTracking()
.FirstOrDefaultAsync(s =>
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
var exists = await _context.Swipes.AsNoTracking().AnyAsync(s =>
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
var efPreferences = await _context.UserPreferences.AsNoTracking()
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
_context.Entry(efPreferences).State = EntityState.Detached;
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
var efPreferences = BusinessToEFMapper.MapToUserPreferencesEF(preferences);
_context.UserPreferences.Update(efPreferences);
await _context.SaveChangesAsync();
_context.Entry(efPreferences).State = EntityState.Detached;
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
_context.Entry(efUser).State = EntityState.Detached;
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
var efUser = await _context.Users.AsNoTracking()
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
var efUser = await _context.Users.AsNoTracking()
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
public async Task UpdateUserNameAndBioAsync(User user)
{
try
{
var efUser = BusinessToEFMapper.MapToUserEF(user);
_context.Users.Update(efUser);
await _context.SaveChangesAsync();
_context.Entry(efUser).State = EntityState.Detached;
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
_context.Entry(efUser).State = EntityState.Detached;
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
_context.Entry(efUser).State = EntityState.Detached;
}
}
}