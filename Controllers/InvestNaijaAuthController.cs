using AutoMapper;
using InvestNaijaAuth.Data;
using InvestNaijaAuth.DTO_s;
using Microsoft.AspNetCore.Mvc;
using InvestNaijaAuth.Servicies;
using InvestNaijaAuth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using User_Authapi.DTO_s;

namespace InvestNaijaAuth.Controllers
{
// configure login mail
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
        private readonly IPortfolioService _portfolioservice;
        public InvestNaijaAuthController(
                           IConfiguration configuration,
                           InvestNaijaDBContext context,
                           IMapper mapper,
                           ILogger<InvestNaijaAuthController> logger,
                           ITokenService tokenService,
                           IPortfolioService portfolioservice
                           )
        {
            _configuration = configuration;
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _tokenservice = tokenService;
            _portfolioservice = portfolioservice;
        }
        [AllowAnonymous]
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

                signup.Role ??= "User";

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
        [AllowAnonymous]
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

                        // Check if the user already has a wallet
                        var existingWallet = await _context.Wallet
                            .FirstOrDefaultAsync(w => w.UserId == Loginentity.Id && !w.IsDeleted);

                        if (existingWallet == null)
                        {
                            var newWallet = new Wallet
                            {
                                WalletId = Guid.NewGuid(),
                                UserId = Loginentity.Id,
                                AccountBalance = 0,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                IsDeleted = false
                            };

                            _context.Wallet.Add(newWallet);
                            _logger.LogInformation("Wallet created for user {UserId} during login. TraceId: {requestId}", Loginentity.Id, requestId);
                        }
                        else
                        {
                            _logger.LogInformation("Wallet already exists for user {UserId}. TraceId: {requestId}", Loginentity.Id, requestId);
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
        [Authorize(Roles = "Admin,User")]
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(UpdateUserDTO updateDto)
        {
            var requestId = HttpContext.TraceIdentifier;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var user = await _context.User.FindAsync(Guid.Parse(userId));
            if (user == null || user.IsDeleted)
                return NotFound("User not found");

            if (!string.IsNullOrWhiteSpace(updateDto.UserName))
                user.Username = updateDto.UserName;

            if (!string.IsNullOrWhiteSpace(updateDto.EmailAddress))
                user.EmailAddress = updateDto.EmailAddress;

            if (!string.IsNullOrWhiteSpace(updateDto.HashedPassword))
                user.HashedPassword = updateDto.HashedPassword;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User updated successfully. TraceId: {requestId}, UserId: {UserId}", requestId, user.Id);
            return Ok(new { message = "User updated successfully" });
        }
        [Authorize(Roles = "Admin,User")]
        [HttpPatch("PartialUpdateUser")]
        public async Task<IActionResult> PatchUser([FromBody] PartialUpdateUserDTO patchDto)
        {
            var requestId = HttpContext.TraceIdentifier;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var user = await _context.User.FindAsync(Guid.Parse(userId));
            if (user == null || user.IsDeleted)
            {
                _logger.LogWarning("Patch failed: User not found or deleted. TraceId: {requestId}", requestId);
                return NotFound("User not found");
            }

            bool changesMade = false;

            if (!string.IsNullOrWhiteSpace(patchDto.UserName) && patchDto.UserName != user.Username)
            {
                user.Username = patchDto.UserName;
                changesMade = true;
            }

            if (!string.IsNullOrWhiteSpace(patchDto.EmailAddress) && patchDto.EmailAddress != user.EmailAddress)
            {
                user.EmailAddress = patchDto.EmailAddress;
                changesMade = true;
            }

            if (!string.IsNullOrWhiteSpace(patchDto.HashedPassword) && patchDto.HashedPassword != user.HashedPassword)
            {
                user.HashedPassword = patchDto.HashedPassword;
                changesMade = true;
            }

            if (!changesMade)
            {
                return BadRequest("No valid updates provided.");
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User partially updated. TraceId: {requestId}, UserId: {UserId}", requestId, user.Id);
            return Ok(new { message = "User partially updated successfully" });
        }

        [Authorize(Roles = "Admin,User")]
        [HttpDelete("SoftDeleteUser")]
        public async Task<IActionResult> DeleteUser()
        {
            var isAdmin = User.IsInRole("Admin");

            var requestId = HttpContext.TraceIdentifier;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var user = await _context.User.FindAsync(Guid.Parse(userId));
            if (user == null || user.IsDeleted)
                return NotFound("User not found or already deleted");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User soft-deleted successfully. TraceId: {requestId}, UserId: {UserId}", requestId, user.Id);
            return Ok(new { message = "User deleted successfully" });
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("HardDeleteUser/{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var requestId = HttpContext.TraceIdentifier;
            var adminName = User.Identity?.Name ?? "Unknown";

            _logger.LogInformation("Received hard delete request. TraceId: {requestId}, Admin: {Admin}, TargetUserId: {UserId}", requestId, adminName, userId);

            var user = await _context.User.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User not found for hard delete. TraceId: {requestId}, Admin: {Admin}", requestId, adminName);
                return NotFound("User not found.");
            }

            _context.User.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User permanently deleted. TraceId: {requestId}, Admin: {Admin}, DeletedUserId: {UserId}", requestId, adminName, userId);

            return Ok(new { message = $"User with ID {userId} permanently deleted." });
        }

    }
}
