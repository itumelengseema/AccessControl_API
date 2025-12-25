using AccessControl_API.Data;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl_API.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    public class AccessControll : ControllerBase
    {

        private readonly AppDbContext _db;

        public AccessControll(AppDbContext db)
        {
            _db = db;
        }



    }
}
