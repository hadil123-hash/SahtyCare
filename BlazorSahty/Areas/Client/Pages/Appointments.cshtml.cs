using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sahty.Web.Models.Api;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Areas.Client.Pages;

[Authorize(Policy = "ClientOnly")]
public class AppointmentsModel : PageModel
{
    private readonly AppointmentsClient _appointmentsClient;
    private readonly DoctorsClient _doctorsClient;

    public AppointmentsModel(AppointmentsClient appointmentsClient, DoctorsClient doctorsClient)
    {
        _appointmentsClient = appointmentsClient;
        _doctorsClient = doctorsClient;
    }

    public List<AppointmentDto> Items { get; private set; } = new();
    public List<SelectListItem> Doctors { get; private set; } = new();
    public bool ShowCreateForm { get; private set; }
    public bool ShowUpdateForm { get; private set; }

    [BindProperty]
    public CreateInput Create { get; set; } = new();

    [BindProperty]
    public UpdateInput Update { get; set; } = new();

    public string? CreateError { get; set; }
    public string? UpdateError { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; }

    public sealed class CreateInput
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(1);

        public string? Notes { get; set; }
    }

    public sealed class UpdateInput
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(1);
    }

    public async Task OnGetAsync(string? form = null, int? id = null)
    {
        await LoadDoctorsAsync();
        await LoadAsync();

        if (string.Equals(form, "create", StringComparison.OrdinalIgnoreCase))
        {
            ShowCreateForm = true;
            return;
        }

        if (string.Equals(form, "edit", StringComparison.OrdinalIgnoreCase) && id.HasValue)
        {
            var selected = Items.FirstOrDefault(a => a.Id == id.Value);
            if (selected is null) return;
            if (!string.Equals(selected.Status, "Requested", StringComparison.OrdinalIgnoreCase))
                return;

            Update = new UpdateInput
            {
                Id = selected.Id,
                DoctorId = selected.DoctorId,
                Date = selected.Date
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
            await LoadDoctorsAsync();
            await LoadAsync();
            return Page();
        }

        var dto = new AppointmentCreateDto
        {
            DoctorId = Create.DoctorId,
            PatientId = 0,
            Date = Create.Date,
            Status = "Requested"
        };

        var res = await _appointmentsClient.CreateAsync(dto);
        if (!res.IsSuccess)
        {
            CreateError = FormatError(res);
            ShowCreateForm = true;
            await LoadDoctorsAsync();
            await LoadAsync();
            return Page();
        }

        TempData["Flash"] = "RDV envoye.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!await BindAndValidateUpdateAsync())
        {
            UpdateError = "Veuillez corriger les champs du formulaire.";
            ShowUpdateForm = true;
            await LoadDoctorsAsync();
            await LoadAsync();
            return Page();
        }

        var res = await _appointmentsClient.UpdateAsync(Update.Id, new AppointmentUpdateRequestDto
        {
            DoctorId = Update.DoctorId,
            Date = Update.Date
        });

        if (!res.IsSuccess)
        {
            UpdateError = FormatError(res);
            ShowUpdateForm = true;
            await LoadDoctorsAsync();
            await LoadAsync();
            return Page();
        }

        TempData["Flash"] = "RDV mis a jour.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id)
    {
        var res = await _appointmentsClient.DeleteAsync(id);
        TempData["Flash"] = res.IsSuccess ? "RDV supprime." : FormatError(res);
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        var res = await _appointmentsClient.GetMyAsync();
        IsLoading = false;

        if (!res.IsSuccess || res.Data is null)
        {
            ErrorMessage = FormatError(res);
            Items = new List<AppointmentDto>();
            return;
        }

        Items = res.Data
            .OrderByDescending(a => a.Date)
            .ToList();
    }

    private async Task LoadDoctorsAsync()
    {
        var res = await _doctorsClient.GetAllAsync();
        if (!res.IsSuccess || res.Data is null)
        {
            Doctors = new List<SelectListItem>();
            return;
        }

        Doctors = res.Data
            .OrderBy(d => d.FullName)
            .Select(d => new SelectListItem($"{d.FullName} - {d.Speciality}", d.Id.ToString()))
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
        if (Create.DoctorId == 0 && Request.HasFormContentType)
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
