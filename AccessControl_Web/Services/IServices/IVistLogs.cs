namespace AccessControl_Web.Services.IServices
{
    public interface IVistLogs
    {

        Task<T?> CheckInAsync<T>(string token);
        Task<T?> CheckOutAsync<T>(int visitLogId, string token);

        Task<T?> ActiveVisitorsAsync<T>(string token);


    }
}
