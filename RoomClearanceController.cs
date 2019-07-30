using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApiCore.Controllers
{
    [Route("[controller]")]
    public class RoomClearanceController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public RoomClearanceController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

       
        [HttpGet("GetAllRoomStatus/{filter?}/{OccupyStatus?}")]
        public IEnumerable<Room_Clearance> GetAllRoomStatus(string filter,string OccupyStatus)
        {
            return _repoWrapper.RoomClearance.GetAllRoomStatus(filter,OccupyStatus);
        }

        [HttpPost("UpdateRoomClearanceStatus")]
        public dynamic Post([FromBody] List<Room_Clearance> RoomClearance)
        {
            return _repoWrapper.RoomClearance.UpdateRoomClearanceStatus(RoomClearance);
        }
    }
}