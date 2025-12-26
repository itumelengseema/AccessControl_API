using AccessControl_API.Data;
using AccessControl_API.Models;
using AccessControl_API.Models.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccessControl_API.Controllers
{
    [Route("api/groups")]
    [ApiController]
    public class GroupsController : ControllerBase
    {

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public GroupsController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<GroupDTO>>> CreateGroup([FromBody] GroupDTO groupDto)
        {
            var group = _mapper.Map<Group>(groupDto);
            _db.Groups.Add(group);
            await _db.SaveChangesAsync();


            return ApiResponse<GroupDTO>.CreatedResponse(_mapper.Map<GroupDTO>(group), "Group created successfully");
        }



        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GroupDTO>>>> GetGoups()
        {
            var groups = await _db.Groups.ToListAsync();

            var groupDtos = _mapper.Map<IEnumerable<GroupDTO>>(groups);

            return ApiResponse<IEnumerable<GroupDTO>>.SuccessResponse(groupDtos, "Groups retrieved successfully");



        }

        [HttpGet("users-count")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UsersPerGroupDTO>>>> GetUsersPerGroupCoun()
        {
            var data = await _db.Groups
                .Select(g => new UsersPerGroupDTO
                {
                    GroupName = g.Name,
                    UserCount = g.UserGroups.Count
                })
                .ToListAsync();

            return ApiResponse<IEnumerable<UsersPerGroupDTO>>.SuccessResponse(data, "User counts per group retrieved successfully");
        }
    }
}
