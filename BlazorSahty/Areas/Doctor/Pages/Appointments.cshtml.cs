using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sahty.Web.Models.Api;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Areas.Doctor.Pages;

[Authorize(Policy = "DoctorOnly")]
public class AppointmentsModel : PageModel
{
    private readonly AppointmentsClient _appointmentsClient;

    public AppointmentsModel(AppointmentsClient appointmentsClient)
    {
        _appointmentsClient = appointmentsClient;
    }

    public List<AppointmentDto> Items { get; private set; } = new();
    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAcceptAsync([FromForm] int id)
    {
        var res = await _appointmentsClient.AcceptAsync(id);
        TempData["Flash"] = res.IsSuccess ? "RDV accepté." : FormatError(res);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRefuseAsync([FromForm] int id)
    {
        var res = await _appointmentsClient.RefuseAsync(id);
        TempData["Flash"] = res.IsSuccess ? "RDV refusé." : FormatError(res);
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        var res = await _appointmentsClient.GetAllAsync();
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

    private static string FormatError<T>(ApiResponse<T> response)
    {
        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            return "Accès refusé par l'API.";
        return string.IsNullOrWhiteSpace(response.Error) ? "Erreur API." : response.Error;
    }
}
