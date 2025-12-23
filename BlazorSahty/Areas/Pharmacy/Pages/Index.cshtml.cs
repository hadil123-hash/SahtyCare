using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sahty.Web.Models.Api;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Areas.Pharmacy.Pages;

[Authorize(Policy = "PharmacyOnly")]
public class IndexModel : PageModel
{
    private readonly PrescriptionsClient _prescriptionsClient;
    private readonly NotificationsClient _notificationsClient;

    public IndexModel(PrescriptionsClient prescriptionsClient, NotificationsClient notificationsClient)
    {
        _prescriptionsClient = prescriptionsClient;
        _notificationsClient = notificationsClient;
    }

    public int Issued { get; set; }
    public int Dispensed { get; set; }
    public int UnreadNotifications { get; set; }
    public List<NotificationDto> Notifications { get; private set; } = new();
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        var res = await _prescriptionsClient.GetAllAsync();
        if (!res.IsSuccess || res.Data is null)
        {
            ErrorMessage = FormatError(res);
            Issued = 0;
            Dispensed = 0;
            return;
        }

        Issued = res.Data.Count(p => !string.Equals(p.Status, "Accepted", StringComparison.OrdinalIgnoreCase));
        Dispensed = res.Data.Count(p => string.Equals(p.Status, "Accepted", StringComparison.OrdinalIgnoreCase));

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
