using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sahty.Web.Models.Api;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Areas.Admin.Pages;

[Authorize(Policy = "AdminOnly")]
public class PharmacistsModel : PageModel
{
    private readonly PharmacistsClient _pharmacistsClient;

    public PharmacistsModel(PharmacistsClient pharmacistsClient)
    {
        _pharmacistsClient = pharmacistsClient;
    }

    public List<PharmacistDto> Items { get; private set; } = new();
    public bool ShowCreateForm { get; private set; }
    public bool ShowUpdateForm { get; private set; }

    [BindProperty]
    public CreateInput Create { get; set; } = new();

    [BindProperty]
    public UpdateInput Update { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? CreateError { get; set; }
    public string? UpdateError { get; set; }
    public bool IsLoading { get; set; }

    public sealed class CreateInput
    {
        [Required]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string PharmacyName { get; set; } = "";

        [Required, MinLength(8)]
        public string Password { get; set; } = "";
    }

    public sealed class UpdateInput
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string PharmacyName { get; set; } = "";
    }

    public async Task OnGetAsync(string? form = null, int? id = null)
    {
        await LoadAsync();

        if (string.Equals(form, "create", StringComparison.OrdinalIgnoreCase))
        {
            ShowCreateForm = true;
            return;
        }

        if (string.Equals(form, "edit", StringComparison.OrdinalIgnoreCase) && id.HasValue)
        {
            var selected = Items.FirstOrDefault(p => p.Id == id.Value);
            if (selected is null) return;

            Update = new UpdateInput
            {
                Id = selected.Id,
                FullName = selected.FullName,
                Email = selected.Email,
                PharmacyName = selected.PharmacyName
            };

            ShowUpdateForm = true;
        }
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!await BindAndValidateCreateAsync())
        {
            CreateError = "Veuillez corriger les champs du formulaire.";
            ShowCreateForm = true;
            await LoadAsync();
            return Page();
        }

        var res = await _pharmacistsClient.CreateAsync(new PharmacistCreateDto
        {
            FullName = Create.FullName,
            Email = Create.Email,
            PharmacyName = Create.PharmacyName,
            Password = Create.Password
        });

        if (!res.IsSuccess)
        {
            CreateError = FormatError(res);
            ShowCreateForm = true;
            await LoadAsync();
            return Page();
        }

        TempData["Flash"] = "Pharmacien cree.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!await BindAndValidateUpdateAsync())
        {
            UpdateError = "Veuillez corriger les champs du formulaire.";
            ShowUpdateForm = true;
            await LoadAsync();
            return Page();
        }

        var res = await _pharmacistsClient.UpdateAsync(Update.Id, new PharmacistUpdateDto
        {
            Id = Update.Id,
            FullName = Update.FullName,
            Email = Update.Email,
            PharmacyName = Update.PharmacyName
        });

        if (!res.IsSuccess)
        {
            UpdateError = FormatError(res);
            ShowUpdateForm = true;
            await LoadAsync();
            return Page();
        }

        TempData["Flash"] = "Pharmacien mis a jour.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id)
    {
        var res = await _pharmacistsClient.DeleteAsync(id);
        TempData["Flash"] = res.IsSuccess ? "Pharmacien supprime." : FormatError(res);
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        var res = await _pharmacistsClient.GetAllAsync();
        IsLoading = false;

        if (!res.IsSuccess || res.Data is null)
        {
            ErrorMessage = FormatError(res);
            Items = new List<PharmacistDto>();
            return;
        }

        Items = res.Data.OrderBy(p => p.FullName).ToList();
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

    private async Task<bool> BindAndValidateUpdateAsync()
    {
        ModelState.Clear();
        await TryUpdateModelAsync(Update, nameof(Update));
        if (Update.Id == 0 && Request.HasFormContentType)
            await TryUpdateModelAsync(Update, string.Empty);
        TryValidateModel(Update, nameof(Update));
        return ModelState.IsValid;
    }
}

