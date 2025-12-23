using Sahty.Web.Models.Api;

namespace Sahty.Web.Services.Api;

public sealed class NotificationsClient
{
    private readonly RestClient _rest;

    public NotificationsClient(RestClient rest)
    {
        _rest = rest;
    }

    public Task<ApiResponse<List<NotificationDto>>> GetAllAsync()
        => _rest.GetAsync<List<NotificationDto>>("api/notifications");

    public Task<ApiResponse<object>> MarkReadAsync(int id)
        => _rest.PutAsync<object, object>($"api/notifications/{id}/read", new { });

    public Task<ApiResponse<object>> MarkAllReadAsync()
        => _rest.PutAsync<object, object>("api/notifications/read-all", new { });
}
