using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Repository;
using IHMS.Data.Repository.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebApiCore.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public HomeController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        [HttpGet("dummy")]
        public string Dummy()
        {
            return "hi";
        }

        [HttpGet("test")]
        public async Task<object> Test()
        {
            Thread.Sleep(5000);
            return await _repoWrapper.Common.GetDropdown("Site_Master", "Site_Code", "Site_Name");
        }

        [Authorize, HttpGet("getAllPatients")]
        public IEnumerable<Patient> Get()
        {
            return _repoWrapper.Patient.FindAll();
        }

        [HttpPost("addPatient")]
        public bool Post([FromBody] Patient patient)
        {
            //var file = Request.Form.Files[0];
            _repoWrapper.Patient.Create(patient);
            return _repoWrapper.Patient.Save();
        }

        [HttpPut("updatePatient/{id}")]
        public bool UpdatePatient(int id, [FromBody]Patient patient)
        {
            _repoWrapper.Patient.Update(patient);
            return _repoWrapper.Patient.Save();
        }

        [HttpDelete("deletePatient/{id}")]
        public bool DeletePatient(int id)
        {
            var patient = _repoWrapper.Patient.FindByCondition(x => x.Id == id).FirstOrDefault();
            _repoWrapper.Patient.Delete(patient);
            return _repoWrapper.Patient.Save();
        }

        //[HttpGet("patientDropDown")]
        //public async Task<IEnumerable<Dropdown>> PatientDropDown()
        //{
        //    return await _repoWrapper.Patient.GetPatientsForDropDown();
        //}

        //[HttpGet("patientDropDown1")]
        //public async Task<IEnumerable<Dropdown>> PatientDropDown1()
        //{
        //    Thread.Sleep(2000);
        //    return await _repoWrapper.Patient.GetPatientsForDropDown();
        //}

        [HttpPost("uploadFile")]
        public bool uploadFile()
        {
            var file = Request.Form.Files[0];
            return false;
        }
    }
}
