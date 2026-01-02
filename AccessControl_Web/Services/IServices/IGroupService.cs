using AccessControl_API.Models.DTO;

namespace AccessControl_Web.Services.IServices
{
    public interface IGroupService
    {
        Task<ApiResponse<GroupDTO>?> CreateGroupAsync(GroupDTO groupDto);
        Task<ApiResponse<IEnumerable<GroupDTO>>?> GetAllGroupsAsync();
        Task<ApiResponse<IEnumerable<UsersPerGroupDTO>>?> GetUsersPerGroupCountAsync();
    }
}
