using Sahty.Web.Models.Api;

namespace Sahty.Web.Services.Api;

public sealed class PrescriptionsClient
{
    private readonly RestClient _rest;

    public PrescriptionsClient(RestClient rest)
    {
        _rest = rest;
    }

    public Task<ApiResponse<List<PrescriptionDto>>> GetAllAsync()
        => _rest.GetAsync<List<PrescriptionDto>>("api/prescriptions");

    public Task<ApiResponse<List<PrescriptionDto>>> GetMyAsync()
        => _rest.GetAsync<List<PrescriptionDto>>("api/prescriptions/my");

    public Task<ApiResponse<PrescriptionDto>> CreateAsync(PrescriptionCreateDto dto)
        => _rest.PostAsync<PrescriptionCreateDto, PrescriptionDto>("api/prescriptions", dto);

    public Task<ApiResponse<PrescriptionDto>> UpdateAsync(int id, PrescriptionUpdateDto dto)
        => _rest.PutAsync<PrescriptionUpdateDto, PrescriptionDto>($"api/prescriptions/{id}", dto);

    public Task<ApiResponse<PrescriptionDto>> AcceptAsync(int id)
        => _rest.PutAsync<object, PrescriptionDto>($"api/prescriptions/accept/{id}", new { });

    public Task<ApiResponse> DeleteAsync(int id)
        => _rest.DeleteAsync($"api/prescriptions/{id}");
}
