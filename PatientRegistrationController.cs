using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Repository;
using IHMS.Data.Repository.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiCore.Controllers
{
    [Route("[controller]")]
    public class PatientRegistrationController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public PatientRegistrationController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        [HttpGet("getViewCollectionByOperator/{operatorCode}/{siteId}")]
        public double GetViewCollectionByOperator(string operatorCode, int siteId)
        {
            return _repoWrapper.PatientRegistration.GetViewCollectionByOperator(operatorCode, siteId);
        }

        [HttpGet("SaveImage/{ImgStr}")]
        public dynamic SaveImage(string Imagename)
        {
            return _repoWrapper.PatientRegistration.SaveImage(Imagename);
        }



        [HttpGet("getAllocation/{specialityCode}/{siteCode}")]
        public IEnumerable<Dropdown> GetAllocation(string specialityCode, string siteCode)
        {
            return _repoWrapper.PatientRegistration.GetAllocation(specialityCode, siteCode);
        }

        [HttpGet("ageOrDboChange/{type}/{value}")]
        public dynamic AgeOrDboChange(string type, string value)
        {
            return _repoWrapper.PatientRegistration.AgeOrDboChange(type, value);
        }

        [HttpGet("getRegistrationFees/{specialityCode}/{instance}/{type?}")]
        public decimal GetFees(string specialityCode, string instance, string type = "")
        {
            return _repoWrapper.PatientRegistration.GetRegistrationFees(specialityCode, instance, type);
        }

        [HttpGet("getCorporates")]
        public IEnumerable<Dropdown> GetCorporates()
        {
            return _repoWrapper.PatientRegistration.GetCorporates();
        }

        [HttpGet("getEmployeeGrade/{corporateCode}")]
        public IEnumerable<Dropdown> GetEmployeeGrade(string corporateCode)
        {
            return _repoWrapper.PatientRegistration.GetEmployeeGrade(corporateCode);
        }

        [HttpGet("getNewPatientInfoByUIN/{uin}/{siteId}")]
        public Patient_Registration UinSearch(string uin, int siteId)
        {
            return _repoWrapper.PatientRegistration.UinSearch(uin, siteId);
        }

        [HttpGet("getReviewPatient/{no}/{siteId}/{isMrNo}/{patientClass}")]
        public Patient_Registration GetReviewPatient(string no, int siteId, bool isMrNo, string patientClass)
        {
            return _repoWrapper.PatientRegistration.GetReviewPatient(no, siteId, isMrNo, patientClass);
        }

        [HttpGet("getSelectedCategoryDetails/{categoryCode}")]
        public dynamic GetSelectedCategoryDetails(string categoryCode)
        {
            return _repoWrapper.PatientRegistration.GetSelectedCategoryDetails(categoryCode);
        }

        [HttpPost("updatePatientRegistration")]
        public dynamic Post([FromBody] Patient_Registration patientRegistration)
        {
            return _repoWrapper.PatientRegistration.UpdatePatientRegistration(patientRegistration);
        }

        [HttpPost("uploadImage/{uin}")]
        public bool UploadImage(string uin)
        {
            var file = Request.Form.Files[0];
            return _repoWrapper.PatientRegistration.UploadImage(file, uin);
        }

        [HttpGet("getPatientImage/{uin}")]
        public FileResult GetPatientImage(string uin)
        {
            var path = _repoWrapper.PatientRegistration.GetPatientImagePath(uin);
            var fileContents = System.IO.File.ReadAllBytes(path);
            var fileExtension = System.IO.Path.GetExtension(path);
            if (path != null)
                return File(fileContents, $"image/{fileExtension.Substring(1)}");
            return null;
        }

        [HttpGet("getPatientRegistrationList/{siteId}")]
        public List<PatientRegistrationList> GetPatientRegistrationList(int siteId)
        {
            return _repoWrapper.PatientRegistration.GetPatientRegistrationList(siteId);
        }

        [HttpGet("referralDoctorChange/{code}")]
        public dynamic ReferralDoctorChange(string code)
        {
            return _repoWrapper.PatientRegistration.ReferralDoctorChange(code);
        }

        [HttpGet("recall/{mrNo}")]
        public dynamic Recall(string mrNo)
        {
            return _repoWrapper.PatientRegistration.Recall(mrNo);
        }

        [HttpGet("getAllUnits/{specialityCode}/{siteCode}/{siteId}")]
        public dynamic GetAllUnits(string specialityCode, string siteCode, int siteId)
        {
            return _repoWrapper.PatientRegistration.GetAllUnits(specialityCode, siteCode, siteId);
        }

        [HttpPost("updateFunctionalUnits/{specialityCode}/{siteId}")]
        public bool UpdateFunctionalUnits(string specialityCode, int siteId, [FromBody] FunctionUnits[] units)
        {
            return _repoWrapper.PatientRegistration.UpdateFunctionalUnits(specialityCode, siteId, units);
        }

        [HttpGet("getVisitCancelDetails/{no}/{siteId}")]
        public ReceiptCancel GetVisitCancelDetails(string no, int siteId)
        {
            return _repoWrapper.PatientRegistration.GetVisitCancelDetails(no, siteId);
        }

        [HttpPost("saveCancelVisit")]
        public bool SaveCancelVisit([FromBody] ReceiptCancel receiptCancel)
        {
            return _repoWrapper.PatientRegistration.SaveCancelVisit(receiptCancel);
        }

        [HttpGet("getReferralDetails/{no}/{isMrNo}/{siteId}")]
        public ReferralViewModel GetReferralDetails(string no, bool isMrNo, int siteId)
        {
            return _repoWrapper.PatientRegistration.GetReferralDetails(no, isMrNo, siteId);
        }

        [HttpPost("saveReferral")]
        public dynamic SaveReferral([FromBody] ReferralPatientsHDR referral)
        {
            return _repoWrapper.PatientRegistration.SaveReferral(referral);
        }

        [HttpGet("translate/{patientName}/{langCode}")]
        public dynamic Translate(string patientName, string langCode = "ta")
        {
            return _repoWrapper.PatientRegistration.Translate(patientName, langCode);
        }


        [HttpGet("Reprint/{mrNo}")]
        public dynamic Reprint(string mrNo)
        {
            return _repoWrapper.PatientRegistration.Reprint(mrNo);
        }

        [HttpGet("OTP/{PIN}")]
        public dynamic OTP(string PIN)
        {
            return _repoWrapper.PatientRegistration.OTP(PIN);
        }





    }
}
