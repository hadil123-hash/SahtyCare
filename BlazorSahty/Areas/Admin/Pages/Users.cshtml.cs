using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sahty.Web.Models;
using Sahty.Web.Models.Api;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Areas.Admin.Pages;

[Authorize(Policy = "AdminOnly")]
public class UsersModel : PageModel
{
    private readonly AdminClient _adminClient;

    public UsersModel(AdminClient adminClient)
    {
        _adminClient = adminClient;
    }

    public sealed record UserRow(string Id, string Email, string? FullName, string Role);

    public List<UserRow> Users { get; private set; } = new();
    public bool ShowCreateForm { get; private set; }

    [BindProperty]
    public CreateUserInput Create { get; set; } = new();

    public string? CreateError { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; }

    public sealed class CreateUserInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(8)]
        public string Password { get; set; } = "";

        public string? FullName { get; set; }

        [Required]
        public string Role { get; set; } = AppRoles.Patient;
    }

    public async Task OnGetAsync(string? form = null)
    {
        ShowCreateForm = string.Equals(form, "create", StringComparison.OrdinalIgnoreCase);
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostCreateUserAsync()
    {
        if (!await BindAndValidateCreateAsync())
        {
            CreateError = "Veuillez corriger les champs du formulaire.";
            ShowCreateForm = true;
            await LoadAsync();
            return Page();
        }

        if (!AppRoles.All.Contains(Create.Role))
        {
            CreateError = "Role invalide.";
            ShowCreateForm = true;
            await LoadAsync();
            return Page();
        }

        var res = await _adminClient.CreateUserAsync(new AdminCreateUserRequest
        {
            Email = Create.Email,
            Password = Create.Password,
            Role = Create.Role,
            FullName = Create.FullName
        });

        if (!res.IsSuccess)
        {
            CreateError = FormatError(res);
            ShowCreateForm = true;
            await LoadAsync();
            return Page();
        }

        TempData["Flash"] = $"Utilisateur cree: {Create.Email} ({Create.Role})";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetRoleAsync([FromForm] string userId, [FromForm] string role)
    {
        if (!AppRoles.All.Contains(role)) return BadRequest("Role invalide.");

        var res = await _adminClient.SetRoleAsync(new AdminSetRoleRequest
        {
            UserId = userId,
            Role = role
        });

        if (!res.IsSuccess)
        {
            TempData["Flash"] = FormatError(res);
            return RedirectToPage();
        }

        TempData["Flash"] = $"Role mis a jour: {userId} => {role}";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteUserAsync([FromForm] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return BadRequest("UserId invalide.");

        var res = await _adminClient.DeleteUserAsync(userId);
        TempData["Flash"] = res.IsSuccess ? "Utilisateur supprime." : FormatError(res);
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        var res = await _adminClient.ListUsersAsync();
        IsLoading = false;

        if (!res.IsSuccess || res.Data is null)
        {
            ErrorMessage = FormatError(res);
            Users = new List<UserRow>();
            return;
        }

        Users = res.Data
            .OrderBy(u => u.Email)
            .Select(u => new UserRow(
                u.Id,
                u.Email,
                u.FullName,
                u.Roles.FirstOrDefault() ?? "-"))
            .ToList();
    }

    private static string FormatError<T>(ApiResponse<T> response)
    {
        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            return "Acces refuse par l'API.";
        return string.IsNullOrWhiteSpace(response.Error) ? "Erreur API." : response.Error;
    }

    private static string FormatError(ApiResponse response)
    {
        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            return "Acces refuse par l'API.";
        return string.IsNullOrWhiteSpace(response.Error) ? "Erreur API." : response.Error;
    }

    private async Task<bool> BindAndValidateCreateAsync()
    {
        ModelState.Clear();
        await TryUpdateModelAsync(Create, nameof(Create));
        if (string.IsNullOrWhiteSpace(Create.Email) && Request.HasFormContentType)
            await TryUpdateModelAsync(Create, string.Empty);
        TryValidateModel(Create, nameof(Create));
        return ModelState.IsValid;
    }
}
