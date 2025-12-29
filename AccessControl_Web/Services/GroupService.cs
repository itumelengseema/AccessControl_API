using AccessControl_API.Models.DTO;
using AccessControl_Web.Services.IServices;

namespace AccessControl_Web.Services
{
    public class GroupService : IGroupService
    {
        private readonly IBaseServices _baseService;

        public GroupService(IBaseServices baseService)
        {
            _baseService = baseService;
        }

        public async Task<ApiResponse<GroupDTO>?> CreateGroupAsync(GroupDTO groupDto)
        {
            return await _baseService.SendAsync<ApiResponse<GroupDTO>>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Url = SD.AccessControlAPIBase + SD.GroupAPIBase,
                Data = groupDto
            });
        }

        public async Task<ApiResponse<IEnumerable<GroupDTO>>?> GetAllGroupsAsync()
        {
            return await _baseService.SendAsync<ApiResponse<IEnumerable<GroupDTO>>>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = SD.AccessControlAPIBase + SD.GroupAPIBase
            });
        }

        public async Task<ApiResponse<IEnumerable<UsersPerGroupDTO>>?> GetUsersPerGroupCountAsync()
        {
            return await _baseService.SendAsync<ApiResponse<IEnumerable<UsersPerGroupDTO>>>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = SD.AccessControlAPIBase + SD.GroupUsersCount
            });
        }
    }
}
