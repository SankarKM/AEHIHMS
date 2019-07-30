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
    public class EditAddressController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public EditAddressController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        [HttpGet("getReviewPatient/{no}/{siteId}")]
        public Edit_Address GetReviewPatient(string no, int siteId)
        {
            return _repoWrapper.EditAddress.GetReviewPatient(no, siteId);
        }

        [HttpPost("UpdateEditAddress")]
        public dynamic UpdateEditAddress([FromBody] Edit_Address EditAddress)
        {
            return _repoWrapper.EditAddress.UpdateEditAddress(EditAddress);
        }

        [HttpGet("ageOrDboChange/{type}/{value}")]
        public dynamic AgeOrDboChange(string type, string value)
        {
            return _repoWrapper.EditAddress.AgeOrDboChange( type, value);
        }

















    }
}

