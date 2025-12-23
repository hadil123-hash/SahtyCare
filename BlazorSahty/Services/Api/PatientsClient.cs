using Sahty.Web.Models.Api;

namespace Sahty.Web.Services.Api;

public sealed class PatientsClient
{
    private readonly RestClient _rest;

    public PatientsClient(RestClient rest)
    {
        _rest = rest;
    }

    public Task<ApiResponse<List<PatientDto>>> GetAllAsync()
        => _rest.GetAsync<List<PatientDto>>("api/patients");

    public Task<ApiResponse<PatientDto>> GetByIdAsync(int id)
        => _rest.GetAsync<PatientDto>($"api/patients/{id}");

    public Task<ApiResponse<PatientDto>> CreateAsync(PatientCreateDto dto)
        => _rest.PostAsync<PatientCreateDto, PatientDto>("api/patients", dto);

    public Task<ApiResponse<PatientDto>> UpdateAsync(int id, PatientUpdateDto dto)
        => _rest.PutAsync<PatientUpdateDto, PatientDto>($"api/patients/{id}", dto);

    public Task<ApiResponse> DeleteAsync(int id)
        => _rest.DeleteAsync($"api/patients/{id}");
}
