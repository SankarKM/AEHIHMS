using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Model.ViewModel;
using IHMS.Data.Repository;
using IHMS.Data.Repository.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiCore.Controllers
{
    [Route("[controller]")]
    public class CampAdmissionController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public CampAdmissionController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        [HttpGet("getCampAdmission/{CampCode}/{siteId}")]
        public CampAdmission GetCampAdmission(string CampCode, int siteId)
        {
            return _repoWrapper.CampAdmission.GetCampAdmission(CampCode, siteId);
        }
        [HttpPost("ToDBCampAdmission")]
        public dynamic Post([FromBody] CampAdmission CampAdmission)
        {
            return _repoWrapper.CampAdmission.ToDBCampAdmission(CampAdmission);
        }

        [HttpGet("RoomChangeEvent/{RoomType}/{RoomNo}")]
        public dynamic RoomChangeEvent(string RoomType, string RoomNo)
        {
            return _repoWrapper.CampAdmission.RoomChangeEvent(RoomType, RoomNo);
        }


        [HttpGet("GetpopDetails/{MR_NO}/{siteId?}")]
        public CampAdmission GetpopDetails(string MR_NO, int? siteId = null)
        {
            return _repoWrapper.CampAdmission.GetpopDetails(MR_NO, siteId);
        }


        [HttpGet("GetipaDetails/{ipa_no}/{siteId?}")]
        public dynamic GetipaDetails(string ipa_no, int? siteId = null)
        {
            return _repoWrapper.CampAdmission.GetipaDetails(ipa_no, siteId);
        }


    }
}
