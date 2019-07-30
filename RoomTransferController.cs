using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IHMS.Data.Model;
using IHMS.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApiCore.Controllers
{
    [Route("[controller]")]
    public class RoomTransferController : Controller
    {
        private IRepositoryWrapper _repoWrapper;
        public RoomTransferController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        [HttpGet("GetPatientDetail/{Mr_No?}")]
        public Patient_Registration GetPatientDetail(string Mr_No)
        {
            return _repoWrapper.RoomTransfer.GetPatientDetail(Mr_No);
        }

        [HttpGet("GetAllPatients/{SearchText?}")]
        public IEnumerable<IPAdmission> GetAllPatients(string SearchText)
        {
            return _repoWrapper.RoomTransfer.GetAllPatients(SearchText);
        }

        [HttpGet("GetSurgeryDetail/{SurgeryCode?}/{IPANo}")]
        public IpSurgeryDtl GetSurgeryDetail(string SurgeryCode,string IPANo)     
        {
            return _repoWrapper.RoomTransfer.GetSurgeryDetail(SurgeryCode,IPANo);
        }

        [HttpGet("GetCurrentRoomStatus/{RoomType?}/{Floor?}/{ToiletType?}")]
        public IEnumerable<CurrentRoomStatus> GetCurrentRoomStatus(string RoomType, string Floor, string ToiletType)
        {
            return _repoWrapper.RoomTransfer.GetCurrentRoomStatus(RoomType, Floor, ToiletType);
        }

        [HttpPost("UpdateRoomTransferDetails/{UIN}")]
        public dynamic UpdateRoomTransferDetails(string UIN,[FromBody] RoomTransfer roomTransfer)
        {
            return _repoWrapper.RoomTransfer.UpdateRoomTransferDetails(roomTransfer,UIN);
        }

        [HttpGet("GetExpectedRoomVacancy")]
        public List<RoomVacancy> GetExpectedRoomVacancy()
        {
            return _repoWrapper.RoomTransfer.GetExpectedRoomVacancy(DateTime.Now, DateTime.Now.AddDays(1));
        }


    }
}