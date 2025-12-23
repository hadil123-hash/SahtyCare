using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sahty.Web.Models.Api;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Areas.Doctor.Pages;

[Authorize(Policy = "DoctorOnly")]
public class IndexModel : PageModel
{
    private readonly AppointmentsClient _appointmentsClient;
    private readonly PrescriptionsClient _prescriptionsClient;
    private readonly NotificationsClient _notificationsClient;

    public IndexModel(
        AppointmentsClient appointmentsClient,
        PrescriptionsClient prescriptionsClient,
        NotificationsClient notificationsClient)
    {
        _appointmentsClient = appointmentsClient;
        _prescriptionsClient = prescriptionsClient;
        _notificationsClient = notificationsClient;
    }

    public int MyAppointments { get; set; }
    public int MyPrescriptions { get; set; }
    public int UnreadNotifications { get; set; }
    public List<NotificationDto> Notifications { get; private set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var apptRes = await _appointmentsClient.GetAllAsync();
        if (apptRes.IsSuccess && apptRes.Data is not null)
            MyAppointments = apptRes.Data.Count;
        else
            ErrorMessage = FormatError(apptRes);

        var presRes = await _prescriptionsClient.GetAllAsync();
        if (presRes.IsSuccess && presRes.Data is not null)
            MyPrescriptions = presRes.Data.Count;
        else if (string.IsNullOrWhiteSpace(ErrorMessage))
            ErrorMessage = FormatError(presRes);

        var notifRes = await _notificationsClient.GetAllAsync();
        if (notifRes.IsSuccess && notifRes.Data is not null)
        {
            UnreadNotifications = notifRes.Data.Count(n => !n.IsRead);
            Notifications = notifRes.Data.Take(5).ToList();
        }
        else if (string.IsNullOrWhiteSpace(ErrorMessage))
        {
            ErrorMessage = FormatError(notifRes);
        }
    }

    private static string FormatError<T>(ApiResponse<T> response)
    {
        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            return "Accès refusé par l'API.";
        return string.IsNullOrWhiteSpace(response.Error) ? "Erreur API." : response.Error;
    }
}
