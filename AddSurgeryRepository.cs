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
    public class AddSurgeryRepository : RepositoryBase<Add_Surgery>, IAddSurgeryRepository
    {
        private readonly IHMSContext _context;
        public AddSurgeryRepository(IHMSContext context) : base(context)
        {
            _context = context;
        }


        public Add_Surgery GetAddSurgeryPatient(string no, int siteId ,bool isMrNo)

            {

            var AddSurgery = new Add_Surgery();

            AddSurgery.OTSurgeryHdr = new OTSurgeryHdr();

            if(isMrNo==true)
            {
                AddSurgery.IPAdmission = Context.Ip_Admission.Where(x => (x.Mr_No == no) && x.SiteId == siteId).OrderByDescending(x => x.Admission_Date).FirstOrDefault();
            }
            else
            {
                AddSurgery.IPAdmission = Context.Ip_Admission.Where(x => (x.UIN == no) && x.SiteId == siteId).OrderByDescending(x => x.Admission_Date).FirstOrDefault();

            }
            

            if (AddSurgery.IPAdmission != null)
            {
                var surgerydone = Context.Ip_Surgery_Dtl.Where(x => x.Ipa_No == AddSurgery.IPAdmission.Ipa_No && x.SiteId == siteId).Select(x => x.Surgery_Done).FirstOrDefault();

                if (AddSurgery.IPAdmission.Discharge_Status == "ADM" && surgerydone == "N")
                {
                    
                AddSurgery.Master = Context.Patient_Registration_Master.FirstOrDefault(x => x.UIN ==AddSurgery.IPAdmission.UIN && x.SiteId == siteId);
                AddSurgery.Master.Taluk = Context.Taluk_Master.Where(x => x.Taluk_Code == AddSurgery.Master.Taluk).Select(x => x.Taluk_Name).FirstOrDefault();
                AddSurgery.Master.District = Context.District_Master.Where(x => x.District_Code == AddSurgery.Master.District).Select(x => x.District_Name).FirstOrDefault();
                AddSurgery.Master.State = Context.State_Master.Where(x => x.State_Code == AddSurgery.Master.State).Select(x => x.State_Name).FirstOrDefault();
                AddSurgery.SiteMaster = Context.Site_Master.FirstOrDefault(x => x.Site_Code == AddSurgery.Master.Patient_Class);
                AddSurgery.OTSurgeryHdr = Context.OT_Surgery_Hdr.FirstOrDefault(x => x.Mr_No == AddSurgery.IPAdmission.Mr_No && x.Ip_No==AddSurgery.IPAdmission.Ip_No);
                AddSurgery.ICDCodeMaster = Context.ICD_Code_Master.FirstOrDefault(x => x.Icd_Code == AddSurgery.IPAdmission.Surgery_Code);
                AddSurgery.EyeMaster = Context.EYEMASTER.FirstOrDefault(x => x.EyeCode == AddSurgery.IPAdmission.Eye);
                AddSurgery.IpSurgeryDtl = Context.Ip_Surgery_Dtl.FirstOrDefault(x => x.UIN == AddSurgery.IPAdmission.UIN && x.SiteId == siteId);
                AddSurgery.IPClinic = Context.IP_Clinic.FirstOrDefault(x => x.Uin == AddSurgery.IPAdmission.UIN && x.Siteid == siteId );
                //AddSurgery.location = Context.Location_Master.FirstOrDefault(x => x.Location_Code == AddSurgery.IPClinic.Clinic_Code);
                AddSurgery.DoctorMaster = Context.Doctor_Master.FirstOrDefault(x => x.Doctor_Code == AddSurgery.IPAdmission.Doctor_Code);

                var now = DateTime.Now;
                var dbo = AddSurgery.Master.Date_Of_Birth;
                var age = now.Year - dbo.Year;
                if (dbo > now.AddYears(-age))

                    age--;
                AddSurgery.Age = age;




                if (AddSurgery.Master.Sex == "M")
                {
                    AddSurgery.Master.Sex = "Male";
                }
                else if (AddSurgery.Master.Sex == "F")
                {
                    AddSurgery.Master.Sex = "FeMale";
                }


            }
                else
                {
                    AddSurgery.IpSurgeryDtl = Context.Ip_Surgery_Dtl.FirstOrDefault(x => x.Ipa_No == AddSurgery.IPAdmission.Ipa_No && x.SiteId == siteId);

                }
            }



            return AddSurgery;
        }

        public dynamic UpdateAddSugery(Add_Surgery AddSugery)
        {
            var AddSurgeryupdate = _context.Ip_Admission.Where(x => x.Ipa_No == AddSugery.IPAdmission.Ipa_No).AsNoTracking().FirstOrDefault();


            var IPSURGERYDTL = new IpSurgeryDtl();

            IPSURGERYDTL.Ipa_No = AddSugery.IPAdmission.Ipa_No;
            IPSURGERYDTL.Eye = AddSugery.IPAdmission.Eye;
            IPSURGERYDTL.Anaesthesia = AddSugery.IPAdmission.Anesthesia;
            IPSURGERYDTL.SiteId = AddSugery.IPAdmission.SiteId;
            IPSURGERYDTL.UIN = AddSugery.IPAdmission.UIN;
            IPSURGERYDTL.Surgery_Code = AddSugery.IPAdmission.Surgery_Code;
            IPSURGERYDTL.Surgery_Type_Code = "T";
            IPSURGERYDTL.Surgery_Type_Code = AddSugery.IpSurgeryDtl.Surgery_Type_Code;
            IPSURGERYDTL.Doctor_Code = AddSugery.IpSurgeryDtl.Doctor_Code;
            IPSURGERYDTL.Surgery_Approval = AddSugery.IpSurgeryDtl.Surgery_Approval;
            IPSURGERYDTL.Surgery_Done = AddSugery.IpSurgeryDtl.Surgery_Done;
            IPSURGERYDTL.Surgery_Date = DateTime.Now.Date;
            Context.Ip_Surgery_Dtl.Add(IPSURGERYDTL);

            var OTSURGERYHDR = new OTSurgeryHdr();

            OTSURGERYHDR.Surgery_No= Convert.ToInt32(GenerateRunningCtrlNo("Surgery_No", 1));
            OTSURGERYHDR.Surgery_Date = AddSugery.IpSurgeryDtl.Surgery_Date;
            OTSURGERYHDR.UIN = AddSugery.IPAdmission.UIN;
            OTSURGERYHDR.SiteId = AddSugery.IPAdmission.SiteId;
            OTSURGERYHDR.Ipa_No = AddSugery.IPAdmission.Ipa_No;
            OTSURGERYHDR.Mr_No = AddSugery.IPAdmission.Mr_No;
            OTSURGERYHDR.Ip_No = AddSugery.IPAdmission.Ip_No;
            OTSURGERYHDR.Eye = AddSugery.IPAdmission.Eye;
            OTSURGERYHDR.Surgery_Code = AddSugery.IPAdmission.Surgery_Code;
            OTSURGERYHDR.Anesthesia_Code = AddSugery.IPAdmission.Anesthesia;
            OTSURGERYHDR.Doctor_Code = AddSugery.IPAdmission.Doctor_Code;
            OTSURGERYHDR.Iol_Power = AddSugery.OTSurgeryHdr.Iol_Power;
            OTSURGERYHDR.Iol_Type = AddSugery.OTSurgeryHdr.Iol_Type;
            OTSURGERYHDR.Sysdate = DateTime.Now;
            Context.OT_Surgery_Hdr.Add(OTSURGERYHDR);

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


        private string GenerateRunningCtrlNo(string rnControlCode, int siteId = 1)
        {
            var rn = Context.Running_Number_Control.Where(x => x.RnControl_Code == rnControlCode && x.IsActive == true && x.SiteId == siteId).FirstOrDefault();
            if (rn != null)
            {
                rn.Control_Value += 1;
                Context.Entry(rn).State = EntityState.Modified;
                return $"{rn.Control_String_Value}{rn.Control_Value}";
            }
            else
                return "";
        }




    }
}









    

    