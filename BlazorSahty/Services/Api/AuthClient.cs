using Sahty.Web.Models.Api;

namespace Sahty.Web.Services.Api;

public sealed class AuthClient
{
    private readonly RestClient _rest;

    public AuthClient(RestClient rest)
    {
        _rest = rest;
    }

    public Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
        => _rest.PostAsync<LoginDto, AuthResponseDto>("api/auth/login", dto);

    public Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
        => _rest.PostAsync<RegisterDto, AuthResponseDto>("api/auth/register", dto);
}
