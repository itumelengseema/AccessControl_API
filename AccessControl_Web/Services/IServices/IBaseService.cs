namespace AccessControl_Web.Services.IServices
{
    public interface IBaseServices
    {

        Task<T> SendAsync<T>(ApiRequest apiRequest);

    }
}
