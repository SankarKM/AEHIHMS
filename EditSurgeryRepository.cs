using IHMS.Data.Model;
using IHMS.Data.Model.ViewModel;
//using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace IHMS.Data.Repository.Implementation
{
    public class EditSurgeryRepository : RepositoryBase<Edit_Surgery>, IEditSurgeryRepository
    {
        private readonly IHMSContext _context;
        public EditSurgeryRepository(IHMSContext context) : base(context)
        {
            _context = context;
        }


        public Edit_Surgery GetEditSurgeryPatient(string no, int siteId,bool isMrNo)

            {

            var EditSurgery = new Edit_Surgery();

            EditSurgery.OTSurgeryHdr = new OTSurgeryHdr();

            if (isMrNo == true)
            {
                EditSurgery.IPAdmission = Context.Ip_Admission.Where(x => (x.Mr_No == no) && x.SiteId == siteId).OrderByDescending(x => x.Admission_Date).FirstOrDefault();
            }
            else
            {
                EditSurgery.IPAdmission = Context.Ip_Admission.Where(x => (x.UIN == no) && x.SiteId == siteId).OrderByDescending(x => x.Admission_Date).FirstOrDefault();

            }

          
            if (EditSurgery.IPAdmission != null)
            {
                var surgerydone = Context.Ip_Surgery_Dtl.Where(x => x.Ipa_No == EditSurgery.IPAdmission.Ipa_No && x.SiteId == siteId).Select(x => x.Surgery_Done).FirstOrDefault();

                if (EditSurgery.IPAdmission.Discharge_Status == "ADM" && surgerydone == "N")
                {

                    EditSurgery.Master = Context.Patient_Registration_Master.FirstOrDefault(x => x.UIN == EditSurgery.IPAdmission.UIN && x.SiteId == siteId);
                    EditSurgery.Master.Taluk = Context.Taluk_Master.Where(x => x.Taluk_Code == EditSurgery.Master.Taluk).Select(x => x.Taluk_Name).FirstOrDefault();
                    EditSurgery.Master.District = Context.District_Master.Where(x => x.District_Code == EditSurgery.Master.District).Select(x => x.District_Name).FirstOrDefault();
                    EditSurgery.Master.State = Context.State_Master.Where(x => x.State_Code == EditSurgery.Master.State).Select(x => x.State_Name).FirstOrDefault();
                    EditSurgery.SiteMaster = Context.Site_Master.FirstOrDefault(x => x.Site_Code == EditSurgery.Master.Patient_Class);
                    EditSurgery.ICDCodeMaster = Context.ICD_Code_Master.FirstOrDefault(x => x.Icd_Code == EditSurgery.IPAdmission.Surgery_Code);
                    EditSurgery.EyeMaster = Context.EYEMASTER.FirstOrDefault(x => x.EyeCode == EditSurgery.IPAdmission.Eye);
                    EditSurgery.IpSurgeryDtl = Context.Ip_Surgery_Dtl.FirstOrDefault(x => x.UIN == EditSurgery.IPAdmission.UIN && x.SiteId == siteId);
                    EditSurgery.IPClinic = Context.IP_Clinic.FirstOrDefault(x => x.Uin == EditSurgery.IPAdmission.UIN && x.Siteid == siteId);
                    //EditSurgery.location = Context.Location_Master.FirstOrDefault(x => x.Location_Code == EditSurgery.IPClinic.Clinic_Code);
                    EditSurgery.DoctorMaster = Context.Doctor_Master.FirstOrDefault(x => x.Doctor_Code == EditSurgery.IPAdmission.Doctor_Code);


                    var now = DateTime.Now;
                    var dbo = EditSurgery.Master.Date_Of_Birth;
                    var age = now.Year - dbo.Year;
                    if (dbo > now.AddYears(-age))

                        age--;
                    EditSurgery.Age = age;
                    if (EditSurgery.Master.Sex == "M")
                    {
                        EditSurgery.Master.Sex = "Male";
                    }
                    else if (EditSurgery.Master.Sex == "F")
                    {
                        EditSurgery.Master.Sex = "FeMale";
                    }
                }

                else
                {
                    EditSurgery.IpSurgeryDtl = Context.Ip_Surgery_Dtl.FirstOrDefault(x => x.Ipa_No == EditSurgery.IPAdmission.Ipa_No && x.SiteId == siteId);

                }
            }
            


            return EditSurgery;
        }
        public dynamic UpdateEditSugery(Edit_Surgery EditSugery)
        {
            var EditSurgeryupdate = _context.Ip_Admission.Where(x => x.Ipa_No == EditSugery.IPAdmission.Ipa_No).AsNoTracking().FirstOrDefault();

            var ipadmissions = Context.Ip_Admission.Where(x => x.Ipa_No == EditSugery.IPAdmission.Ipa_No).ToList();
            if (ipadmissions != null)
            {

                ipadmissions.All(x => { x.Surgery_Code = EditSugery.IPAdmission.Surgery_Code; return true; });
                Context.Ip_Admission.UpdateRange(ipadmissions);
        }
            var IPSURGERYDTL = Context.Ip_Surgery_Dtl.Where(x => x.Ipa_No == EditSugery.IPAdmission.Ipa_No).ToList();

            if (IPSURGERYDTL != null)
            {

                IPSURGERYDTL.All(x => { x.Surgery_Code = EditSugery.IPAdmission.Surgery_Code; return true; });
                Context.Ip_Surgery_Dtl.UpdateRange(IPSURGERYDTL);

              
            }
            var OTSURGERYHDR = Context.OT_Surgery_Hdr.Where(x => x.Ipa_No == EditSugery.IPAdmission.Ipa_No).ToList();

            if (OTSURGERYHDR != null)
            {

                OTSURGERYHDR.All(x => { x.Surgery_Code = EditSugery.IPAdmission.Surgery_Code; return true; });
                Context.OT_Surgery_Hdr.UpdateRange(OTSURGERYHDR);

            }

            try
            {
                if (Context.SaveChanges() > 0)
                    return new
                    {
                        Success = true,
                        Message = "Patient Edit Sugery saved successfully"
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









    

    