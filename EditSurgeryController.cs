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
    public class EditSurgeryController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public EditSurgeryController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        [HttpGet("getEditSurgeryPatient/{no}/{siteId}/{isMrNo}")]
        public Edit_Surgery GetEditSurgeryPatient(string no, int siteId,bool isMrNo)
        {
            return _repoWrapper.EditSugery.GetEditSurgeryPatient(no, siteId, isMrNo);
        }

        [HttpPost("UpdateEditSugery")]
        public dynamic UpdateEditSugery([FromBody] Edit_Surgery EditSugery)
        {
            return _repoWrapper.EditSugery.UpdateEditSugery(EditSugery);
        }
    }
}

