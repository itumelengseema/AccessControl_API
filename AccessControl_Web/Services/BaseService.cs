using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;
using System.Text.Json;

namespace AccessControl_Web.Services
{
    public class BaseService : IBaseServices
    {
        public IHttpClientFactory _httpClient { get; set; }

        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        public ApiResponse<object> ResponseModel { get; set; }

        public BaseService(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
            this.ResponseModel = new();
        }

        public async Task<T?> SendAsync<T>(ApiRequest apiRequest)
        {
            try
            {
                var client = _httpClient.CreateClient("AccessControlAPI");

                var message = new HttpRequestMessage
                {
                    RequestUri = new Uri(apiRequest.Url!),
                    Method = GetHttpMethod(apiRequest.ApiType)
                };

                if (apiRequest.Data != null)
                {
                    message.Content = JsonContent.Create(apiRequest.Data, options: _options);
                }

                var apiResponse = await client.SendAsync(message);

                return await apiResponse.Content.ReadFromJsonAsync<T>(_options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return default(T);
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
