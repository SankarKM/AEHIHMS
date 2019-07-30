using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Model.ViewModel;
//using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IHMS.Data.Repository.Implementation
{
    public class AppointmentRepository : RepositoryBase<Appointment>, IAppointmentRepository
    {
        private readonly IHMSContext _context;
        public AppointmentRepository(IHMSContext context) : base(context)
        {
            _context = context;
        }





        public Appointment GetAppointmentPatient(string no, int siteId)

        {


            var Appointment = new Appointment();
            Appointment.Appointments = new Appointments();
            Appointment.EmrwfVisitPurposeServiceMaster = new EmrwfVisitPurposeServiceMaster();
            Appointment.PatientStatus = new PatientStatus();




            no = Context.Patient_Registration_Master.Where(x => x.MR_NO == no || x.UIN == no && x.SiteId == siteId).Select(x => x.MR_NO).FirstOrDefault();
            if (no != null)
            {
            Appointment.Admission = Context.Ip_Admission.Where(x => x.UIN == no ||x.Mr_No==no && x.SiteId == siteId).FirstOrDefault();
            if (Appointment.Admission == null|| Appointment.Admission.Discharge_Status !="ADM")
            {

                Appointment.Appointments = Context.Appointments.FirstOrDefault(x => x.Mrn ==no);
                if (Appointment.Appointments != null)
                {

                    Appointment.Appointments = Context.Appointments.FirstOrDefault(x => x.Mrn ==no);

                }

                if (no != null)
                {
                    Appointment.Master = Context.Patient_Registration_Master.FirstOrDefault(x => x.MR_NO == no && x.SiteId == siteId);
                    Appointment.Master.District = Context.District_Master.Where(x => x.District_Code == Appointment.Master.District).Select(x => x.District_Name).FirstOrDefault();
                    Appointment.Master.State = Context.State_Master.Where(x => x.State_Code == Appointment.Master.State).Select(x => x.State_Name).FirstOrDefault();
                    Appointment.PatientRegistrationDetail=Context.Patient_Registration_Detail.Where(x => x.MR_NO == no && x.SiteId == siteId).FirstOrDefault();




                        var now = DateTime.Now;
                    var dbo = Appointment.Master.Date_Of_Birth;
                    var age = now.Year - dbo.Year;
                    if (dbo > now.AddYears(-age))

                        age--;
                    Appointment.Age = age;




                    if (Appointment.Master.Sex == "M")
                    {
                        Appointment.Master.Sex = "Male";
                    }
                    else if (Appointment.Master.Sex == "F")
                    {
                        Appointment.Master.Sex = "FeMale";
                    }

                }



            }
        }
            return Appointment;
           
        }







        public dynamic UpdateAppointment(Appointment Appointment)
        {
            var Appointmentupdate = _context.Patient_Registration_Master.Where(x => x.MR_NO == Appointment.Master.MR_NO).AsNoTracking().FirstOrDefault();

            var masters = Context.Patient_Registration_Master.Where(x => x.MR_NO == Appointment.Master.MR_NO).ToList();
            if (masters != null)
            {

                masters.All(x => { x.Last_Visit_Date = DateTime.Now.Date; return true; });
                Context.Patient_Registration_Master.UpdateRange(masters);
            }

            var PatientRegDtl = new PatientRegistrationDetail();

            PatientRegDtl.MR_NO = Appointment.Master.MR_NO;
            PatientRegDtl.UIN = Appointment.Master.UIN;
            PatientRegDtl.Sysdate = DateTime.Now;
            PatientRegDtl.Visit_Number = 0;

            PatientRegDtl.Visit_Date = DateTime.Now.Date;
            PatientRegDtl.Allocation_Code = "LC009";
            PatientRegDtl.Category_Code = "CATC001";
            PatientRegDtl.Type_Code = "PC001";
            PatientRegDtl.Instance_Code = "INSC002";
            PatientRegDtl.Speciality_Code = "SPC001";
            PatientRegDtl.SiteId = Appointment.Master.SiteId;
            Context.Patient_Registration_Detail.Add(PatientRegDtl);



            var PATIENTSTATUS = new PatientStatus();

            PATIENTSTATUS.Visit_Date = DateTime.Now.Date;
            PATIENTSTATUS.MR_NO = Appointment.Master.MR_NO;
            PATIENTSTATUS.UIN = Appointment.Master.UIN;
            PATIENTSTATUS.Assign_Doctor = "N";
            PATIENTSTATUS.PurposeId = Appointment.EmrwfVisitPurposeServiceMaster.PurposeId;
            PATIENTSTATUS.SiteId = Appointment.Master.SiteId;
            Context.Patient_Status.Add(PATIENTSTATUS);


            var opVisitDatewise = _context.Op_Visit_Datewise.Where(x => x.Date == DateTime.Now.Date && x.Patient_Class == Appointment.Master.Patient_Class).FirstOrDefault();
            var isNewOpVisit = false;
            if (opVisitDatewise == null)
            {
                isNewOpVisit = true;
                opVisitDatewise = new OpVisitDatewise();
                opVisitDatewise.Date = DateTime.Now.Date;
                opVisitDatewise.Patient_Class = Appointment.Master.Patient_Class;
            }
            
                opVisitDatewise.Old += 1;
            

            if (isNewOpVisit)
                _context.Op_Visit_Datewise.Add(opVisitDatewise);
            else
                _context.Entry(opVisitDatewise).State = EntityState.Modified;


            try
            {
                if (Context.SaveChanges() > 0)
                    return new
                    {
                        Success = true,
                        Message = "saved successfully"
                    };
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return new
            {
                Success = false,
                Message = "Some data are Missing"
            };



        }



    }
}









    

    