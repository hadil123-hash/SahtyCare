using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sahty.Web.Models.Api;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Areas.Pharmacy.Pages;

[Authorize(Policy = "PharmacyOnly")]
public class PrescriptionsModel : PageModel
{
    private readonly PrescriptionsClient _prescriptionsClient;
    private readonly MedicationsClient _medicationsClient;

    public PrescriptionsModel(PrescriptionsClient prescriptionsClient, MedicationsClient medicationsClient)
    {
        _prescriptionsClient = prescriptionsClient;
        _medicationsClient = medicationsClient;
    }

    public List<PrescriptionDto> Items { get; private set; } = new();
    public List<SelectListItem> Medications { get; private set; } = new();
    public string? SelectedPatientLabel { get; private set; }
    public string? SelectedDoctorLabel { get; private set; }
    public bool ShowUpdateForm { get; private set; }

    [BindProperty]
    public UpdateInput Update { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? UpdateError { get; set; }
    public bool IsLoading { get; set; }

    public sealed class UpdateInput
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int PharmacistId { get; set; }

        [Required]
        public int MedicationId { get; set; }

        [Required]
        public DateTime DateIssued { get; set; } = DateTime.Today;

        [Required]
        public string Dosage { get; set; } = "";

        public string? Notes { get; set; }
        public string Status { get; set; } = "Accepted";
    }

    public async Task OnGetAsync()
    {
        await LoadMedicationsAsync();
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!ModelState.IsValid)
        {
            ShowUpdateForm = true;
            await LoadMedicationsAsync();
            await LoadAsync();
            return Page();
        }

        var res = await _prescriptionsClient.UpdateAsync(Update.Id, new PrescriptionUpdateDto
        {
            Id = Update.Id,
            DoctorId = Update.DoctorId,
            PatientId = Update.PatientId,
            PharmacistId = Update.PharmacistId,
            MedicationId = Update.MedicationId,
            DateIssued = Update.DateIssued,
            Dosage = Update.Dosage,
            Notes = Update.Notes,
            Status = Update.Status
        });

        if (!res.IsSuccess)
        {
            UpdateError = FormatError(res);
            ShowUpdateForm = true;
            await LoadMedicationsAsync();
            await LoadAsync();
            return Page();
        }

        TempData["Flash"] = "Ordonnance mise a jour.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id)
    {
        var res = await _prescriptionsClient.DeleteAsync(id);
        TempData["Flash"] = res.IsSuccess ? "Ordonnance supprimee." : FormatError(res);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync([FromForm] int id)
    {
        await LoadMedicationsAsync();
        await LoadAsync();

        var selected = Items.FirstOrDefault(p => p.Id == id);
        if (selected == null)
        {
            UpdateError = "Ordonnance introuvable.";
            ShowUpdateForm = true;
            return Page();
        }

        SelectedPatientLabel = !string.IsNullOrWhiteSpace(selected.PatientName)
            ? selected.PatientName
            : $"Patient #{selected.PatientId}";
        SelectedDoctorLabel = !string.IsNullOrWhiteSpace(selected.DoctorName)
            ? selected.DoctorName
            : $"Doctor #{selected.DoctorId}";

        Update = new UpdateInput
        {
            Id = selected.Id,
            DoctorId = selected.DoctorId,
            PatientId = selected.PatientId,
            PharmacistId = selected.PharmacistId,
            MedicationId = selected.MedicationId,
            DateIssued = selected.DateIssued,
            Dosage = selected.Dosage,
            Notes = selected.Notes,
            Status = string.IsNullOrWhiteSpace(selected.Status) ? "Accepted" : selected.Status
        };

        ShowUpdateForm = true;
        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync([FromForm] int id)
    {
        var res = await _prescriptionsClient.AcceptAsync(id);
        TempData["Flash"] = res.IsSuccess ? "Ordonnance acceptee." : FormatError(res);
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        var res = await _prescriptionsClient.GetAllAsync();
        IsLoading = false;

        if (!res.IsSuccess || res.Data is null)
        {
            ErrorMessage = FormatError(res);
            Items = new List<PrescriptionDto>();
            return;
        }

        Items = res.Data
            .OrderByDescending(p => p.DateIssued)
            .ToList();
    }

    private async Task LoadMedicationsAsync()
    {
        var res = await _medicationsClient.GetAllAsync();
        if (!res.IsSuccess || res.Data is null)
        {
            Medications = new List<SelectListItem>();
            return;
        }

        Medications = res.Data
            .OrderBy(m => m.Name)
            .Select(m => new SelectListItem(m.Name, m.Id.ToString()))
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
}
