using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sahty.Web.Models;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Areas.Admin.Pages;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly AdminClient _adminClient;
    private readonly AppointmentsClient _appointmentsClient;
    private readonly PrescriptionsClient _prescriptionsClient;

    public IndexModel(
        AdminClient adminClient,
        AppointmentsClient appointmentsClient,
        PrescriptionsClient prescriptionsClient)
    {
        _adminClient = adminClient;
        _appointmentsClient = appointmentsClient;
        _prescriptionsClient = prescriptionsClient;
    }

    public int UsersCount { get; set; }
    public int AppointmentsCount { get; set; }
    public int PrescriptionsCount { get; set; }
    public List<string> Errors { get; } = new();

    public async Task OnGetAsync()
    {
        var usersRes = await _adminClient.ListUsersAsync();
        if (usersRes.IsSuccess && usersRes.Data is not null)
            UsersCount = usersRes.Data.Count;
        else
            AddError("Utilisateurs", usersRes);

        var apptRes = await _appointmentsClient.GetAllAsync();
        if (apptRes.IsSuccess && apptRes.Data is not null)
            AppointmentsCount = apptRes.Data.Count;
        else
            AddError("Rendez-vous", apptRes);

        var presRes = await _prescriptionsClient.GetAllAsync();
        if (presRes.IsSuccess && presRes.Data is not null)
            PrescriptionsCount = presRes.Data.Count;
        else
            AddError("Ordonnances", presRes);
    }

    private void AddError<T>(string label, ApiResponse<T> response)
    {
        if (response.IsSuccess) return;
        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
        {
            Errors.Add($"{label}: accès refusé par l'API (rôle non autorisé).");
            return;
        }

        var message = string.IsNullOrWhiteSpace(response.Error) ? "Erreur API." : response.Error;
        Errors.Add($"{label}: {message}");
    }
}
