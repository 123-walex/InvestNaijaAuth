using AutoMapper;
using InvestNaijaAuth.Data;
using InvestNaijaAuth.DTO_s;
using Microsoft.AspNetCore.Mvc;
using InvestNaijaAuth.Servicies;
using InvestNaijaAuth.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvestNaijaAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpPost("Signup")]
        public async Task<IActionResult> Signup(SignupDTO signup)
        {

            var requestId = HttpContext.TraceIdentifier;
            var userName = User.Identity?.Name ?? "Anonymous";
            _logger.LogInformation("Received SignUpUser request. TraceId: {requestId}, RequestedBy: {User}, Payload: {@SignUpDTO}", requestId, userName, signup);

            bool usernameExists = await _context.user.AnyAsync(u => u.Username == signup.Username);
            bool emailExists = await _context.user.AnyAsync(u => u.EmailAddress == signup.EmailAddress);
            bool passwordExists = await _context.user.AnyAsync(u => u.HashedPassword == signup.HashedPassword);

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

                _context.user.Add(SignUpEntity);
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
                var Loginentity = await _context.user
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
                _logger.LogError(ex, "Error occurred during login. TraceId: {requestId}, RequestedBy: {EmailAddress}", requestId, login.EmailAddress);
                return StatusCode(500, "An error occurred while processing your login request.");
            }
        }
        
    }


}
