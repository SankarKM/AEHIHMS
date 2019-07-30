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
    public class CampDischargeController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        
        public CampDischargeController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        [HttpGet("GetCampDischarge/{no}/{siteId}/{isIPNO}")]
        public dynamic GetCampDischarge(string no, int siteId,bool isIPNO)
        {
            return _repoWrapper.Campdischarge.GetCampDischarge(no, siteId, isIPNO);
        }


        [HttpPost("InsertCampDischarge")]
        public dynamic Post([FromBody] Camp_Discharge CampDischarge)
        {
            return _repoWrapper.Campdischarge.InsertCampDischarge(CampDischarge);
        }


    }
}
