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
public class PatientsModel : PageModel
{
    private readonly PatientsClient _patientsClient;

    public PatientsModel(PatientsClient patientsClient)
    {
        _patientsClient = patientsClient;
    }

    public List<PatientDto> Items { get; private set; } = new();
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
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

        [Required]
        public string PhoneNumber { get; set; } = "";

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
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-18);

        [Required]
        public string PhoneNumber { get; set; } = "";
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
                DateOfBirth = selected.DateOfBirth,
                PhoneNumber = selected.PhoneNumber
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

        var res = await _patientsClient.CreateAsync(new PatientCreateDto
        {
            FullName = Create.FullName,
            Email = Create.Email,
            DateOfBirth = Create.DateOfBirth,
            PhoneNumber = Create.PhoneNumber,
            Password = Create.Password
        });

        if (!res.IsSuccess)
        {
            CreateError = FormatError(res);
            ShowCreateForm = true;
            await LoadAsync();
            return Page();
        }

        TempData["Flash"] = "Patient cree.";
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

        var res = await _patientsClient.UpdateAsync(Update.Id, new PatientUpdateDto
        {
            Id = Update.Id,
            FullName = Update.FullName,
            Email = Update.Email,
            DateOfBirth = Update.DateOfBirth,
            PhoneNumber = Update.PhoneNumber
        });

        if (!res.IsSuccess)
        {
            UpdateError = FormatError(res);
            ShowUpdateForm = true;
            await LoadAsync();
            return Page();
        }

        TempData["Flash"] = "Patient mis a jour.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id)
    {
        var res = await _patientsClient.DeleteAsync(id);
        TempData["Flash"] = res.IsSuccess ? "Patient supprime." : FormatError(res);
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        var res = await _patientsClient.GetAllAsync();
        IsLoading = false;

        if (!res.IsSuccess || res.Data is null)
        {
            ErrorMessage = FormatError(res);
            Items = new List<PatientDto>();
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

