using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Model.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IHMS.Data.Repository.Implementation
{
    public class SelfRegistrationRepository : RepositoryBase<Self_Registration>, ISelfRegistrationRepository
    {
        private readonly IHMSContext _context;
        public SelfRegistrationRepository(IHMSContext context) : base(context)
        {
            _context = context;
        }



        public dynamic UpdateSelfregistration(Self_Registration Kiosk)

        {

            var Patientotp = new OtpHoneywell();

            var PatientKiosk = new Kiosk();

            PatientKiosk.Patient_Name = Kiosk.Master.Patient_Name;
            PatientKiosk.Nextofkin = Kiosk.Master.Next_Of_Kin;
            PatientKiosk.DOB = Kiosk.Master.Date_Of_Birth;
            PatientKiosk.Locality = Kiosk.Master.Street_Locality;
            PatientKiosk.Pincode = Kiosk.Master.Pincode;
            PatientKiosk.City = Kiosk.Master.Town_City;
            PatientKiosk.Taluk = Kiosk.Master.Taluk;
            PatientKiosk.District = Kiosk.Master.District;
            PatientKiosk.State = Kiosk.Master.State;
            PatientKiosk.Country = Kiosk.Master.Country;
            PatientKiosk.MobileNo = Kiosk.Master.Police_Station;
            PatientKiosk.LandlineNo = Kiosk.Master.Phone;
            PatientKiosk.Email = Kiosk.Master.Email_Id;
            PatientKiosk.AadhaarNo = Kiosk.Master.AadhaarNo;
            PatientKiosk.Siteid = 1;

            PatientKiosk.Visit = DateTime.Now;

            var Pin = "";
            Pin = GenerateRunningCtrlNo("OTP_NUMBER", Kiosk.Master.SiteId);
            PatientKiosk.PIN = Pin;




            PatientKiosk.Gender = Kiosk.Master.Sex;
            PatientKiosk.Address = "test";
            PatientKiosk.Date = DateTime.Now.Date;

            PatientKiosk.Mr_No = " ";


            // PatientKiosk.Signature =Kiosk.Kiosk.Signature;



            PatientKiosk.Referral = "Test2";
            PatientKiosk.OtherRegistration = "test3";
            PatientKiosk.Insurancetype = "test4";

            var now = DateTime.Now;
            var dbo = Kiosk.Master.Date_Of_Birth;
            var age = now.Year - dbo.Year;
            if (dbo > now.AddYears(-age))
                age--;


            PatientKiosk.Age = Kiosk.Kiosk.Age;
            Context.patient.Add(PatientKiosk);

            Patientotp.Otp = PatientKiosk.PIN;
            Patientotp.MR_NO = " ";
            Patientotp.UIN = "";
            Patientotp.VISIT_DATE = DateTime.Now;

            Context.OTP_HONEYWELL.Add(Patientotp);



            try
            {
                if (Context.SaveChanges() > 0)
                    return new
                    {
                        Success = true,
                        Patientotps = Pin,
                        names = PatientKiosk.Patient_Name,
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

        public dynamic UploadImage(IFormFile file, string patientname)
        {



            var currentDir = Directory.GetCurrentDirectory();
            if (!Directory.Exists(currentDir + "/PatientImages/"))
                Directory.CreateDirectory(currentDir + "/PatientImages/");
            var fileName = $"{patientname}{Path.GetExtension(file.FileName)}";
            var path = $"{currentDir}/PatientImages/{fileName}";

            if ((File.Exists(path)))
                File.Delete(path);


            using (var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
                var kiosk123 = Context.patient.Where(x => x.PIN == patientname).FirstOrDefault();
                kiosk123.Signature = fileName;

                Context.Entry(kiosk123).State = EntityState.Modified;

                var tt = Context.SaveChanges() > 0;
                return tt;
   
            }

        }






        public string GetPatientImagePath(string patientname)
        {
            var imageName = Context.patient.Where(x => x.Patient_Name == patientname).Select(x => x.Signature).FirstOrDefault();
            if (imageName != null)
            {
                var currentDir = Directory.GetCurrentDirectory();
                return $"{currentDir}/PatientImages/{imageName}";
            }
            return null;
        }



        private string GenerateRunningCtrlNo(string rnControlCode, int? emrSiteId = 1)
        {
            var rn = Context.Running_Number_Control.FirstOrDefault(x => x.RnControl_Code == rnControlCode && x.IsActive == true && x.SiteId == emrSiteId);
            if (rn != null)
            {
                rn.Control_Value += 1;
                Context.Entry(rn).State = EntityState.Modified;
                return $"{rn.Control_String_Value}{rn.Control_Value}";
            }
            else
                return "";
        }

        public dynamic AgeOrDboChange(string type, string value)
        {
            var age = 0;

            dynamic calculatedValue = null;
            if (type == "age")
            {
                age = Convert.ToInt16(value);
                calculatedValue = DateTime.Now.AddYears(-age).ToString("yyyy-MM-dd");
            }
            else if (type == "dbo")
            {
                var now = DateTime.Now;
                var dbo = DateTime.Parse(value);
                age = now.Year - dbo.Year;
                if (dbo > now.AddYears(-age))
                    age--;
                calculatedValue = age;
            }


            return new
            {

                Value = calculatedValue
            };
        }






    }
}











