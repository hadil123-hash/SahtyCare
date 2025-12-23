using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sahty.API.Data;
using Sahty.API.DTOs;
using Sahty.Metiers;
using Sahty.Shared.Auth;
using Sahty.Shared.Data;
using Sahty.Shared.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Sahty.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _db;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration config,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _db = db;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null) return BadRequest("Email already used.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true,
            FullName = dto.FullName
        };

        var create = await _userManager.CreateAsync(user, dto.Password);
        if (!create.Succeeded) return BadRequest(create.Errors.Select(e => e.Description));

        var addRole = await _userManager.AddToRoleAsync(user, AppRoles.Patient);
        if (!addRole.Succeeded)
            return BadRequest(addRole.Errors.Select(e => e.Description));

        await EnsurePatientProfileAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var normalizedRoles = AppRoles.NormalizeAll(roles);

        return Ok(CreateTokenResponse(user, normalizedRoles.ToArray()));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return Unauthorized("Invalid credentials.");

        var res = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!res.Succeeded) return Unauthorized("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        var normalizedRoles = AppRoles.NormalizeAll(roles);
        return Ok(CreateTokenResponse(user, normalizedRoles.ToArray()));
    }

    private async Task EnsurePatientProfileAsync(ApplicationUser user)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
            return;

        var exists = await _db.Patients.AnyAsync(p => p.Email == user.Email);
        if (exists)
            return;

        var patient = new Patient
        {
            FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName,
            Email = user.Email,
            DateOfBirth = DateTime.UtcNow.Date,
            PhoneNumber = "N/A"
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();
    }

    private AuthResponseDto CreateTokenResponse(ApplicationUser user, string[] roles)
    {
        var now = DateTimeOffset.UtcNow;
        var expMinutes = int.Parse(_config["Jwt:ExpMinutes"] ?? "60");
        var expires = now.AddMinutes(expMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("fullName", user.FullName ?? "")
        };

        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SigningKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: creds
        );

        return new AuthResponseDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            expires,
            user.Id,
            user.Email ?? "",
            roles
        );
    }
}
