
// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cloud.Models;
using Cloud.Services;
using Microsoft.AspNetCore.Identity;

namespace Cloud.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;

        public AuthController(
            ApplicationDbContext context,
            IEmailService emailService,
            UserManager<UserModel> userManager,
            SignInManager<UserModel> signInManager)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Registers a new user with the specified role.
        /// </summary>
        /// <param name="model">The registration model containing user details.</param>
        /// <returns>An IActionResult indicating the result of the registration process.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate the role
            // Validate the role
            if (model.Role != UserRole.Tenant && model.Role != UserRole.Owner)
            {
                return BadRequest(new { message = "Invalid role. Must be either Tenant or Owner." });
            }

            var user = new UserModel
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = model.Role // Set the role from the model
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Create the role-specific model
                if (user.Role == UserRole.Tenant)
                {
                    var tenant = new TenantModel
                    {
                        UserId = user.Id
                    };
                    await _context.Tenants.AddAsync(tenant);
                }
                else if (user.Role == UserRole.Owner)
                {
                    var owner = new OwnerModel
                    {
                        UserId = user.Id
                    };
                    await _context.Owners.AddAsync(owner);
                }

                await _context.SaveChangesAsync();

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token = token }, Request.Scheme);

                _emailService.SendEmail(user.Email, "Confirm your email", $"Please confirm your account by clicking this link: {confirmationLink}");

                return Ok(new { message = $"User created successfully as {user.Role}. Please check your email to confirm your account." });
            }

            return BadRequest(new { message = "User registration failed.", errors = result.Errors });

        }
        /// <summary>
        /// Authenticates a user and returns a JWT token upon successful login.
        /// </summary>
        /// <param name="model">The login model containing user credentials.</param>
        /// <returns>An IActionResult with the JWT token or an error message.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid email or password.");
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        /// <returns>An IActionResult indicating successful logout.</returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully." });
        }

        /// <summary>
        /// Refreshes the JWT token for an authenticated user.
        /// </summary>
        /// <param name="model">The refresh token model containing the expired token.</param>
        /// <returns>An IActionResult with a new JWT token or an error message.</returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            var principal = GetPrincipalFromExpiredToken(model.Token);
            var email = principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return BadRequest("Invalid token");
            }

            var newToken = GenerateJwtToken(user);
            return Ok(new { Token = newToken });
        }

        /// <summary>
        /// Initiates the password reset process for a user.
        /// </summary>
        /// <param name="model">The forgot password model containing the user's email.</param>
        /// <returns>An IActionResult indicating that a reset link has been sent if the email exists.</returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Ok(new { message = "If your email is registered, you will receive a password reset link." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Auth", new { email = user.Email, token = token }, Request.Scheme);

            _emailService.SendEmail(user.Email, "Reset your password", $"Please reset your password by clicking this link: {resetLink}");

            return Ok(new { message = "If your email is registered, you will receive a password reset link." });
        }

        /// <summary>
        /// Resets the user's password using the provided token.
        /// </summary>
        /// <param name="model">The reset password model containing the new password and reset token.</param>
        /// <returns>An IActionResult indicating the success or failure of the password reset.</returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Invalid request");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { message = "Password has been reset successfully." });
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Confirms a user's email address using the provided token.
        /// </summary>
        /// <param name="userId">The ID of the user confirming their email.</param>
        /// <param name="token">The email confirmation token.</param>
        /// <returns>An IActionResult indicating the success or failure of the email confirmation.</returns>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Invalid email confirmation link");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Invalid email confirmation link");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok(new { message = "Email confirmed successfully." });
            }

            return BadRequest("Email confirmation failed.");
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user model for which to generate the token.</param>
        /// <returns>A string containing the JWT token.</returns>
        private string GenerateJwtToken(UserModel user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET is not set")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(Environment.GetEnvironmentVariable("JWT_EXPIRATION_DAYS") ?? "1"));

            var token = new JwtSecurityToken(
                Environment.GetEnvironmentVariable("JWT_ISSUER"),
                Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Extracts the ClaimsPrincipal from an expired JWT token.
        /// </summary>
        /// <param name="token">The expired JWT token.</param>
        /// <returns>A ClaimsPrincipal object extracted from the token.</returns>
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET is not set"))),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }

    /// <summary>
    /// Represents the model for user login.
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for the user account.
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the model for refreshing a JWT token.
    /// </summary>
    public class RefreshTokenModel
    {
        /// <summary>
        /// Gets or sets the expired JWT token.
        /// </summary>
        [Required]
        public string Token { get; set; } = string.Empty;
    }


    public class RegisterModel
    {
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password for the user account.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [Required]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [Required]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the role of the user. Must be either "Tenant" or "Owner".
        /// </summary>
        [Required]
        public UserRole Role { get; set; } = UserRole.Tenant;
    }

    /// <summary>
    /// Represents the model for initiating a password reset.
    /// </summary>
    public class ForgotPasswordModel
    {
        /// <summary>
        /// Gets or sets the email address of the user requesting a password reset.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the model for resetting a user's password.
    /// </summary>
    public class ResetPasswordModel
    {
        /// <summary>
        /// Gets or sets the email address of the user resetting their password.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the password reset token.
        /// </summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the new password for the user account.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}

