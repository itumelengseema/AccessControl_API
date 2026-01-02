using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;
using System.Text.Json;

namespace AccessControl_Web.Services
{
    public class BaseService : IBaseServices
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        public ApiResponse<object> ResponseModel { get; set; }

        public BaseService(IHttpClientFactory httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            this.ResponseModel = new();
        }

        public async Task<T?> SendAsync<T>(ApiRequest apiRequest)
        {
            try
            {
                var client = _httpClient.CreateClient("AccessControlAPI");

                if (string.IsNullOrEmpty(apiRequest.Url))
                {
                    Console.WriteLine("Error: API request URL is null or empty");
                    return default;
                }

                var message = new HttpRequestMessage
                {
                    RequestUri = new Uri(apiRequest.Url, UriKind.RelativeOrAbsolute),
                    Method = GetHttpMethod(apiRequest.ApiType)
                };

                // Add JWT token from session if available
                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                {
                    message.Headers.Add("Authorization", $"Bearer {token}");
                }

                if (apiRequest.Data != null)
                {
                    message.Content = JsonContent.Create(apiRequest.Data, options: _options);
                }

                var apiResponse = await client.SendAsync(message);

                if (!apiResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API Error: {apiResponse.StatusCode} - {await apiResponse.Content.ReadAsStringAsync()}");
                    return default;
                }

                return await apiResponse.Content.ReadFromJsonAsync<T>(_options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return default;
            }
        }

        private static HttpMethod GetHttpMethod(SD.ApiType apiType)
        {
            return apiType switch
            {
                SD.ApiType.POST => HttpMethod.Post,
                SD.ApiType.PUT => HttpMethod.Put,
                SD.ApiType.DELETE => HttpMethod.Delete,
                _ => HttpMethod.Get,
            };
        }
    }
}
