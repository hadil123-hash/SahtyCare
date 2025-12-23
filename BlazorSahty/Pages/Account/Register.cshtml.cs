using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sahty.Web.Models.Api;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly AuthClient _authClient;

    public RegisterModel(AuthClient authClient)
    {
        _authClient = authClient;
    }

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public sealed class RegisterInput
    {
        public string? FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(8)]
        public string Password { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var res = await _authClient.RegisterAsync(new RegisterDto
        {
            Email = Input.Email,
            Password = Input.Password,
            FullName = Input.FullName ?? ""
        });

        if (!res.IsSuccess || res.Data is null)
        {
            ErrorMessage = string.IsNullOrWhiteSpace(res.Error) ? "Erreur lors de l'inscription." : res.Error;
            return Page();
        }

        await SignInAsync(res.Data);
        TempData["Flash"] = "Bienvenue !";
        return RedirectToPage("/Dashboard");
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
