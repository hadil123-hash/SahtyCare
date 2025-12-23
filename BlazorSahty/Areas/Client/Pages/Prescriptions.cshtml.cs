using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sahty.Web.Models.Api;
using Sahty.Web.Services.Api;

namespace Sahty.Web.Areas.Client.Pages;

[Authorize(Policy = "ClientOnly")]
public class PrescriptionsModel : PageModel
{
    private readonly PrescriptionsClient _prescriptionsClient;

    public PrescriptionsModel(PrescriptionsClient prescriptionsClient)
    {
        _prescriptionsClient = prescriptionsClient;
    }

    public List<PrescriptionDto> Items { get; private set; } = new();
    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; }

    public async Task OnGetAsync()
    {
        IsLoading = true;
        var res = await _prescriptionsClient.GetMyAsync();
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

    private static string FormatError<T>(ApiResponse<T> response)
    {
        if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            return "Accès refusé par l'API.";
        return string.IsNullOrWhiteSpace(response.Error) ? "Erreur API." : response.Error;
    }
}
