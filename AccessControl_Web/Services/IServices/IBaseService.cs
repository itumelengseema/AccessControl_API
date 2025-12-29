using AccessControl_API.Models.DTO;

namespace AccessControl_Web.Services.IServices
{
    public interface IBaseServices
    {
        ApiResponse<object> ResponseModel { get; set; }
        Task<T?> SendAsync<T>(ApiRequest apiRequest);

    }
}
