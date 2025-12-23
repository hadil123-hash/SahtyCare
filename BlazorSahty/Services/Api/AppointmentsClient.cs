using Sahty.Web.Models.Api;

namespace Sahty.Web.Services.Api;

public sealed class AppointmentsClient
{
    private readonly RestClient _rest;

    public AppointmentsClient(RestClient rest)
    {
        _rest = rest;
    }

    public Task<ApiResponse<List<AppointmentDto>>> GetAllAsync()
        => _rest.GetAsync<List<AppointmentDto>>("api/appointments");

    public Task<ApiResponse<List<AppointmentDto>>> GetMyAsync()
        => _rest.GetAsync<List<AppointmentDto>>("api/appointments/my");

    public Task<ApiResponse<AppointmentDto>> CreateAsync(AppointmentCreateDto dto)
        => _rest.PostAsync<AppointmentCreateDto, AppointmentDto>("api/appointments", dto);

    public Task<ApiResponse<AppointmentDto>> UpdateAsync(int id, AppointmentUpdateRequestDto dto)
        => _rest.PutAsync<AppointmentUpdateRequestDto, AppointmentDto>($"api/appointments/{id}", dto);

    public Task<ApiResponse> DeleteAsync(int id)
        => _rest.DeleteAsync($"api/appointments/{id}");

    public Task<ApiResponse<AppointmentDto>> AcceptAsync(int id)
        => _rest.PutAsync<object, AppointmentDto>($"api/appointments/accept/{id}", new { });

    public Task<ApiResponse<AppointmentDto>> RefuseAsync(int id)
        => _rest.PutAsync<object, AppointmentDto>($"api/appointments/refuse/{id}", new { });
}
