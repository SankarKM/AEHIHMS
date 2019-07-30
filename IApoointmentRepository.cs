using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Model.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IHMS.Data.Repository
{
    public interface IAppointmentRepository : IRepositoryBase<Appointment>
    {
       Appointment GetAppointmentPatient(string no, int siteId);
       dynamic UpdateAppointment(Appointment Appointment);
      
    } 
}
