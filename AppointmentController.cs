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
    public class AppointmentController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public AppointmentController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        } 

        [HttpGet("GetAppointmentPatient/{no}/{siteId}")]
        public Appointment GetAppointmentPatient(string no, int siteId)
        {
            return _repoWrapper.Appointment.GetAppointmentPatient(no, siteId);
        }


        [HttpPost("UpdateAppointment")]
        public dynamic UpdateAppointment([FromBody] Appointment Appointment)
        {
            return _repoWrapper.Appointment.UpdateAppointment(Appointment);
        }












    }
}

