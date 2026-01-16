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

                Console.WriteLine($"[BaseService] Making {apiRequest.ApiType} request to: {apiRequest.Url}");
                Console.WriteLine($"[BaseService] API Base URL: {SD.AccessControlAPIBase}");

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
                    var requestBody = await message.Content.ReadAsStringAsync();
                    Console.WriteLine($"[BaseService] Request body: {requestBody}");
                }

                var apiResponse = await client.SendAsync(message);
                var responseContent = await apiResponse.Content.ReadAsStringAsync();

                Console.WriteLine($"[BaseService] Response status: {apiResponse.StatusCode}");
                Console.WriteLine($"[BaseService] Response content: {responseContent}");

                if (!apiResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[BaseService] API Error: {apiResponse.StatusCode}");
                    return default;
                }

                return JsonSerializer.Deserialize<T>(responseContent, _options);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[BaseService] Connection Error: Cannot reach API at {SD.AccessControlAPIBase}");
                Console.WriteLine($"[BaseService] Error: {ex.Message}");
                Console.WriteLine($"[BaseService] Make sure the API is running on the correct port!");
                return default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BaseService] Unexpected Error: {ex.Message}");
                Console.WriteLine($"[BaseService] Stack Trace: {ex.StackTrace}");
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
