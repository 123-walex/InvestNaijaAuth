using AutoMapper;
using InvestNaijaAuth.Data;
using InvestNaijaAuth.DTO_s;
using Microsoft.AspNetCore.Mvc;
using InvestNaijaAuth.Servicies;
using InvestNaijaAuth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace InvestNaijaAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvestNaijaAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly InvestNaijaDBContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<InvestNaijaAuthController> _logger;
        private readonly ITokenService _tokenservice;

        public InvestNaijaAuthController(
                           IConfiguration configuration,
                           InvestNaijaDBContext context,
                           IMapper mapper,
                           ILogger<InvestNaijaAuthController> logger,
                           ITokenService tokenService
                           )
        {
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _tokenservice = tokenService;

        }
        [Authorize(Roles = "Admin,User")]
        [HttpPost("Signup")]
        public async Task<IActionResult> Signup(SignupDTO signup)
        {

            var requestId = HttpContext.TraceIdentifier;
            var userName = User.Identity?.Name ?? "Anonymous";
            _logger.LogInformation("Received SignUpUser request. TraceId: {requestId}, RequestedBy: {User}, Payload: {@SignUpDTO}", requestId, userName, signup);

            bool usernameExists = await _context.User.AnyAsync(u => u.Username == signup.Username);
            bool emailExists = await _context.User.AnyAsync(u => u.EmailAddress == signup.EmailAddress);
            bool passwordExists = await _context.User.AnyAsync(u => u.HashedPassword == signup.HashedPassword);

            if (usernameExists || emailExists || passwordExists)
            {
                var errors = new Dictionary<string, string>();

                if (usernameExists)
                    errors.Add("Username", "Username already exists.");

                if (emailExists)
                    errors.Add("EmailAddress", "Email already in use.");

                if (passwordExists)
                    errors.Add("HashedPassword", "Password is already in use");

                return BadRequest(new { Errors = errors });
            }

            if (signup == null)
            {
                _logger.LogError("No User information provided . TraceId: {requestId}, RequestedBy : {User} , Payload: {@SignUpDTO}", requestId, userName, signup);
                return BadRequest();
            }

            try
            {
                var SignUpEntity = _mapper.Map<User>(signup);
                SignUpEntity.CreatedAt = DateTime.UtcNow;
                SignUpEntity.WalletBalance = 30000;

                _context.User.Add(SignUpEntity);
                await _context.SaveChangesAsync();

                var SignUpresponse = new HashExclusionDTO
                {
                    Id = SignUpEntity.Id,
                    Username = SignUpEntity.Username,
                    EmailAddress = SignUpEntity.EmailAddress
                };

                _logger.LogInformation("User Successfully Created . TraceId : {requestId} , RequestedBy : {User} , Payload : {@SignUpDTO} , Username : {Username} , Id : {Id} ", requestId, userName, signup, SignUpEntity.Username, SignUpEntity.Id);
                return CreatedAtAction(nameof(Signup), new { Id = SignUpEntity.Id }, SignUpresponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during signup. TraceId: {requestId}, RequestedBy: {User}", requestId, userName);
                return StatusCode(500, "An error occurred while signing up.");
            }

        }
        [Authorize(Roles = "Admin,User")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            var requestId = HttpContext.TraceIdentifier;
            string accessToken = String.Empty;
            RefreshTokens refreshToken = null;

            _logger.LogInformation("Received User Login request. TraceId: {requestId}, RequestedBy: {EmailAddress}, Payload: {@LoginDTO}", requestId, login.EmailAddress, login);

            if (String.IsNullOrWhiteSpace(login.EmailAddress) || String.IsNullOrWhiteSpace(login.HashedPassword))
            {
                _logger.LogError("No User information provided . TraceId: {requestId}, RequestedBy : {EmailAddress} , Payload: {@LoginDTO}", requestId, login.EmailAddress, login);
                return BadRequest("Email and password are required");
            }

            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    var Loginentity = await _context.User
                        .FirstOrDefaultAsync(l => l.EmailAddress == login.EmailAddress && !l.IsDeleted);

                    if (Loginentity is null)
                    {
                        _logger.LogError("User doesn't Exist on the Database , TraceId : {requestId} , RequestedBy : {EmailAddress} , Payload : {@LoginDTO} ", requestId, login.EmailAddress, login);
                        return Unauthorized("Invalid Credentials !!!");
                    }

                    _logger.LogInformation("User Exists on the Database, TraceId : {requestId} , RequestedBy {Username} ,", requestId, Loginentity.Username);

                    if (login.HashedPassword == Loginentity.HashedPassword && login.EmailAddress == Loginentity.EmailAddress)
                    {
                        _logger.LogInformation("Credentials Successfully Verified , TraceId : {requestId} , RequestedBy : {Username} , Payload : {LoginDTO} ", requestId, Loginentity.Username, login);

                        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                        // Generate tokens
                        accessToken = _tokenservice.CreateAccessToken(Loginentity);
                        refreshToken = _tokenservice.CreateRefreshToken(ipAddress);

                        // Get or create user session
                        var userSession = await _context.UserSessions
                            .FirstOrDefaultAsync(s => s.EmailAddress == login.EmailAddress);

                        if (userSession == null)
                        {
                            userSession = new UserSessions
                            {
                                Id = Guid.NewGuid(),
                                User = Loginentity,
                                EmailAddress = Loginentity.EmailAddress
                            };
                            _context.UserSessions.Add(userSession);
                        }

                        // Update session
                        userSession.LoggedInAt = DateTime.UtcNow;
                        userSession.AccessSessionToken = accessToken;
                        userSession.RefreshSessionToken = refreshToken.RefreshToken;

                        // Save refresh token
                        refreshToken.UserId = Loginentity.Id;
                        _context.RefreshTokens.Add(refreshToken);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return Ok(new
                        {
                            AccessToken = accessToken,
                            RefreshToken = refreshToken.RefreshToken,
                            User = new
                            {
                                Id = Loginentity.Id,
                                Username = Loginentity.Username,
                                EmailAddress = Loginentity.EmailAddress
                            }
                        });
                    }
                    else
                    {
                        _logger.LogError("Invalid Credentials , TraceId : {requestId} , RequestedBy : {Username} , Payload : {LoginDTO} ", requestId, Loginentity.Username, login);
                        return Unauthorized();
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during login process. TraceId: {requestId}, RequestedBy: {EmailAddress}", requestId, login.EmailAddress);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login. TraceId: {requestId}, RequestedBy: {EmailAddress}", requestId, login.EmailAddress);
                return StatusCode(500, "An error occurred while processing your login request");
            }
        }
        [Authorize(Roles = "Admin,User")]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout(LogoutDTO logout)
        {
            var requestId = HttpContext.TraceIdentifier;
            _logger.LogInformation("Recieved User Logout Request , TraceId : {requestId} , RequestedBy : {EmailAddress} , Payload : {@LogoutDTO} . ", requestId, logout.EmailAddress, logout);

            if (String.IsNullOrWhiteSpace(logout.RefreshToken))
                return BadRequest("Refresh Token is required");

            try
            {
                var logoutEntity = await _context.User
                    .FirstOrDefaultAsync(u => u.EmailAddress == logout.EmailAddress);

                var StoredToken = await _context.RefreshTokens
                           .Include(s => s.User)
                                  .SingleOrDefaultAsync(s => s.RefreshToken == logout.RefreshToken);

                var logouttime = await _context.UserSessions
                          .FirstOrDefaultAsync(l => l.EmailAddress == logout.EmailAddress);

                if (logouttime is null)
                {
                    _logger.LogError("Session Table can't be accesed because the Entity is null. , TraceId : {requestId} , RequestedBy : {EmailAddress} ", requestId , logout.EmailAddress);
                    return Unauthorized();
                }
                if (logoutEntity is null)
                {
                    _logger.LogError("Users Table can't be accesed because the Entity is null , TraceId : {requestId}  , RequestedBy : {EmailAddress} .", requestId , logout.EmailAddress);
                    return Unauthorized();
                }
                if (StoredToken == null || !StoredToken.IsActive)
                {
                    _logger.LogWarning(" Invalid or already revoked refresh token , TraceId : {requestId} , RequestedBy : {EmailAddress} .", requestId , logout.EmailAddress);
                    return Unauthorized("Invalid refresh token.");
                }

                logouttime.LoggedOutAt = DateTime.UtcNow;
                logoutEntity.NoOfSessions += 1;
                StoredToken.Revoked = DateTime.UtcNow;
                StoredToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();

                _logger.LogInformation(" Successfully revoked refresh token , UserId : {UserId} , TraceId : {requestId} , RequestedBy : {EmailAddress} .",
                   requestId, StoredToken.UserId, logout.EmailAddress);

                await _context.SaveChangesAsync();

                _logger.LogInformation(" Successfully Logged User Out , UserId : {UserId} , TraceId : {requestId} , RequestedBy : {EmailAddress} .",
                   requestId, StoredToken.UserId, logout.EmailAddress);

                return Ok(new { message = "Logged out successfully." });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex ,"Unable to log out !!!! , TraceId {requestId} , RequestedBy : {EmailAddress} ", requestId , logout.EmailAddress);
                return StatusCode(500, "An error occurred while logging out.");
            }
        }
       
    }
}
