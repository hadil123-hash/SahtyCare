using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sahty.Web.Models.Api;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Areas.Doctor.Pages;

[Authorize(Policy = "DoctorOnly")]
public class PrescriptionsModel : PageModel
{
    private readonly PrescriptionsClient _prescriptionsClient;
    private readonly PatientsClient _patientsClient;
    private readonly PharmacistsClient _pharmacistsClient;
    private readonly MedicationsClient _medicationsClient;

    public PrescriptionsModel(
        PrescriptionsClient prescriptionsClient,
        PatientsClient patientsClient,
        PharmacistsClient pharmacistsClient,
        MedicationsClient medicationsClient)
    {
        _prescriptionsClient = prescriptionsClient;
        _patientsClient = patientsClient;
        _pharmacistsClient = pharmacistsClient;
        _medicationsClient = medicationsClient;
    }

    public List<PrescriptionDto> Items { get; private set; } = new();
    public List<SelectListItem> Patients { get; private set; } = new();
    public List<SelectListItem> Pharmacists { get; private set; } = new();
    public List<SelectListItem> Medications { get; private set; } = new();
    public bool ShowCreateForm { get; private set; }

    [BindProperty]
    public CreateInput Create { get; set; } = new();

    public string? CreateError { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; }

    public sealed class CreateInput
    {
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
    }

    public async Task OnGetAsync(string? form = null)
    {
        Create.DoctorId = 0;
        ShowCreateForm = string.Equals(form, "create", StringComparison.OrdinalIgnoreCase);
        await LoadLookupsAsync();
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            ShowCreateForm = true;
            await LoadLookupsAsync();
            await LoadAsync();
            return Page();
        }

        var res = await _prescriptionsClient.CreateAsync(new PrescriptionCreateDto
        {
            DoctorId = Create.DoctorId,
            PatientId = Create.PatientId,
            PharmacistId = Create.PharmacistId,
            MedicationId = Create.MedicationId,
            DateIssued = Create.DateIssued,
            Dosage = Create.Dosage,
            Notes = Create.Notes
        });

        if (!res.IsSuccess)
        {
            CreateError = FormatError(res);
            ShowCreateForm = true;
            await LoadLookupsAsync();
            await LoadAsync();
            return Page();
        }

        TempData["Flash"] = "Ordonnance creee.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromForm] int id)
    {
        var res = await _prescriptionsClient.DeleteAsync(id);
        TempData["Flash"] = res.IsSuccess ? "Ordonnance supprimee." : FormatError(res);
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

    private async Task LoadLookupsAsync()
    {
        var patientsRes = await _patientsClient.GetAllAsync();
        if (patientsRes.IsSuccess && patientsRes.Data is not null)
        {
            Patients = patientsRes.Data
                .OrderBy(p => p.FullName)
                .Select(p => new SelectListItem($"{p.FullName} - {p.Email}", p.Id.ToString()))
                .ToList();
        }
        else
        {
            Patients = new List<SelectListItem>();
        }

        var pharmacistsRes = await _pharmacistsClient.GetAllAsync();
        if (pharmacistsRes.IsSuccess && pharmacistsRes.Data is not null)
        {
            Pharmacists = pharmacistsRes.Data
                .OrderBy(p => p.FullName)
                .Select(p => new SelectListItem($"{p.FullName} - {p.PharmacyName}", p.Id.ToString()))
                .ToList();
        }
        else
        {
            Pharmacists = new List<SelectListItem>();
        }

        var medicationsRes = await _medicationsClient.GetAllAsync();
        if (medicationsRes.IsSuccess && medicationsRes.Data is not null)
        {
            Medications = medicationsRes.Data
                .OrderBy(m => m.Name)
                .Select(m => new SelectListItem(m.Name, m.Id.ToString()))
                .ToList();
        }
        else
        {
            Medications = new List<SelectListItem>();
        }
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
