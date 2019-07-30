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
    public class PatientDischargeController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public PatientDischargeController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }


        [HttpGet("GetPatientDetails/{UIN}/{emrSiteId?}")]
        public Patient_Discharge GetPatientDetails(string UIN, int? emrSiteId = null)
        {
            return _repoWrapper.PatientDischarge.GetPatientDetails(UIN, emrSiteId);
        }

        [HttpGet("GetTestCostDetails/{TestCode}")]
        public Patient_Discharge GetTestCostDetails(string TestCode)
        {
            return _repoWrapper.PatientDischarge.GetTestCostDetails(TestCode);
        }

        [HttpGet("getCorporates")]
        public IEnumerable<Dropdown> GetCorporates()
        {
            return _repoWrapper.PatientDischarge.GetCorporates();
        }

        [HttpGet("getViewCollectionByOperator/{operatorCode}")]
        public double GetViewCollectionByOperator(string operatorCode)
        {
            return _repoWrapper.PatientDischarge.GetViewCollectionByOperator(operatorCode);
        }




        [HttpGet("getEmployeeGrade/{corporateCode}")]
        public IEnumerable<Dropdown> GetEmployeeGrade(string corporateCode)
        {
            return _repoWrapper.PatientDischarge.GetEmployeeGrade(corporateCode);
        }
        [HttpGet("getSelectedCategoryDetails/{categoryCode}")]
        public dynamic GetSelectedCategoryDetails(string categoryCode)
        {
            return _repoWrapper.PatientDischarge.GetSelectedCategoryDetails(categoryCode);
        }

        [HttpPost("updatePatientDischarge")]
        public dynamic UpdatePatientDischarge([FromBody] Patient_Discharge patientDischarge)
        {
            return _repoWrapper.PatientDischarge.updatePatientDischarge(patientDischarge);
        }


        [HttpGet("GetipaDetails/{ipa_no}/{siteId?}")]
        public dynamic GetipaDetails(string ipa_no, int? siteId = null)
        {
            return _repoWrapper.PatientDischarge.GetipaDetails(ipa_no, siteId);
        }




    }
}   
