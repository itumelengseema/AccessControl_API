using static AccessControl_Web.SD;

namespace AccessControl_Web
{
    public class ApiRequest
    {
        public string? Url { get; set; }
        public object? Data { get; set; }
        public ApiType ApiType { get; set; } = ApiType.GET;
        public string? Token { get; set; }
    }
}
