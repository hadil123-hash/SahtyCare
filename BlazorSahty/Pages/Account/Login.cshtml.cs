using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sahty.Web.Models.Api;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly AuthClient _authClient;

    public LoginModel(AuthClient authClient)
    {
        _authClient = authClient;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public sealed class LoginInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();

        var res = await _authClient.LoginAsync(new LoginDto
        {
            Email = Input.Email,
            Password = Input.Password
        });

        if (!res.IsSuccess || res.Data is null)
        {
            ErrorMessage = string.IsNullOrWhiteSpace(res.Error) ? "Email ou mot de passe incorrect." : res.Error;
            return Page();
        }

        await SignInAsync(res.Data);
        return LocalRedirect(returnUrl ?? "/Dashboard");
    }

    private async Task SignInAsync(AuthResponseDto auth)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, auth.UserId),
            new Claim(ClaimTypes.Name, auth.Email),
            new Claim(ClaimTypes.Email, auth.Email),
            new Claim("access_token", auth.Token)
        };

        foreach (var role in auth.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties
        {
            IsPersistent = false,
            ExpiresUtc = auth.ExpiresAt
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
    }
}
