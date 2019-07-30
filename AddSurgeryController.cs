using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Model.ViewModel;
//using IHMS.Data.Model.ViewModel;
using IHMS.Data.Repository;
using IHMS.Data.Repository.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiCore.Controllers
{
    [Route("[controller]")]
    public class AddSurgeryController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public AddSurgeryController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        [HttpGet("getAddSurgeryPatient/{no}/{siteId}/{isMrNo}")]
        public Add_Surgery GetAddSurgeryPatient(string no, int siteId,bool isMrNo)
        {
            return _repoWrapper.AddSugery.GetAddSurgeryPatient(no, siteId, isMrNo);
        }

        [HttpPost("UpdateAddSugery")]
        public dynamic UpdateAddSugery([FromBody] Add_Surgery AddSugery)
        {
            return _repoWrapper.AddSugery.UpdateAddSugery(AddSugery);
        }


















    }
}

