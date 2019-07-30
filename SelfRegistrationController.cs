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
    public class SelfRegistrationController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public SelfRegistrationController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        [HttpPost("UpdateSelfregistration")]
        public dynamic UpdateSelfregistration([FromBody] Self_Registration Kiosk)
        {
            return _repoWrapper.Kiosk.UpdateSelfregistration(Kiosk);
        }

        [HttpGet("ageOrDboChange/{type}/{value}")]
        public dynamic AgeOrDboChange(string type, string value)
        {
            return _repoWrapper.Kiosk.AgeOrDboChange(type, value);
        }


        [HttpPost("uploadImage/{patientname}")]
        public bool UploadImage(string patientname)
        {
            var file = Request.Form.Files[0];
            return _repoWrapper.Kiosk.UploadImage(file, patientname);
        }

        [HttpGet("getPatientImage/{patientname}")]
        public FileResult GetPatientImage(string patientname)
        {
            var path = _repoWrapper.Kiosk.GetPatientImagePath(patientname);
            var fileContents = System.IO.File.ReadAllBytes(path);
            var fileExtension = System.IO.Path.GetExtension(path);
            if (path != null)
                return File(fileContents, $"image/{fileExtension.Substring(1)}");
            return null;
        }












    }
}

