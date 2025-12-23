using Sahty.Web.Models.Api;

namespace Sahty.Web.Services.Api;

public sealed class MedicationsClient
{
    private readonly RestClient _rest;

    public MedicationsClient(RestClient rest)
    {
        _rest = rest;
    }

    public Task<ApiResponse<List<MedicationDto>>> GetAllAsync()
        => _rest.GetAsync<List<MedicationDto>>("api/medications");

    public Task<ApiResponse<MedicationDto>> GetByIdAsync(int id)
        => _rest.GetAsync<MedicationDto>($"api/medications/{id}");

    public Task<ApiResponse<MedicationDto>> CreateAsync(MedicationCreateDto dto)
        => _rest.PostAsync<MedicationCreateDto, MedicationDto>("api/medications", dto);

    public Task<ApiResponse<MedicationDto>> UpdateAsync(int id, MedicationUpdateDto dto)
        => _rest.PutAsync<MedicationUpdateDto, MedicationDto>($"api/medications/{id}", dto);

    public Task<ApiResponse> DeleteAsync(int id)
        => _rest.DeleteAsync($"api/medications/{id}");
}
