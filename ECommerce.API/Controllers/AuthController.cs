using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.API.Contracts;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IEmailNotificationService emailNotificationService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailNotificationService = emailNotificationService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegistrationResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Conflict("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.Phone,
            Role = request.Role,
            EmailConfirmed = false,
            Profile = new UserProfile { FullName = request.FullName ?? request.Email }
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(createResult.Errors);
        }

        await EnsureRoleExistsAsync(request.Role);
        var roleResult = await _userManager.AddToRoleAsync(user, request.Role.ToString());
        if (!roleResult.Succeeded)
        {
            return BadRequest(roleResult.Errors);
        }

        await SendConfirmationEmailAsync(user, cancellationToken);

        return CreatedAtAction(nameof(Register), new RegistrationResponse(user.Id, user.Email!, user.Role, EmailConfirmationRequired: true));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || user.IsSuspended || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized("Invalid email or password.");
        }

        if (!user.EmailConfirmed)
        {
            return Unauthorized("Email address is not confirmed.");
        }

        return new AuthResponse(user.Id, user.Email!, user.Role, await CreateToken(user));
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] int userId, [FromQuery] string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return NotFound();
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded ? Ok("Email confirmed successfully.") : BadRequest(result.Errors);
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(ResendEmailConfirmationRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return NoContent();
        }

        if (user.EmailConfirmed)
        {
            return BadRequest("Email address is already confirmed.");
        }

        await SendConfirmationEmailAsync(user, cancellationToken);
        return NoContent();
    }

    private async Task<string> CreateToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["DurationInMinutes"] ?? "60"));
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials));
    }

    private async Task EnsureRoleExistsAsync(UserRole role)
    {
        var roleName = role.ToString();
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
    }

    private async Task SendConfirmationEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var apiBaseUrl = _configuration["App:ApiBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var confirmationUrl = $"{apiBaseUrl}/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
        await _emailNotificationService.SendEmailConfirmationAsync(user.Email!, confirmationUrl, cancellationToken);
    }
}
