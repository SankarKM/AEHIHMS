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
    public class EditAddressRepository : RepositoryBase<Edit_Address>, IEditAddressRepository
    {
        private readonly IHMSContext _context;
        public EditAddressRepository(IHMSContext context) : base(context)
        {
            _context = context;
        }


        public Edit_Address GetReviewPatient(string no, int siteId)
        {


        


            var EditAddress = new Edit_Address();
            EditAddress.PrLog = new Patient_Registration_Edit_Log();
            EditAddress.Master = new PatientRegistrationMaster();
            no = Context.Patient_Registration_Master.Where(x => x.MR_NO == no || x.UIN == no && x.SiteId == siteId).Select(x => x.MR_NO).FirstOrDefault();
            if (!string.IsNullOrEmpty(no))
            {
                var master = Context.Patient_Registration_Master.Where(x => x.MR_NO == no && x.SiteId == siteId).AsNoTracking()
                    //.Include(x => x.Patient_Registration_Detail)
                    .Include(x => x.Patient_Registration_Master_Address)
                    .FirstOrDefault();
                EditAddress.Master = master;
                var now = DateTime.Now;
                var dbo = EditAddress.Master.Date_Of_Birth;
                var age = now.Year - dbo.Year;
                if (dbo > now.AddYears(-age))
                    age--;
                EditAddress.Age = age;




                if (master != null)
                {
                    if (master.Patient_Registration_Master_Address.Count() > 0)
                    {
                        master.Patient_Registration_Master_Address.All(x => { x.Patient_Registration_Master = null; return true; });
                        EditAddress.PermanentAddress = master.Patient_Registration_Master_Address.Where(x => x.SiteId == siteId).LastOrDefault();


                       

                    }
                    else
                    {
                        EditAddress.PermanentAddress = new PatientRegistrationMasterAddress();
                        EditAddress.PermanentAddress.SiteId = master.SiteId;
                    }
                    
                    master.Patient_Registration_Master_Address = null;
                    EditAddress.Master = master;
                }
            }
            //  }
            return EditAddress;
        }


        public dynamic UpdateEditAddress(Edit_Address EditAddress)
        {
            var PatientRegistrationEditLogOLD = _context.Patient_Registration_Master.Where(x => x.MR_NO == EditAddress.Master.MR_NO).AsNoTracking().FirstOrDefault();

            //PatientRegistrationEditLogOLD.MR_NO = "0";
         
            var PatientRegistration_EditLog = new Patient_Registration_Edit_Log();
            PatientRegistration_EditLog.Mr_No = EditAddress.Master.MR_NO;
            PatientRegistration_EditLog.Uin = EditAddress.Master.UIN;
            PatientRegistration_EditLog.Patient_Name = EditAddress.Master.Patient_Name;
            PatientRegistration_EditLog.Next_Of_Kin = EditAddress.Master.Next_Of_Kin;
            PatientRegistration_EditLog.Date_Of_Birth = EditAddress.Master.Date_Of_Birth;
            PatientRegistration_EditLog.Sex = EditAddress.Master.Sex;
            PatientRegistration_EditLog.Door = EditAddress.Master.Door;
            PatientRegistration_EditLog.Street_Locality = EditAddress.Master.Street_Locality;
            PatientRegistration_EditLog.Pincode = EditAddress.Master.Pincode;
            PatientRegistration_EditLog.Town_City = EditAddress.Master.Town_City;
            PatientRegistration_EditLog.Taluk = EditAddress.Master.Taluk;
            PatientRegistration_EditLog.District = EditAddress.Master.District;
            PatientRegistration_EditLog.State = EditAddress.Master.State;
            PatientRegistration_EditLog.Country = EditAddress.Master.Country;
            PatientRegistration_EditLog.Police_Station = EditAddress.Master.Police_Station;
            PatientRegistration_EditLog.Phone = EditAddress.Master.Phone;
            PatientRegistration_EditLog.Email_Id = EditAddress.Master.Email_Id;
            PatientRegistration_EditLog.Registered_Date = EditAddress.Master.Registered_Date.Date;
            PatientRegistration_EditLog.Last_Visit_Date = EditAddress.Master.Last_Visit_Date.Date;
            PatientRegistration_EditLog.Next_Of_Kin = EditAddress.Master.Next_Of_Kin;
            PatientRegistration_EditLog.Operator_Code = "1";
            PatientRegistration_EditLog.Log_Date = DateTime.Now.Date;
            PatientRegistration_EditLog.Sysdate = DateTime.Now.Date;

            PatientRegistration_EditLog.Country_Old = PatientRegistrationEditLogOLD.Country;
            PatientRegistration_EditLog.Date_Of_Birth_Old = PatientRegistrationEditLogOLD.Date_Of_Birth;
            PatientRegistration_EditLog.District_Old = PatientRegistrationEditLogOLD.District;
            PatientRegistration_EditLog.Door_Old = PatientRegistrationEditLogOLD.Door;
            PatientRegistration_EditLog.Email_Id_Old = PatientRegistrationEditLogOLD.Email_Id;
            PatientRegistration_EditLog.Next_Of_Kin_Old = PatientRegistrationEditLogOLD.Next_Of_Kin;
            PatientRegistration_EditLog.Patient_Name_Old = PatientRegistrationEditLogOLD.Patient_Name;
            PatientRegistration_EditLog.Phone_Old = PatientRegistrationEditLogOLD.Police_Station;
            PatientRegistration_EditLog.Police_Station_Old = PatientRegistrationEditLogOLD.Police_Station;
            PatientRegistration_EditLog.Sex_Old = PatientRegistrationEditLogOLD.Sex;
            PatientRegistration_EditLog.State_Old = PatientRegistrationEditLogOLD.State;
            PatientRegistration_EditLog.Street_Locality_Old = PatientRegistrationEditLogOLD.Street_Locality;
            PatientRegistration_EditLog.Taluk_Old = PatientRegistrationEditLogOLD.Taluk;
            PatientRegistration_EditLog.Town_City_Old = PatientRegistrationEditLogOLD.Town_City;
            PatientRegistration_EditLog.Pincode_Old = PatientRegistrationEditLogOLD.Pincode;
            PatientRegistration_EditLog.Next_Of_Kin_Old = PatientRegistrationEditLogOLD.Next_Of_Kin;
            PatientRegistration_EditLog.Log_Date = DateTime.Now.Date;
			Context.Patient_Registration_Edit_Log.Add(PatientRegistration_EditLog);

			PatientRegistrationEditLogOLD = null;

            Context.Entry(EditAddress.Master).State = EntityState.Modified;
            EditAddress.PermanentAddress.UpdatedUTC = DateTime.Now.Date;
			EditAddress.PermanentAddress.UpdatedBy = 9999;
			EditAddress.PermanentAddress.CreatedUTC = DateTime.Now;
			Context.Entry(EditAddress.PermanentAddress).State = EntityState.Modified;

          
          







            try
            {
                if (Context.SaveChanges() > 0)
                    return new
                    {
                        Success = true,
                        Message = "Patient Edit Details saved successfully"
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




            //return Context.SaveChanges() > 0;

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









    

    