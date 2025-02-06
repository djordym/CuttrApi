
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
builder.Configuration.GetConnectionString("CuttrDb3"),
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
builder.Services.AddScoped<IConnectionManager, ConnectionManager>();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();
builder.Services.AddScoped<IConnectionRepository, ConnectionRepository>();
builder.Services.AddScoped<ITradeProposalRepository, TradeProposalRepository>();
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
//ConnectionController
namespace Cuttr.Api.Controllers
{
[ApiController]
[Route("api/connections")]
[Authorize]
public class ConnectionController : ControllerBase
{
private readonly IConnectionManager _connectionManager;
private readonly ILogger<ConnectionController> _logger;
private readonly IMessageManager _messageManager;
public ConnectionController(
IConnectionManager connectionManager,
ILogger<ConnectionController> logger,
IMessageManager messageManager
)
{
_connectionManager = connectionManager;
_logger = logger;
_messageManager = messageManager;
}
[HttpGet("me")]
public async Task<IActionResult> GetMyConnections()
{
try
{
int userId = User.GetUserId();
var connections = await _connectionManager.GetConnectionsForUserAsync(userId);
return Ok(connections);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error retrieving connections.");
return BadRequest(ex.Message);
}
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while retrieving connections.");
return StatusCode(500, "An unexpected error occurred.");
}
}
[HttpGet("{connectionId}")]
public async Task<IActionResult> GetConnection(int connectionId)
{
try
{
int userId = User.GetUserId();
var connection = await _connectionManager.GetConnectionByIdAsync(connectionId, userId);
return Ok(connection);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"Connection with ID {connectionId} not found.");
return NotFound(ex.Message);
}
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access to connection.");
return Forbid(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving connection with ID {connectionId}.");
return BadRequest(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while retrieving the connection.");
return StatusCode(500, "An unexpected error occurred.");
}
}
[HttpGet("{connectionId}/proposals")]
public async Task<IActionResult> GetTradeProposals(int connectionId)
{
try
{
int userId = User.GetUserId();
var proposals = await _connectionManager.GetTradeProposalsAsync(connectionId, userId);
return Ok(proposals);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"Connection with ID {connectionId} not found.");
return NotFound(ex.Message);
}
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access to trade proposals.");
return Forbid(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving trade proposals for connection {connectionId}.");
return BadRequest(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while retrieving trade proposals.");
return StatusCode(500, "An unexpected error occurred.");
}
}
[HttpPost("{connectionId}/proposals")]
public async Task<IActionResult> CreateTradeProposal(int connectionId, [FromBody] TradeProposalRequest request)
{
try
{
int userId = User.GetUserId();
var createdProposal = await _connectionManager.CreateTradeProposalAsync(connectionId, userId, request);
return Ok(createdProposal);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"Connection with ID {connectionId} not found.");
return NotFound(ex.Message);
}
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt to create a trade proposal.");
return Forbid(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error creating trade proposal.");
return BadRequest(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while creating the trade proposal.");
return StatusCode(500, "An unexpected error occurred.");
}
}
[HttpPut("{connectionId}/proposals/{proposalId}/status")]
public async Task<IActionResult> UpdateTradeProposalStatus(
int connectionId,
int proposalId,
[FromBody] UpdateTradeProposalStatusRequest request
)
{
try
{
int userId = User.GetUserId();
await _connectionManager.UpdateTradeProposalStatusAsync(
connectionId,
proposalId,
userId,
request
);
return NoContent();
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"Proposal with ID {proposalId} or Connection with ID {connectionId} not found.");
return NotFound(ex.Message);
}
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt to update trade proposal status.");
return Forbid(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error updating trade proposal status.");
return BadRequest(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while updating the trade proposal status.");
return StatusCode(500, "An unexpected error occurred.");
}
}
[HttpGet("{connectionId}/messages")]
public async Task<IActionResult> GetMessages(int connectionId)
{
int userId = 0;
try
{
userId = User.GetUserId();
var messages = await _messageManager.GetMessagesByConnectionIdAsync(connectionId, userId);
return Ok(messages);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"Match with ID {connectionId} not found.");
return NotFound(ex.Message);
}
catch (Business.Exceptions.UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access to messages.");
return Forbid(ex.Message);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving messages for match with ID {connectionId}.");
return BadRequest(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while accessing messages.");
return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
}
}
[HttpPost("{connectionId}/messages/user/me")]
public async Task<IActionResult> SendMessage([FromBody] MessageRequest request, int connectionId)
{
int senderUserId = 0;
try
{
senderUserId = User.GetUserId();
MessageResponse messageResponse = await _messageManager.SendMessageAsync(request, senderUserId, connectionId);
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
[HttpGet("connection/{connectionId}")]
public async Task<IActionResult> GetMatches(int connectionId)
{
try
{
var matches = await _matchManager.GetMatchesByConnectionIdAsync(connectionId);
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
public async Task<ActionResult<PlantResponse>> AddPlant([FromForm] PlantCreateRequest request)
{
int userId = 0;
try
{
userId = User.GetUserId();
PlantResponse plantResponse = await _plantManager.AddPlantAsync(request, userId);
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
public async Task<ActionResult<PlantResponse>> GetPlantById(int plantId)
{
try
{
PlantResponse plantResponse = await _plantManager.GetPlantByIdAsync(plantId);
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
public async Task<ActionResult<PlantResponse>> UpdatePlant(int plantId, [FromBody] PlantRequest request)
{
int userId = 0;
try
{
userId = User.GetUserId();
PlantResponse plantResponse = await _plantManager.UpdatePlantAsync(plantId, userId, request);
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
[HttpGet("users/{userId}")]
public async Task<ActionResult<List<PlantResponse>>> GetPlantsByUserId(int userId)
{
try
{
List<PlantResponse> plantResponses = await _plantManager.GetPlantsByUserIdAsync(userId);
return Ok(plantResponses);
}
catch (BusinessException ex)
{
_logger.LogError(ex, $"Error retrieving plants for user with ID {userId}.");
return BadRequest(ex.Message);
}
}
[HttpGet("users/me")]
public async Task<ActionResult<List<PlantResponse>>> GetPlantsOfUser()
{
int userId = 0;
try
{
userId = User.GetUserId();
List<PlantResponse> plantResponses = await _plantManager.GetPlantsByUserIdAsync(userId);
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
[HttpGet("likable")]
public async Task<ActionResult<List<PlantResponse>>> GetLikablePlants(int maxCount = 10)
{
int userId = 0;
try
{
userId = User.GetUserId();
List<PlantResponse> likablePlants = await _plantManager.GetLikablePlantsAsync(userId, maxCount);
return Ok(likablePlants);
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error retrieving likable plants.");
return BadRequest(ex.Message);
}
}
[HttpGet("liked-by-me/from/{userAId}")]
public async Task<ActionResult<List<PlantResponse>>> GetPlantsLikedByMeFromUser(int userAId)
{
try
{
int currentUserId = User.GetUserId();
List<PlantResponse> likedPlants = await _plantManager.GetPlantsLikedByMeFromUserAsync(userAId, currentUserId);
return Ok(likedPlants);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"User with ID {userAId} not found.");
return NotFound(new { error = ex.Message });
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error retrieving liked plants.");
return BadRequest(new { error = ex.Message });
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while retrieving liked plants.");
return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
}
}
[HttpGet("liked-by/{userAId}/from-me")]
public async Task<ActionResult<List<PlantResponse>>> GetPlantsLikedByUserFromMe(int userAId)
{
try
{
int currentUserId = User.GetUserId();
List<PlantResponse> plantsLikedByUser = await _plantManager.GetPlantsLikedByUserFromMeAsync(userAId, currentUserId);
return Ok(plantsLikedByUser);
}
catch (NotFoundException ex)
{
_logger.LogWarning(ex, $"User with ID {userAId} not found.");
return NotFound(new { error = ex.Message });
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error retrieving plants liked by user.");
return BadRequest(new { error = ex.Message });
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while retrieving plants liked by user.");
return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
}
}
[AllowAnonymous]
[HttpPost("seed")]
public async Task<IActionResult> SeedPlants([FromBody] List<SeedPlantRequest> request)
{
try
{
foreach (var plant in request)
{
await _plantManager.SeedPlantAsync(plant);
}
return Ok();
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error seeding plants.");
return BadRequest(ex.Message);
}
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt.");
return Unauthorized(ex.Message);
}
catch (Exception ex)
{
_logger.LogError(ex, "An unexpected error occurred while seeding plants.");
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
[AllowAnonymous]
[HttpPost("seed")]
public async Task<IActionResult> SeedRegisterUsers([FromBody] List<UserRegistrationRequest> requests)
{
try
{
foreach (var request in requests)
{
await _userManager.RegisterUserAsync(request);
}
return Ok();
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error seeding users.");
return BadRequest(ex.Message);
}
}
[AllowAnonymous]
[HttpPost("seed/locations")]
public async Task<IActionResult> SeedUpdateLocations([FromBody] List<UpdateLocationRequest> requests)
{
try
{
foreach (var request in requests)
{
await _userManager.UpdateUserLocationAsync(request.UserId, request.Latitude, request.Longitude);
}
return Ok();
}
catch (BusinessException ex)
{
_logger.LogError(ex, "Error seeding locations.");
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
//SeedPlantRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class SeedPlantRequest
{
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
public List<Extras>? Extras { get; set; } = new List<Extras>();
public string ImageUrl { get; set; }
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
//TradeProposalRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class TradeProposalRequest
{
public List<int> UserPlantIds { get; set; }
public List<int> OtherPlantIds { get; set; }
}
}
//UpdateTradeProposalStatusRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class UpdateTradeProposalStatusRequest
{
public TradeProposalStatus NewStatus { get; set; }
}
}
//UpdateUserLocationRequest
namespace Cuttr.Business.Contracts.Inputs
{
public class UpdateLocationRequest
{
public int UserId { get; set; }
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
//ConnectionResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class ConnectionResponse
{
public int ConnectionId { get; set; }
public UserResponse User1 { get; set; }
public UserResponse User2 { get; set; }
public int NumberOfMatches { get; set; }
}
}
//MatchResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class MatchResponse
{
public int MatchId { get; set; }
public int ConnectionId { get; set; }
public PlantResponse Plant1 { get; set; }
public PlantResponse Plant2 { get; set; }
}
}
//MessageResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class MessageResponse
{
public int MessageId { get; set; }
public int ConnectionId { get; set; }
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
public ConnectionResponse Connection { get; set; }
public MatchResponse Match { get; set; }
}
}
//TradeProposalResponse
namespace Cuttr.Business.Contracts.Outputs
{
public class TradeProposalResponse
{
public int TradeProposalId { get; set; }
public int ConnectionId { get; set; }
public List<PlantResponse>? PlantsProposedByUser1 { get; set; }
public List<PlantResponse>? PlantsProposedByUser2 { get; set; }
public TradeProposalStatus TradeProposalStatus { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime? AcceptedAt { get; set; }
public DateTime? DeclinedAt { get; set; }
public DateTime? CompletedAt { get; set; }
public ConnectionResponse Connection { get; set; }
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
//Connection
namespace Cuttr.Business.Entities
{
public class Connection
{
public int ConnectionId { get; set; }
public int UserId1 { get; set; }
public int UserId2 { get; set; }
public DateTime CreatedAt { get; set; }
public User User1 { get; set; }
public User User2 { get; set; }
public bool IsActive { get; set; }
public List<Message> Messages { get; set; }
public List<TradeProposal> TradeProposals { get; set; }
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
public int ConnectionId { get; set; }
public DateTime CreatedAt { get; set; }
public Plant Plant1 { get; set; }
public Plant Plant2 { get; set; }
}
}
//Message
namespace Cuttr.Business.Entities
{
public class Message
{
public int MessageId { get; set; }
public int ConnectionId { get; set; }
public int SenderUserId { get; set; }
public string MessageText { get; set; }
public DateTime SentAt { get; set; }
public bool IsRead { get; set; }
public Connection Connection { get; set; }
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
public bool IsClosed { get; set; }
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
//TradeProposal
namespace Cuttr.Business.Entities
{
public class TradeProposal
{
public int TradeProposalId { get; set; }
public int ConnectionId { get; set; }
public List<int> PlantIdsProposedByUser1 { get; set; }
public List<int> PlantIdsProposedByUser2 { get; set; }
public List<Plant> ItemsProposedByUser1 { get; set; }
public List<Plant> ItemsProposedByUser2 { get; set; }
public TradeProposalStatus TradeProposalStatus { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime? AcceptedAt { get; set; }
public DateTime? DeclinedAt { get; set; }
public DateTime? CompletedAt { get; set; }
public Connection Connection { get; set; }
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
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlantStage
{
Seedling,
Cutting,
Mature
}
[JsonConverter(typeof(JsonStringEnumConverter))]
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
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WateringNeed
{
VeryLowWater,
LowWater,
ModerateWater,
HighWater,
VeryHighWater
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LightRequirement
{
FullSun,
PartialSun,
BrightIndirectLight,
LowLight
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Size
{
SmallSize,
MediumSize,
LargeSize
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IndoorOutdoor
{
Indoor,
Outdoor,
IndoorAndOutdoor
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PropagationEase
{
EasyPropagation,
ModeratePropagation,
DifficultPropagation
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PetFriendly
{
PetFriendly,
NotPetFriendly
}
[JsonConverter(typeof(JsonStringEnumConverter))]
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
//TradeProposalStatus
namespace Cuttr.Business.Enums
{
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TradeProposalStatus
{
Pending, Accepted, Rejected, Completed
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
//IConnectionManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface IConnectionManager
{
Task<TradeProposalResponse> CreateTradeProposalAsync(int connectionId, int userId, TradeProposalRequest request);
Task<ConnectionResponse> GetConnectionByIdAsync(int connectionId, int userId);
Task<IEnumerable<ConnectionResponse>> GetConnectionsForUserAsync(int userId);
Task<IEnumerable<TradeProposalResponse>> GetTradeProposalsAsync(int connectionId, int userId);
Task UpdateTradeProposalStatusAsync(int connectionId, int proposalId, int userId, UpdateTradeProposalStatusRequest request);
}
}
//IMatchManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface IMatchManager
{
Task<IEnumerable<MatchResponse>> GetMatchesByConnectionIdAsync(int userId);
Task<MatchResponse> GetMatchByIdAsync(int matchId);
}
}
//IMessageManager
namespace Cuttr.Business.Interfaces.ManagerInterfaces
{
public interface IMessageManager
{
Task<MessageResponse> SendMessageAsync(MessageRequest request, int senderUserId, int connectionId);
Task<IEnumerable<MessageResponse>> GetMessagesByConnectionIdAsync(int connectionId, int senderUserId);
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
Task<List<PlantResponse>> GetPlantsByUserIdAsync(int userId);
Task<List<PlantResponse>> GetLikablePlantsAsync(int userId, int maxCount);
Task SeedPlantAsync(SeedPlantRequest plant);
Task<List<PlantResponse>> GetPlantsLikedByUserFromMeAsync(int userAId, int currentUserId);
Task<List<PlantResponse>> GetPlantsLikedByMeFromUserAsync(int userAId, int currentUserId);
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
//IConnectionRepository
namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
public interface IConnectionRepository
{
Task<Connection> GetConnectionByIdAsync(int connectionId);
Task<IEnumerable<Connection>> GetConnectionsByUserIdAsync(int userId);
Task<Connection> CreateConnectionAsync(Connection connection);
Task<Connection> UpdateConnectionAsync(Connection connection);
Task<Connection> GetConnectionByUsersAsync(int swiperUserId, int swipedUserId);
Task<int> GetNumberOfMatchesAsync(int connectionId);
}
}
//IMatchRepository
namespace Cuttr.Business.Interfaces.RepositoryInterfaces
{
public interface IMatchRepository
{
Task<IEnumerable<Match>> GetMatchesByConnectionIdAsync(int connectionId);
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
Task<IEnumerable<Message>> GetMessagesByConnectionIdAsync(int matchId);
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
Task<Swipe> GetSwipeForPairAsync(int swiperPlantId, int swipedPlantId);
Task UpdateSwipeAsync(Swipe swipe);
Task<IEnumerable<Plant>> GetLikedPlantsBySwiperAsync(int swiperUserId, int swipedPlantOwnerUserId);
}
}
//ITradeProposalRepository
namespace Cuttr.Business.Managers
{
public interface ITradeProposalRepository
{
Task<TradeProposal> GetByIdAsync(int proposalId);
Task<IEnumerable<TradeProposal>> GetByConnectionIdAsync(int connectionId);
Task<TradeProposal> CreateAsync(TradeProposal proposal);
Task<TradeProposal> UpdateAsync(TradeProposal proposal);
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
//ConnectionManager
namespace Cuttr.Business.Managers
{
public class ConnectionManager : IConnectionManager
{
private readonly IConnectionRepository _connectionRepository;
private readonly IMessageRepository _messageRepository;
private readonly ITradeProposalRepository _tradeProposalRepository;
private readonly IPlantRepository _plantRepository;
private readonly ILogger<ConnectionManager> _logger;
public ConnectionManager(
IConnectionRepository connectionRepository,
IMessageRepository messageRepository,
ITradeProposalRepository tradeProposalRepository,
IPlantRepository plantRepository,
ILogger<ConnectionManager> logger)
{
_connectionRepository = connectionRepository;
_messageRepository = messageRepository;
_tradeProposalRepository = tradeProposalRepository;
_plantRepository = plantRepository;
_logger = logger;
}
public async Task<IEnumerable<ConnectionResponse>> GetConnectionsForUserAsync(int userId)
{
_logger.LogInformation("Fetching connections for user with ID {UserId}.", userId);
try
{
var connections = await _connectionRepository.GetConnectionsByUserIdAsync(userId);
if (connections == null || !connections.Any())
{
_logger.LogInformation("No connections found for user with ID {UserId}.", userId);
return Enumerable.Empty<ConnectionResponse>();
}
var connResponses = connections
.Select(conn => BusinessToContractMapper.MapToConnectionResponse(conn))
.ToList();
foreach (var conn in connResponses)
{
conn.NumberOfMatches = await _connectionRepository.GetNumberOfMatchesAsync(conn.ConnectionId);
}
_logger.LogInformation("Successfully fetched connections for user with ID {UserId}.", userId);
return connResponses;
}
catch (Exception ex)
{
_logger.LogError(ex, "Error fetching connections for user with ID {UserId}.", userId);
throw new BusinessException("An error occurred while fetching connections.", ex);
}
}
public async Task<ConnectionResponse> GetConnectionByIdAsync(int connectionId, int userId)
{
_logger.LogInformation("Fetching connection with ID {ConnectionId} for user with ID {UserId}.", connectionId, userId);
try
{
var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
if (connection == null)
{
_logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
throw new NotFoundException($"Connection with ID {connectionId} not found.");
}
EnsureUserIsParticipantOfConnection(connection, userId);
var response = BusinessToContractMapper.MapToConnectionResponse(connection);
_logger.LogInformation("Successfully fetched connection with ID {ConnectionId} for user with ID {UserId}.", connectionId, userId);
return response;
}
catch (NotFoundException)
{
throw;
}
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt by user ID {UserId} to connection ID {ConnectionId}.", userId, connectionId);
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "Error fetching connection with ID {ConnectionId} for user with ID {UserId}.", connectionId, userId);
throw new BusinessException("An error occurred while fetching the connection.", ex);
}
}
public async Task<IEnumerable<TradeProposalResponse>> GetTradeProposalsAsync(int connectionId, int userId)
{
_logger.LogInformation("Fetching trade proposals for connection ID {ConnectionId} and user ID {UserId}.", connectionId, userId);
try
{
var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
if (connection == null)
{
_logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
throw new NotFoundException($"Connection with ID {connectionId} not found.");
}
EnsureUserIsParticipantOfConnection(connection, userId);
var proposals = await _tradeProposalRepository.GetByConnectionIdAsync(connectionId);
var response = proposals
.OrderByDescending(tp => tp.CreatedAt)
.Select(tp => BusinessToContractMapper.MapToTradeProposalResponse(tp));
_logger.LogInformation("Successfully fetched trade proposals for connection ID {ConnectionId}.", connectionId);
return response;
}
catch (NotFoundException)
{
throw;
}
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized access attempt by user ID {UserId} to trade proposals in connection ID {ConnectionId}.", userId, connectionId);
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "Error fetching trade proposals for connection ID {ConnectionId} and user ID {UserId}.", connectionId, userId);
throw new BusinessException("An error occurred while fetching trade proposals.", ex);
}
}
public async Task<TradeProposalResponse> CreateTradeProposalAsync(int connectionId, int userId, TradeProposalRequest request)
{
_logger.LogInformation("Creating trade proposal in connection ID {ConnectionId} by user ID {UserId}.", connectionId, userId);
try
{
var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
if (connection == null)
{
_logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
throw new NotFoundException($"Connection with ID {connectionId} not found.");
}
EnsureUserIsParticipantOfConnection(connection, userId);
var newProposal = new TradeProposal
{
ConnectionId = connectionId,
TradeProposalStatus = Enums.TradeProposalStatus.Pending,
CreatedAt = DateTime.UtcNow,
PlantIdsProposedByUser1 = request.UserPlantIds,
PlantIdsProposedByUser2 = request.OtherPlantIds
};
TradeProposal fulltradeprop = await _tradeProposalRepository.CreateAsync(newProposal);
_logger.LogInformation("Trade proposal created with ID {ProposalId} in connection ID {ConnectionId}.", newProposal.TradeProposalId, connectionId);
var response = BusinessToContractMapper.MapToTradeProposalResponse(newProposal);
_logger.LogInformation("Successfully created trade proposal with ID {ProposalId}.", newProposal.TradeProposalId);
return response;
}
catch (NotFoundException)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "Error creating trade proposal in connection ID {ConnectionId} by user ID {UserId}.", connectionId, userId);
throw new BusinessException("An error occurred while creating the trade proposal.", ex);
}
}
public async Task UpdateTradeProposalStatusAsync(int connectionId, int proposalId, int userId, UpdateTradeProposalStatusRequest request)
{
_logger.LogInformation("Updating trade proposal ID {ProposalId} status to {NewStatus} in connection ID {ConnectionId} by user ID {UserId}.", proposalId, request.NewStatus, connectionId, userId);
try
{
var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
if (connection == null)
{
_logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
throw new NotFoundException($"Connection with ID {connectionId} not found.");
}
EnsureUserIsParticipantOfConnection(connection, userId);
var proposal = await _tradeProposalRepository.GetByIdAsync(proposalId);
if (proposal == null || proposal.ConnectionId != connectionId)
{
_logger.LogWarning("Trade proposal with ID {ProposalId} not found in connection ID {ConnectionId}.", proposalId, connectionId);
throw new NotFoundException($"Trade proposal with ID {proposalId} not found in connection {connectionId}.");
}
switch (request.NewStatus)
{
case Enums.TradeProposalStatus.Accepted:
proposal.TradeProposalStatus = Enums.TradeProposalStatus.Accepted;
proposal.AcceptedAt = DateTime.UtcNow;
proposal.DeclinedAt = null;
proposal.CompletedAt = null;
break;
case Enums.TradeProposalStatus.Rejected:
proposal.TradeProposalStatus = Enums.TradeProposalStatus.Rejected;
proposal.DeclinedAt = DateTime.UtcNow;
proposal.AcceptedAt = null;
proposal.CompletedAt = null;
break;
case Enums.TradeProposalStatus.Completed:
if (proposal.TradeProposalStatus != Enums.TradeProposalStatus.Accepted)
{
_logger.LogWarning("Cannot mark proposal ID {ProposalId} as 'Completed' because it is not 'Accepted'.", proposalId);
throw new BusinessException("Cannot mark proposal as 'Completed' if it has not been 'Accepted'.");
}
proposal.TradeProposalStatus = Enums.TradeProposalStatus.Completed;
proposal.CompletedAt = DateTime.UtcNow;
await MarkPlantsAsTraded(proposal);
break;
default:
_logger.LogWarning("Invalid trade proposal status '{NewStatus}' provided for proposal ID {ProposalId}.", request.NewStatus, proposalId);
throw new BusinessException($"Unknown status '{request.NewStatus}'. Allowed: Accepted, Rejected, Completed.");
}
await _tradeProposalRepository.UpdateAsync(proposal);
_logger.LogInformation("Successfully updated trade proposal ID {ProposalId} to status {NewStatus}.", proposalId, request.NewStatus);
}
catch (NotFoundException)
{
throw;
}
catch (BusinessException)
{
throw;
}
catch (UnauthorizedAccessException ex)
{
_logger.LogWarning(ex, "Unauthorized attempt by user ID {UserId} to update trade proposal ID {ProposalId} in connection ID {ConnectionId}.", userId, proposalId, connectionId);
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, "Error updating trade proposal ID {ProposalId} in connection ID {ConnectionId} by user ID {UserId}.", proposalId, connectionId, userId);
throw new BusinessException("An error occurred while updating the trade proposal.", ex);
}
}
private void EnsureUserIsParticipantOfConnection(Connection connection, int userId)
{
if (connection.UserId1 != userId && connection.UserId2 != userId)
{
_logger.LogWarning("User ID {UserId} is not a participant of connection ID {ConnectionId}.", userId, connection.ConnectionId);
throw new UnauthorizedAccessException(
$"User {userId} is not a participant of connection {connection.ConnectionId}."
);
}
}
private async Task MarkPlantsAsTraded(TradeProposal proposal)
{
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
public async Task<IEnumerable<MatchResponse>> GetMatchesByConnectionIdAsync(int connectionId)
{
try
{
var matches = await _matchRepository.GetMatchesByConnectionIdAsync(connectionId);
return BusinessToContractMapper.MapToMatchResponse(matches);
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error retrieving matches for user with ID {connectionId}.");
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
private readonly IConnectionRepository _connectionRepository;
private readonly ILogger<MessageManager> _logger;
public MessageManager(
IMessageRepository messageRepository,
IConnectionRepository matchRepository,
ILogger<MessageManager> logger)
{
_messageRepository = messageRepository;
_connectionRepository = matchRepository;
_logger = logger;
}
public async Task<MessageResponse> SendMessageAsync(MessageRequest request, int senderUserId, int connectionId)
{
try
{
var match = await _connectionRepository.GetConnectionByIdAsync(connectionId);
if (match == null)
throw new NotFoundException($"Match with ID {connectionId} not found.");
if (match.UserId1 != senderUserId && match.UserId2 != senderUserId)
throw new BusinessException("Sender user is not part of the match.");
var message = ContractToBusinessMapper.MapToMessage(request, senderUserId, connectionId);
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
public async Task<IEnumerable<MessageResponse>> GetMessagesByConnectionIdAsync(int connectionId, int userId)
{
try
{
var connection = await _connectionRepository.GetConnectionByIdAsync(connectionId);
if (connection == null)
throw new NotFoundException($"Match with ID {connectionId} not found.");
if (connection.UserId1 != userId && connection.UserId2 != userId)
throw new Exceptions.UnauthorizedAccessException("User is not part of the match.");
var messages = await _messageRepository.GetMessagesByConnectionIdAsync(connectionId);
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
_logger.LogError(ex, $"Error retrieving messages for match with ID {connectionId}.");
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
private readonly ISwipeRepository _swipeRepository;
private readonly ILogger<PlantManager> _logger;
private readonly IBlobStorageService _blobStorageService;
private const string PlantImagesContainer = "plant-images";
public PlantManager(IPlantRepository plantRepository, IUserRepository userRepository, ISwipeRepository swipeRepository, ILogger<PlantManager> logger, IBlobStorageService blobStorageService)
{
_plantRepository = plantRepository;
_userRepository = userRepository;
_swipeRepository = swipeRepository;
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
public async Task<PlantResponse> UpdatePlantAsync(int plantId, int userId, PlantRequest request)
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
public async Task<List<PlantResponse>> GetPlantsByUserIdAsync(int userId)
{
try
{
var user = await _userRepository.GetUserByIdAsync(userId);
if (user == null)
{
throw new NotFoundException($"User with ID {userId} not found.");
}
var plants = await _plantRepository.GetPlantsByUserIdAsync(userId);
return BusinessToContractMapper.MapToPlantResponse(plants).ToList();
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
public async Task SeedPlantAsync(SeedPlantRequest request)
{
var plant = new Plant
{
UserId = request.UserId,
SpeciesName = request.SpeciesName,
Description = request.Description,
PlantStage = request.PlantStage,
WateringNeed = request.WateringNeed,
LightRequirement = request.LightRequirement,
Size = request.Size,
IndoorOutdoor = request.IndoorOutdoor,
PropagationEase = request.PropagationEase,
PetFriendly = request.PetFriendly,
Extras = request.Extras,
ImageUrl = request.ImageUrl
};
await _plantRepository.AddPlantAsync(plant);
return;
}
public async Task<List<PlantResponse>> GetPlantsLikedByUserFromMeAsync(int userAId, int currentUserId)
{
try
{
var userA = await _userRepository.GetUserByIdAsync(userAId);
if (userA == null)
{
throw new NotFoundException($"User with ID {userAId} not found.");
}
var userB = await _userRepository.GetUserByIdAsync(currentUserId);
if (userB == null)
{
throw new NotFoundException($"User with ID {currentUserId} not found.");
}
var likedPlants = await _swipeRepository.GetLikedPlantsBySwiperAsync(userAId, currentUserId);
return BusinessToContractMapper.MapToPlantResponse(likedPlants).ToList();
}
catch (NotFoundException ex)
{
throw;
}
catch (Exception ex)
{
_logger.LogError(ex, $"Error retrieving plants liked by user with ID {userAId}.");
throw new BusinessException("Error retrieving plants.", ex);
}
}
public async Task<List<PlantResponse>> GetPlantsLikedByMeFromUserAsync(int userAId, int currentUserId)
{
try
{
var userA = await _userRepository.GetUserByIdAsync(userAId);
if (userA == null)
{
throw new NotFoundException($"User with ID {userAId} not found.");
}
var userB = await _userRepository.GetUserByIdAsync(currentUserId);
if (userB == null)
{
throw new NotFoundException($"User with ID {currentUserId} not found.");
}
var likedPlants = await _swipeRepository.GetLikedPlantsBySwiperAsync(currentUserId, userAId);
return BusinessToContractMapper.MapToPlantResponse(likedPlants).ToList();
} catch (NotFoundException ex)
{
throw;
} catch (Exception ex)
{
_logger.LogError(ex, $"Error retrieving plants liked by user with ID {userAId}.");
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
private readonly IMatchRepository _matchRepository;
private readonly IConnectionRepository _connectionRepository;
public SwipeManager(
ISwipeRepository swipeRepository,
IPlantRepository plantRepository,
IUserRepository userRepository,
ILogger<SwipeManager> logger,
IMatchRepository matchRepository,
IConnectionRepository connectionRepository)
{
_swipeRepository = swipeRepository;
_plantRepository = plantRepository;
_logger = logger;
_userRepository = userRepository;
_matchRepository = matchRepository;
_connectionRepository = connectionRepository;
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
var existingSwipe = await _swipeRepository.GetSwipeForPairAsync(
request.SwiperPlantId,
request.SwipedPlantId
);
if (existingSwipe == null)
{
var newSwipe = ContractToBusinessMapper.MapToSwipe(request);
await _swipeRepository.AddSwipeAsync(newSwipe);
}
else
{
if (!existingSwipe.IsLike && request.IsLike)
{
existingSwipe.IsLike = true;
await _swipeRepository.UpdateSwipeAsync(existingSwipe);
}
}
bool finalIsLike = request.IsLike ||
(existingSwipe != null && existingSwipe.IsLike);
Swipe oppositeSwipe = null;
if (finalIsLike)
{
oppositeSwipe = await _swipeRepository.GetSwipeAsync(
request.SwipedPlantId,
request.SwiperPlantId,
true
);
}
SwipeResponse swipeResponse = new SwipeResponse { IsMatch = oppositeSwipe != null };
if (swipeResponse.IsMatch)
{
var swiperUserId = swiperPlant.UserId;
var swipedUserId = swipedPlant.UserId;
var existingConnection = await _connectionRepository.GetConnectionByUsersAsync(swiperUserId, swipedUserId);
Connection currentConnection;
if (existingConnection == null)
{
var newConnection = new Connection
{
UserId1 = swiperUserId,
UserId2 = swipedUserId,
CreatedAt = DateTime.UtcNow,
IsActive = true
};
currentConnection = await _connectionRepository.CreateConnectionAsync(newConnection);
swipeResponse.Connection = BusinessToContractMapper.MapToConnectionResponse(currentConnection);
}
else
{
currentConnection = existingConnection;
swipeResponse.Connection = BusinessToContractMapper.MapToConnectionResponse(existingConnection);
}
if (currentConnection == null || currentConnection.ConnectionId == 0)
throw new BusinessException("Connection not properly established.");
int plantId1, plantId2;
if (currentConnection.UserId1 == swiperUserId)
{
plantId1 = swiperPlant.PlantId;
plantId2 = swipedPlant.PlantId;
}
else
{
plantId1 = swipedPlant.PlantId;
plantId2 = swiperPlant.PlantId;
}
var match = new Match
{
PlantId1 = plantId1,
PlantId2 = plantId2,
ConnectionId = currentConnection.ConnectionId,
CreatedAt = DateTime.UtcNow
};
var addedMatch = await _matchRepository.AddMatchAsync(match);
if (addedMatch != null)
{
swipeResponse.Match = BusinessToContractMapper.MapToMatchResponse(addedMatch);
}
}
responses.Add(swipeResponse);
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
public static MessageResponse MapToMessageResponse(Message message)
{
if (message == null)
return null;
return new MessageResponse
{
MessageId = message.MessageId,
ConnectionId = message.ConnectionId,
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
public static TradeProposalResponse MapToTradeProposalResponse(TradeProposal tp)
{
if (tp == null)
return null;
return new TradeProposalResponse
{
TradeProposalId = tp.TradeProposalId,
ConnectionId = tp.ConnectionId,
PlantsProposedByUser1 = (tp.ItemsProposedByUser1).Select(MapToPlantResponse).ToList(),
PlantsProposedByUser2 = (tp.ItemsProposedByUser2).Select(MapToPlantResponse).ToList(),
TradeProposalStatus = tp.TradeProposalStatus,
CreatedAt = tp.CreatedAt,
AcceptedAt = tp.AcceptedAt,
DeclinedAt = tp.DeclinedAt,
CompletedAt = tp.CompletedAt,
Connection = MapToConnectionResponse(tp.Connection)
};
}
public static ConnectionResponse MapToConnectionResponse(Connection match)
{
if (match == null)
return null;
return new ConnectionResponse
{
ConnectionId = match.ConnectionId,
User1 = MapToUserResponse(match.User1),
User2 = MapToUserResponse(match.User2)
};
}
public static IEnumerable<ConnectionResponse> MapToConnectionResponse(IEnumerable<Connection> matches)
{
return matches?.Select(MapToConnectionResponse);
}
public static MatchResponse MapToMatchResponse(Match match)
{
if (match == null)
return null;
return new MatchResponse
{
MatchId = match.MatchId,
ConnectionId = match.ConnectionId,
Plant1 = MapToPlantResponse(match.Plant1),
Plant2 = MapToPlantResponse(match.Plant2),
};
}
public static IEnumerable<MatchResponse> MapToMatchResponse(IEnumerable<Match> matches)
{
return matches?.Select(MapToMatchResponse);
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
public static Message MapToMessage(MessageRequest request, int senderUserId, int connectionId)
{
if (request == null)
return null;
return new Message
{
ConnectionId = connectionId,
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
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+b7165f3d3284962074f8b39a274edeb786368631")]
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
public DbSet<ConnectionEF> Connections { get; set; }
public DbSet<TradeProposalEF> TradeProposals { get; set; }
public DbSet<TradeProposalPlantEF> TradeProposalPlants { get; set; }
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
entity.HasMany(u => u.RefreshTokens)
.WithOne(rt => rt.User)
.HasForeignKey(rt => rt.UserId)
.OnDelete(DeleteBehavior.Cascade);
});
modelBuilder.Entity<RefreshTokenEF>(entity =>
{
entity.HasKey(e => e.RefreshTokenId);
entity.Property(e => e.TokenHash)
.IsRequired()
.HasMaxLength(512);
entity.Property(e => e.ExpiresAt)
.IsRequired();
entity.Property(e => e.IsRevoked)
.HasDefaultValue(false);
entity.HasOne(e => e.User)
.WithMany(u => u.RefreshTokens)
.HasForeignKey(e => e.UserId)
.OnDelete(DeleteBehavior.Cascade);
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
entity.HasOne(m => m.Connection)
.WithMany(c => c.Matches)
.HasForeignKey(m => m.ConnectionId)
.OnDelete(DeleteBehavior.Restrict);
entity.HasIndex(m => new { m.PlantId1, m.PlantId2 })
.IsUnique();
});
modelBuilder.Entity<ConnectionEF>(entity =>
{
entity.HasKey(c => c.ConnectionId);
entity.HasOne(c => c.User1)
.WithMany()
.HasForeignKey(c => c.UserId1)
.OnDelete(DeleteBehavior.Restrict);
entity.HasOne(c => c.User2)
.WithMany()
.HasForeignKey(c => c.UserId2)
.OnDelete(DeleteBehavior.Restrict);
entity.HasMany(c => c.Messages)
.WithOne(m => m.Connection)
.HasForeignKey(m => m.ConnectionId)
.OnDelete(DeleteBehavior.Cascade);
entity.HasMany(c => c.TradeProposals)
.WithOne(tp => tp.Connection)
.HasForeignKey(tp => tp.ConnectionId)
.OnDelete(DeleteBehavior.Cascade);
});
modelBuilder.Entity<MessageEF>(entity =>
{
entity.HasKey(m => m.MessageId);
entity.Property(m => m.MessageText)
.IsRequired();
entity.HasOne(m => m.Connection)
.WithMany(conn => conn.Messages)
.HasForeignKey(m => m.ConnectionId)
.OnDelete(DeleteBehavior.Cascade);
entity.HasOne(m => m.SenderUser)
.WithMany(u => u.SentMessages)
.HasForeignKey(m => m.SenderUserId)
.OnDelete(DeleteBehavior.Restrict);
});
modelBuilder.Entity<TradeProposalEF>(entity =>
{
entity.HasKey(tp => tp.TradeProposalId);
entity.HasOne(tp => tp.Connection)
.WithMany(c => c.TradeProposals)
.HasForeignKey(tp => tp.ConnectionId)
.OnDelete(DeleteBehavior.Cascade);
});
modelBuilder.Entity<TradeProposalPlantEF>(entity =>
{
entity.HasKey(tpp => new { tpp.TradeProposalId, tpp.PlantId });
entity.HasOne(tpp => tpp.TradeProposal)
.WithMany(tp => tp.TradeProposalPlants)
.HasForeignKey(tpp => tpp.TradeProposalId)
.OnDelete(DeleteBehavior.Cascade);
entity.HasOne(tpp => tpp.Plant)
.WithMany()
.HasForeignKey(tpp => tpp.PlantId)
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
//ConnectionEF
namespace Cuttr.Infrastructure.Entities
{
public class ConnectionEF : ICreatedAt
{
[Key]
public int ConnectionId { get; set; }
[Required]
public int UserId1 { get; set; }
[Required]
public int UserId2 { get; set; }
public bool isActive { get; set; }
public DateTime CreatedAt { get; set; }
[ForeignKey("UserId1")]
public virtual UserEF User1 { get; set; }
[ForeignKey("UserId2")]
public virtual UserEF User2 { get; set; }
public virtual ICollection<MatchEF> Matches { get; set; }
public virtual ICollection<MessageEF> Messages { get; set; }
public List<TradeProposalEF> TradeProposals { get; set; }
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
public int ConnectionId { get; set; }
public DateTime CreatedAt { get; set; }
[ForeignKey("PlantId1")]
public virtual PlantEF Plant1 { get; set; }
[ForeignKey("PlantId2")]
public virtual PlantEF Plant2 { get; set; }
[ForeignKey("ConnectionId")]
public virtual ConnectionEF Connection { get; set; }
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
public int ConnectionId { get; set; }
[Required]
public int SenderUserId { get; set; }
[Required]
public string MessageText { get; set; }
public DateTime CreatedAt { get; set; }
public bool IsRead { get; set; }
[ForeignKey("ConnectionId")]
public virtual ConnectionEF Connection { get; set; }
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
public bool IsTraded { get; set; }
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
[Required]
public int UserId { get; set; }
[Required]
[MaxLength(512)]
public string TokenHash { get; set; }
[Required]
public DateTime ExpiresAt { get; set; }
public bool IsRevoked { get; set; }
[Required]
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
//TradeProposalEF
namespace Cuttr.Infrastructure.Entities
{
public class TradeProposalEF
{
[Key]
public int TradeProposalId { get; set; }
public int ConnectionId { get; set; }
public string TradeProposalStatus { get; set; }
public DateTime CreatedAt { get; set; }
public DateTime? AcceptedAt { get; set; }
public DateTime? DeclinedAt { get; set; }
public DateTime? CompletedAt { get; set; }
[ForeignKey("ConnectionId")]
public virtual ConnectionEF Connection { get; set; }
public virtual ICollection<TradeProposalPlantEF> TradeProposalPlants { get; set; } = new List<TradeProposalPlantEF>();
}
}
//TradeProposalPlantEF
namespace Cuttr.Infrastructure.Entities
{
public class TradeProposalPlantEF
{
public int TradeProposalId { get; set; }
public virtual TradeProposalEF TradeProposal { get; set; }
public int PlantId { get; set; }
public virtual PlantEF Plant { get; set; }
public bool IsProposedByUser1 { get; set; }
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
public virtual ICollection<RefreshTokenEF> RefreshTokens { get; set; }
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
ConnectionId = match.ConnectionId,
Plant1 = MapToPlantEFWithoutUser(match.Plant1),
Plant2 = MapToPlantEFWithoutUser(match.Plant2),
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
ConnectionId = message.ConnectionId,
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
public static ConnectionEF MapToConnectionEF(Connection match)
{
if (match == null)
return null;
return new ConnectionEF
{
ConnectionId = match.ConnectionId,
UserId1 = match.UserId1,
UserId2 = match.UserId2,
User1 = MapToUserEFWithoutPlants(match.User1),
User2 = MapToUserEFWithoutPlants(match.User2),
Messages = match.Messages?.Select(MapToMessageEF).ToList(),
CreatedAt = match.CreatedAt,
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
public static TradeProposalEF MapToTradeProposalEF(TradeProposal proposal)
{
if (proposal == null)
return null;
var ef = new TradeProposalEF
{
TradeProposalId = proposal.TradeProposalId,
ConnectionId = proposal.ConnectionId,
TradeProposalStatus = proposal.TradeProposalStatus.ToString(),
CreatedAt = proposal.CreatedAt,
AcceptedAt = proposal.AcceptedAt,
DeclinedAt = proposal.DeclinedAt,
CompletedAt = proposal.CompletedAt
};
if (proposal.PlantIdsProposedByUser1 != null)
{
foreach (var plantId in proposal.PlantIdsProposedByUser1)
{
ef.TradeProposalPlants.Add(new TradeProposalPlantEF
{
TradeProposalId = proposal.TradeProposalId,
PlantId = plantId,
IsProposedByUser1 = true
});
}
}
if (proposal.PlantIdsProposedByUser2 != null)
{
foreach (var plantId in proposal.PlantIdsProposedByUser2)
{
ef.TradeProposalPlants.Add(new TradeProposalPlantEF
{
TradeProposalId = proposal.TradeProposalId,
PlantId = plantId,
IsProposedByUser1 = false
});
}
}
return ef;
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
public static string SerializePlantIds(List<int> plantIds)
{
if (plantIds == null || !plantIds.Any())
return "";
return System.Text.Json.JsonSerializer.Serialize(plantIds);
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
public static Plant MapToPlantWithoutUser(PlantEF efPlant)
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
ConnectionId = efMatch.ConnectionId,
Plant1 = MapToPlantWithoutUser(efMatch.Plant1),
Plant2 = MapToPlantWithoutUser(efMatch.Plant2),
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
ConnectionId = efMessage.ConnectionId,
SenderUserId = efMessage.SenderUserId,
MessageText = efMessage.MessageText,
IsRead = efMessage.IsRead,
SentAt = efMessage.CreatedAt,
SenderUser = MapToUserWithoutPlants(efMessage.SenderUser),
};
}
public static Connection MapToConnection(ConnectionEF efConnection)
{
if (efConnection == null)
return null;
return new Connection
{
ConnectionId = efConnection.ConnectionId,
UserId1 = efConnection.UserId1,
UserId2 = efConnection.UserId2,
User1 = MapToUserWithoutPlants(efConnection.User1),
User2 = MapToUserWithoutPlants(efConnection.User2),
Messages = efConnection.Messages?.Select(MapToMessage).ToList(),
CreatedAt = efConnection.CreatedAt,
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
public static TradeProposal MapToTradeProposal(TradeProposalEF ef)
{
if (ef == null)
return null;
var tradeProposal = new TradeProposal
{
TradeProposalId = ef.TradeProposalId,
ConnectionId = ef.ConnectionId,
TradeProposalStatus = !string.IsNullOrWhiteSpace(ef.TradeProposalStatus)
? Enum.Parse<TradeProposalStatus>(ef.TradeProposalStatus)
: TradeProposalStatus.Pending,
CreatedAt = ef.CreatedAt,
AcceptedAt = ef.AcceptedAt,
DeclinedAt = ef.DeclinedAt,
CompletedAt = ef.CompletedAt,
Connection = MapToConnection(ef.Connection),
PlantIdsProposedByUser1 = ef.TradeProposalPlants
.Where(tpp => tpp.IsProposedByUser1)
.Select(tpp => tpp.PlantId)
.ToList(),
PlantIdsProposedByUser2 = ef.TradeProposalPlants
.Where(tpp => !tpp.IsProposedByUser1)
.Select(tpp => tpp.PlantId)
.ToList(),
ItemsProposedByUser1 = ef.TradeProposalPlants
.Where(tpp => tpp.IsProposedByUser1 && tpp.Plant != null)
.Select(tpp => MapToPlant(tpp.Plant))
.ToList(),
ItemsProposedByUser2 = ef.TradeProposalPlants
.Where(tpp => !tpp.IsProposedByUser1 && tpp.Plant != null)
.Select(tpp => MapToPlant(tpp.Plant))
.ToList()
};
return tradeProposal;
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
//.NETCoreApp,Version=v8.0.AssemblyAttributes
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]
//Cuttr.Infrastructure.AssemblyInfo
[assembly: System.Reflection.AssemblyCompanyAttribute("Cuttr.Infrastructure")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+b7165f3d3284962074f8b39a274edeb786368631")]
[assembly: System.Reflection.AssemblyProductAttribute("Cuttr.Infrastructure")]
[assembly: System.Reflection.AssemblyTitleAttribute("Cuttr.Infrastructure")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]
//Cuttr.Infrastructure.GlobalUsings.g

//EFCoreSqlServerNetTopologySuite
[assembly: Microsoft.EntityFrameworkCore.Design.DesignTimeServicesReferenceAttribute("Microsoft.EntityFrameworkCore.SqlServer.Design.Internal.SqlServerNetTopologySuite" +
"DesignTimeServices, Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite", "Microsoft.EntityFrameworkCore.SqlServer")]
//ConnectionRepository
namespace Cuttr.Infrastructure.Repositories
{
public class ConnectionRepository : IConnectionRepository
{
private readonly CuttrDbContext _context;
private readonly ILogger<ConnectionRepository> _logger;
public ConnectionRepository(CuttrDbContext context, ILogger<ConnectionRepository> logger)
{
_context = context;
_logger = logger;
}
public async Task<Connection> GetConnectionByIdAsync(int connectionId)
{
_logger.LogInformation("Retrieving connection with ID {ConnectionId}.", connectionId);
try
{
var efConnection = await _context.Connections
.Include(c => c.User1)
.Include(c => c.User2)
.FirstOrDefaultAsync(c => c.ConnectionId == connectionId);
if (efConnection == null)
{
_logger.LogWarning("Connection with ID {ConnectionId} not found.", connectionId);
return null;
}
_context.Entry(efConnection).State = EntityState.Detached;
var connection = EFToBusinessMapper.MapToConnection(efConnection);
_logger.LogInformation("Successfully retrieved connection with ID {ConnectionId}.", connectionId);
return connection;
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while retrieving connection with ID {ConnectionId}.", connectionId);
throw new RepositoryException("An error occurred while retrieving the connection.", ex);
}
}
public async Task<IEnumerable<Connection>> GetConnectionsByUserIdAsync(int userId)
{
_logger.LogInformation("Retrieving connections for user with ID {UserId}.", userId);
try
{
var efConnections = await _context.Connections
.Where(c => c.UserId1 == userId || c.UserId2 == userId)
.Include(c => c.User1)
.Include(c => c.User2)
.ToListAsync();
if (efConnections == null || !efConnections.Any())
{
_logger.LogInformation("No connections found for user with ID {UserId}.", userId);
return Enumerable.Empty<Connection>();
}
foreach (var efConnection in efConnections)
{
_context.Entry(efConnection).State = EntityState.Detached;
}
var connections = efConnections.Select(EFToBusinessMapper.MapToConnection);
_logger.LogInformation("Successfully retrieved {Count} connections for user with ID {UserId}.", connections.Count(), userId);
return connections;
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while retrieving connections for user with ID {UserId}.", userId);
throw new RepositoryException("An error occurred while retrieving connections.", ex);
}
}
public async Task<Connection> CreateConnectionAsync(Connection connection)
{
_logger.LogInformation("Creating a new connection between User1 ID {UserId1} and User2 ID {UserId2}.", connection.UserId1, connection.UserId2);
try
{
var efConnection = BusinessToEFMapper.MapToConnectionEF(connection);
await _context.Connections.AddAsync(efConnection);
await _context.SaveChangesAsync();
var efConnectionFull = await _context.Connections
.Include(c => c.User1)
.Include(c => c.User2)
.FirstOrDefaultAsync(c => c.ConnectionId == efConnection.ConnectionId);
_context.Entry(efConnection).State = EntityState.Detached;
var createdConnection = EFToBusinessMapper.MapToConnection(efConnectionFull);
_logger.LogInformation("Successfully created connection with ID {ConnectionId}.", createdConnection.ConnectionId);
return createdConnection;
}
catch (DbUpdateException ex)
{
_logger.LogError(ex, "A database error occurred while creating a new connection between User1 ID {UserId1} and User2 ID {UserId2}.", connection.UserId1, connection.UserId2);
throw new RepositoryException("A database error occurred while creating the connection.", ex);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while creating a new connection between User1 ID {UserId1} and User2 ID {UserId2}.", connection.UserId1, connection.UserId2);
throw new RepositoryException("An error occurred while creating the connection.", ex);
}
}
public async Task<Connection> UpdateConnectionAsync(Connection connection)
{
_logger.LogInformation("Updating connection with ID {ConnectionId}.", connection.ConnectionId);
try
{
var efConnection = BusinessToEFMapper.MapToConnectionEF(connection);
_context.Connections.Update(efConnection);
await _context.SaveChangesAsync();
_context.Entry(efConnection).State = EntityState.Detached;
var updatedConnection = EFToBusinessMapper.MapToConnection(efConnection);
_logger.LogInformation("Successfully updated connection with ID {ConnectionId}.", connection.ConnectionId);
return updatedConnection;
}
catch (DbUpdateConcurrencyException ex)
{
_logger.LogError(ex, "A concurrency error occurred while updating connection with ID {ConnectionId}.", connection.ConnectionId);
throw new RepositoryException("A concurrency error occurred while updating the connection.", ex);
}
catch (DbUpdateException ex)
{
_logger.LogError(ex, "A database error occurred while updating connection with ID {ConnectionId}.", connection.ConnectionId);
throw new RepositoryException("A database error occurred while updating the connection.", ex);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while updating connection with ID {ConnectionId}.", connection.ConnectionId);
throw new RepositoryException("An error occurred while updating the connection.", ex);
}
}
public async Task<Connection> GetConnectionByUsersAsync(int userId1, int userId2)
{
_logger.LogInformation("Retrieving connection between User1 ID {UserId1} and User2 ID {UserId2}.", userId1, userId2);
try
{
var efConnection = await _context.Connections
.Include(c => c.User1)
.Include(c => c.User2)
.FirstOrDefaultAsync(c =>
(c.UserId1 == userId1 && c.UserId2 == userId2) ||
(c.UserId1 == userId2 && c.UserId2 == userId1));
if (efConnection == null)
{
_logger.LogWarning("Connection between User1 ID {UserId1} and User2 ID {UserId2} not found.", userId1, userId2);
return null;
}
_context.Entry(efConnection).State = EntityState.Detached;
var connection = EFToBusinessMapper.MapToConnection(efConnection);
_logger.LogInformation("Successfully retrieved connection between User1 ID {UserId1} and User2 ID {UserId2}.", userId1, userId2);
return connection;
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while retrieving connection between User1 ID {UserId1} and User2 ID {UserId2}.", userId1, userId2);
throw new RepositoryException("An error occurred while retrieving the connection.", ex);
}
}
public async Task<int> GetNumberOfMatchesAsync(int connectionId)
{
_logger.LogInformation("Retrieving number of matches for connection ID {ConnectionId}.", connectionId);
try
{
var count = await _context.Matches
.Where(m => m.ConnectionId == connectionId)
.CountAsync();
_logger.LogInformation("Connection ID {ConnectionId} has {Count} matches.", connectionId, count);
return count;
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while retrieving number of matches for connection ID {ConnectionId}.", connectionId);
throw new RepositoryException("An error occurred while retrieving the number of matches.", ex);
}
}
}
}
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
public async Task<IEnumerable<Match>> GetMatchesByConnectionIdAsync(int connectionId)
{
try
{
var efMatches = await _context.Matches
.AsNoTracking()
.Include(m => m.Plant1)
.Include(m => m.Plant2)
.Where(m => m.ConnectionId == connectionId)
.ToListAsync();
return efMatches.Select(EFToBusinessMapper.MapToMatch);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while retrieving matches for connection with ID {connectionId}.");
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
.Include(m => m.Plant2)
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
await _context.Matches.AddAsync(efMatch);
await _context.SaveChangesAsync();
var efMatchFull = await _context.Matches
.AsNoTracking()
.Include(m => m.Plant1)
.Include(m => m.Plant2)
.FirstOrDefaultAsync(m => m.MatchId == efMatch.MatchId);
_context.Entry(efMatch).State = EntityState.Detached;
return EFToBusinessMapper.MapToMatch(efMatchFull);
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
public async Task<IEnumerable<Message>> GetMessagesByConnectionIdAsync(int connectionId)
{
try
{
var efMessages = await _context.Messages
.AsNoTracking()
.Where(m => m.ConnectionId == connectionId)
.OrderBy(m => m.CreatedAt)
.ToListAsync();
return efMessages.Select(EFToBusinessMapper.MapToMessage);
}
catch (Exception ex)
{
_logger.LogError(ex, $"An error occurred while retrieving messages for match with ID {connectionId}.");
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
_context.RefreshTokens.AddAsync(ef);
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
public async Task<Swipe> GetSwipeForPairAsync(int swiperPlantId, int swipedPlantId)
{
try
{
var efSwipe = await _context.Swipes
.AsNoTracking()
.FirstOrDefaultAsync(s =>
s.SwiperPlantId == swiperPlantId &&
s.SwipedPlantId == swipedPlantId);
return EFToBusinessMapper.MapToSwipe(efSwipe);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while retrieving a swipe for pair.");
throw new RepositoryException("An error occurred while retrieving a swipe for pair.", ex);
}
}
public async Task UpdateSwipeAsync(Swipe swipe)
{
try
{
var efSwipe = BusinessToEFMapper.MapToSwipeEF(swipe);
_context.Swipes.Update(efSwipe);
await _context.SaveChangesAsync();
_context.Entry(efSwipe).State = EntityState.Detached;
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while updating a swipe.");
throw new RepositoryException("An error occurred while updating a swipe.", ex);
}
}
public async Task<IEnumerable<Plant>> GetLikedPlantsBySwiperAsync(int swiperUserId, int swipedPlantOwnerUserId)
{
try
{
var likedPlants = await _context.Swipes
.AsNoTracking()
.Where(s =>
s.SwiperPlant.User.UserId == swiperUserId &&
s.SwipedPlant.User.UserId == swipedPlantOwnerUserId &&
s.IsLike)
.GroupBy(s => s.SwipedPlant.PlantId)
.Select(g => g.First().SwipedPlant)
.ToListAsync();
return likedPlants.Select(EFToBusinessMapper.MapToPlantWithoutUser);
}
catch (Exception ex)
{
_logger.LogError(ex, "An error occurred while retrieving liked plants.");
throw new RepositoryException("An error occurred while retrieving liked plants.", ex);
}
}
}
}
//TradeProposalRepository
namespace Cuttr.Infrastructure.Repositories
{
public class TradeProposalRepository : ITradeProposalRepository
{
private readonly CuttrDbContext _dbContext;
public TradeProposalRepository(CuttrDbContext dbContext)
{
_dbContext = dbContext;
}
public async Task<TradeProposal> GetByIdAsync(int proposalId)
{
var ef = await _dbContext.TradeProposals.AsNoTracking()
.Include(tp => tp.Connection)
.Include(tp => tp.TradeProposalPlants)
.ThenInclude(tpp => tpp.Plant)
.FirstOrDefaultAsync(tp => tp.TradeProposalId == proposalId);
if (ef == null)
return null;
return EFToBusinessMapper.MapToTradeProposal(ef);
}
public async Task<IEnumerable<TradeProposal>> GetByConnectionIdAsync(int connectionId)
{
var efList = await _dbContext.TradeProposals.AsNoTracking()
.Include(tp => tp.Connection)
.Include(tp => tp.TradeProposalPlants)
.ThenInclude(tpp => tpp.Plant)
.Where(tp => tp.ConnectionId == connectionId)
.OrderBy(tp => tp.CreatedAt)
.ToListAsync();
return efList.Select(EFToBusinessMapper.MapToTradeProposal);
}
public async Task<TradeProposal> CreateAsync(TradeProposal proposal)
{
var ef = BusinessToEFMapper.MapToTradeProposalEF(proposal);
_dbContext.TradeProposals.Add(ef);
await _dbContext.SaveChangesAsync();
return EFToBusinessMapper.MapToTradeProposal(ef);
}
public async Task<TradeProposal> UpdateAsync(TradeProposal proposal)
{
var ef = BusinessToEFMapper.MapToTradeProposalEF(proposal);
_dbContext.TradeProposals.Update(ef);
await _dbContext.SaveChangesAsync();
return EFToBusinessMapper.MapToTradeProposal(ef);
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