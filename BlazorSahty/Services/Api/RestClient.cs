using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Sahty.Web.Services.Api;

public interface IApiTokenStore
{
    string? GetToken();
}

public sealed class HttpContextTokenStore : IApiTokenStore
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTokenStore(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetToken()
    {
        return _httpContextAccessor.HttpContext?.User.FindFirst("access_token")?.Value;
    }
}

public sealed class ApiResponse
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public HttpStatusCode StatusCode { get; }

    private ApiResponse(bool isSuccess, string? error, HttpStatusCode statusCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static async Task<ApiResponse> FromHttpResponseAsync(HttpResponseMessage response)
    {
        var content = response.Content is null ? "" : await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
            return new ApiResponse(true, null, response.StatusCode);

        var error = string.IsNullOrWhiteSpace(content) ? response.ReasonPhrase : content;
        return new ApiResponse(false, error, response.StatusCode);
    }
}

public sealed class ApiResponse<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public HttpStatusCode StatusCode { get; }

    private ApiResponse(bool isSuccess, T? data, string? error, HttpStatusCode statusCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        StatusCode = statusCode;
    }

    public static async Task<ApiResponse<T>> FromHttpResponseAsync(HttpResponseMessage response, JsonSerializerOptions json)
    {
        var content = response.Content is null ? "" : await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(content))
                return new ApiResponse<T>(true, default, null, response.StatusCode);

            try
            {
                var data = JsonSerializer.Deserialize<T>(content, json);
                return new ApiResponse<T>(true, data, null, response.StatusCode);
            }
            catch
            {
                return new ApiResponse<T>(false, default, "Invalid JSON response.", response.StatusCode);
            }
        }

        var error = string.IsNullOrWhiteSpace(content) ? response.ReasonPhrase : content;
        return new ApiResponse<T>(false, default, error, response.StatusCode);
    }
}

public sealed class RestClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiTokenStore _tokenStore;
    private readonly JsonSerializerOptions _json;

    public RestClient(HttpClient httpClient, IApiTokenStore tokenStore)
    {
        _httpClient = httpClient;
        _tokenStore = tokenStore;
        _json = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public Task<ApiResponse<T>> GetAsync<T>(string path)
        => SendAsync<T>(HttpMethod.Get, path);

    public Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string path, TRequest body)
        => SendAsync<TResponse>(HttpMethod.Post, path, body);

    public Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string path, TRequest body)
        => SendAsync<TResponse>(HttpMethod.Put, path, body);

    public Task<ApiResponse> DeleteAsync(string path)
        => SendAsync(HttpMethod.Delete, path);

    private async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string path, object? body = null)
    {
        using var request = BuildRequest(method, path, body);
        using var response = await _httpClient.SendAsync(request);
        return await ApiResponse<T>.FromHttpResponseAsync(response, _json);
    }

    private async Task<ApiResponse> SendAsync(HttpMethod method, string path, object? body = null)
    {
        using var request = BuildRequest(method, path, body);
        using var response = await _httpClient.SendAsync(request);
        return await ApiResponse.FromHttpResponseAsync(response);
    }

    private HttpRequestMessage BuildRequest(HttpMethod method, string path, object? body)
    {
        var request = new HttpRequestMessage(method, path);
        var token = _tokenStore.GetToken();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body, _json);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }
}
