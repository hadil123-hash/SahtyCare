using Sahty.Web.Models.Api;

namespace Sahty.Web.Services.Api;

public sealed class PharmacistsClient
{
    private readonly RestClient _rest;

    public PharmacistsClient(RestClient rest)
    {
        _rest = rest;
    }

    public Task<ApiResponse<List<PharmacistDto>>> GetAllAsync()
        => _rest.GetAsync<List<PharmacistDto>>("api/pharmacists");

    public Task<ApiResponse<PharmacistDto>> GetByIdAsync(int id)
        => _rest.GetAsync<PharmacistDto>($"api/pharmacists/{id}");

    public Task<ApiResponse<PharmacistDto>> CreateAsync(PharmacistCreateDto dto)
        => _rest.PostAsync<PharmacistCreateDto, PharmacistDto>("api/pharmacists", dto);

    public Task<ApiResponse<PharmacistDto>> UpdateAsync(int id, PharmacistUpdateDto dto)
        => _rest.PutAsync<PharmacistUpdateDto, PharmacistDto>($"api/pharmacists/{id}", dto);

    public Task<ApiResponse> DeleteAsync(int id)
        => _rest.DeleteAsync($"api/pharmacists/{id}");
}
