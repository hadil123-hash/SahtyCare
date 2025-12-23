using Sahty.Web.Models.Api;

namespace Sahty.Web.Services.Api;

public sealed class AdminClient
{
    private readonly RestClient _rest;

    public AdminClient(RestClient rest)
    {
        _rest = rest;
    }

    public Task<ApiResponse<List<AdminUserDto>>> ListUsersAsync()
        => _rest.GetAsync<List<AdminUserDto>>("api/admin/users");

    public Task<ApiResponse<AdminUserResponseDto>> CreateUserAsync(AdminCreateUserRequest request)
        => _rest.PostAsync<AdminCreateUserRequest, AdminUserResponseDto>("api/admin/users", request);

    public Task<ApiResponse<AdminUserResponseDto>> SetRoleAsync(AdminSetRoleRequest request)
        => _rest.PutAsync<AdminSetRoleRequest, AdminUserResponseDto>("api/admin/users/role", request);

    public Task<ApiResponse> DeleteUserAsync(string userId)
        => _rest.DeleteAsync($"api/admin/users/{userId}");
}
