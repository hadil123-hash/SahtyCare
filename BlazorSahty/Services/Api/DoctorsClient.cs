using Sahty.Web.Models.Api;

namespace Sahty.Web.Services.Api;

public sealed class DoctorsClient
{
    private readonly RestClient _rest;

    public DoctorsClient(RestClient rest)
    {
        _rest = rest;
    }

    public Task<ApiResponse<List<DoctorDto>>> GetAllAsync()
        => _rest.GetAsync<List<DoctorDto>>("api/doctors");

    public Task<ApiResponse<DoctorDto>> GetByIdAsync(int id)
        => _rest.GetAsync<DoctorDto>($"api/doctors/{id}");

    public Task<ApiResponse<DoctorDto>> CreateAsync(DoctorCreateDto dto)
        => _rest.PostAsync<DoctorCreateDto, DoctorDto>("api/doctors", dto);

    public Task<ApiResponse<DoctorDto>> UpdateAsync(int id, DoctorUpdateDto dto)
        => _rest.PutAsync<DoctorUpdateDto, DoctorDto>($"api/doctors/{id}", dto);

    public Task<ApiResponse> DeleteAsync(int id)
        => _rest.DeleteAsync($"api/doctors/{id}");
}
