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
    public class CampDischargeRepository : RepositoryBase<Camp_Discharge>, ICampDischargeRepository
    {
        private readonly IHMSContext _context;
        public CampDischargeRepository(IHMSContext context) : base(context)
        {
            _context = context;
        }

        public Camp_Discharge GetCampDischarge(string no, int siteId, bool isIPNO)
        {

            var CampDischarge = new Camp_Discharge();
            CampDischarge.MedicalRecord = new List<MedicalRecordDtl>();
            CampDischarge.History = new List<PatientHistory>();
            CampDischarge.ClinicalDetail = new ClinicalDetail();
            CampDischarge.Master = new PatientRegistrationMaster();
            CampDischarge.Admission = new IPAdmission();
            CampDischarge.Surgery = new IpSurgeryDtl();
            CampDischarge.AdditionalProcedureTrans = new List<AdditionalProcedureTrans>();

            if (isIPNO == true)
            {

                CampDischarge.Admission = Context.Ip_Admission.Where(x => x.Discharge_Status != "DIS" && x.Ip_No == no && x.SiteId == siteId).FirstOrDefault();
            }
            else
            {
                CampDischarge.Admission = Context.Ip_Admission.Where(x => x.Discharge_Status != "DIS" && x.UIN == no && x.SiteId == siteId).FirstOrDefault();
            }






            try
            {
                if (CampDischarge.Admission != null)
                {
                    var master = Context.Patient_Registration_Master.Where(x => x.UIN == CampDischarge.Admission.UIN && x.SiteId == siteId).AsNoTracking()

                        .FirstOrDefault();
                    CampDischarge.Master = master;

                    var now = DateTime.Now;
                    var dbo = CampDischarge.Master.Date_Of_Birth;
                    var age = now.Year - dbo.Year;
                    if (dbo > now.AddYears(-age))
                        age--;
                    CampDischarge.Age = age;

                    if (CampDischarge.Master.Sex == "M")
                    {
                        CampDischarge.Master.Sex = "Male";
                    }
                    else if (CampDischarge.Master.Sex == "F")
                    {
                        CampDischarge.Master.Sex = "FeMale";
                    }

                    CampDischarge.Surgery = Context.Ip_Surgery_Dtl.FirstOrDefault(x => x.UIN == CampDischarge.Admission.UIN && x.Ipa_No == CampDischarge.Admission.Ipa_No && x.SiteId == siteId);

                    CampDischarge.CurrentRoomStatus = Context.Current_Room_Status.FirstOrDefault(x => x.UIN == CampDischarge.Admission.UIN && x.Siteid == siteId);
                    CampDischarge.Master.Taluk = Context.Taluk_Master.Where(x => x.Taluk_Code == CampDischarge.Master.Taluk).Select(x => x.Taluk_Name).FirstOrDefault();
                    CampDischarge.Master.District = Context.District_Master.Where(x => x.District_Code == CampDischarge.Master.District).Select(x => x.District_Name).FirstOrDefault();
                    CampDischarge.Master.State = Context.State_Master.Where(x => x.State_Code == CampDischarge.Master.State).Select(x => x.State_Name).FirstOrDefault();
                    CampDischarge.Master.Country = Context.Country_Master.Where(x => x.Country_Code == CampDischarge.Master.Country).Select(x => x.Country_Name).FirstOrDefault();
                    //CampDischarge.Admission.Eye = Context.EYEMASTER.Where(x => x.EyeCode == CampDischarge.Admission.Eye).Select(x => x.Description).FirstOrDefault();
                    CampDischarge.LocationCode = Context.ICD_Code_Master.Where(x => x.Icd_Code == CampDischarge.Admission.Surgery_Code).Select(x => x.Location_Code).FirstOrDefault();
                    CampDischarge.campMaster = Context.CAMP_MASTER.FirstOrDefault(x => x.CAMP_CODE == CampDischarge.Admission.Camp_Code);
                    CampDischarge.Admission.Surgery_Code = Context.ICD_Code_Master.Where(x => x.Icd_Code == CampDischarge.Admission.Surgery_Code && x.Icd_Type_Code == "D").Select(x => x.Icd_Description).FirstOrDefault();
                    CampDischarge.Admission.Lens_Code = Context.LENS_MASTER.Where(x => x.Lens_Code == CampDischarge.Admission.Lens_Code).Select(x => x.Lens_Type).FirstOrDefault();
                    CampDischarge.FloorCode = Context.Room_Master.Where(x => x.Room_Type == CampDischarge.Admission.Room_Type).Select(x => x.Floor_Code).FirstOrDefault();
                 
                    CampDischarge.campMaster = Context.CAMP_MASTER.FirstOrDefault(x => x.CAMP_CODE == CampDischarge.Admission.Camp_Code);
                    CampDischarge.PatientHistory = Context.PatientHistory.Where(x => x.UIN == CampDischarge.Admission.UIN && x.SiteId == CampDischarge.Admission.SiteId).FirstOrDefault();
                    CampDischarge.MedicalRecorddtl = Context.Medical_Record_Dtl.Where(x => x.IPA_No == CampDischarge.Admission.Ipa_No).FirstOrDefault();

                    CampDischarge.ClinicalDetail = Context.CLINICAL_DETAIL.Where(x => x.UIN == CampDischarge.Admission.UIN && x.Ipa_No == CampDischarge.Admission.Ipa_No && x.SiteId == CampDischarge.Admission.SiteId).FirstOrDefault();



                    CampDischarge.FloorName = Context.Free_Accommodations_Master.Where(x => x.ROOM_NO == CampDischarge.Admission.Room_No).Select(x => x.HALL_NAME).FirstOrDefault();

                    if (CampDischarge.ClinicalDetail==null)
                    {
                        CampDischarge.ClinicalDetail = new ClinicalDetail();
                    }


                    if (CampDischarge.MedicalRecorddtl != null)
                    {

                        CampDischarge.Surgeryname = Context.ICD_Code_Master.Where(x => x.Icd_Code == CampDischarge.MedicalRecorddtl.ICD_Code).Select(x => x.Icd_Description).FirstOrDefault();
                    }




                   


                }


                }

            catch (Exception ex)
            {


                CampDischarge.Message = ex.ToString().Trim();

            }

        

            return CampDischarge;
        }


        public dynamic InsertCampDischarge(Camp_Discharge CampDischarge)
        {



            if (CampDischarge.MedicalRecord.Count()>0)
            {

                var MedicalrecordDtl = new MedicalRecordDtl();
                foreach (var item in CampDischarge.MedicalRecord.ToList())
                {


                    var locationcode = Context.ICD_Code_Master.Where(x => x.Icd_Code == item.ICD_Code).Select(x => x.Location_Code).FirstOrDefault();

                    MedicalrecordDtl.Visit_Date = DateTime.Now.Date;
                    MedicalrecordDtl.MR_NO = CampDischarge.Admission.Mr_No;
                    MedicalrecordDtl.IPA_No = CampDischarge.Admission.Ipa_No;
                    MedicalrecordDtl.UIN = CampDischarge.Admission.UIN;
                    MedicalrecordDtl.SiteId = CampDischarge.Admission.SiteId;
                    MedicalrecordDtl.ICD_Type_Code = "D";
                    MedicalrecordDtl.Additional_Surgery = "";
                    MedicalrecordDtl.Reference = "N";
                    MedicalrecordDtl.Location_Code = locationcode;
                    MedicalrecordDtl.ICD_Code = item.ICD_Code;
                    MedicalrecordDtl.Eye = item.Eye;
                    MedicalrecordDtl.Sysdate = DateTime.Now.Date;
                    MedicalrecordDtl.Doctor_Code = "";
                    MedicalrecordDtl.Additional_Surgery = "";
                    MedicalrecordDtl.Camp_Code = CampDischarge.campMaster.CAMP_CODE;


                    Context.Medical_Record_Dtl.Add(MedicalrecordDtl);
                }

            }


            var clinicaldtl = Context.CLINICAL_DETAIL.Where(X => X.Ipa_No == CampDischarge.Admission.Ipa_No).FirstOrDefault();

            if (clinicaldtl != null)
            {


                var clinicaldetailss = new ClinicalDetail();

                clinicaldetailss.UIN = CampDischarge.Admission.UIN;
                clinicaldetailss.Mr_No = CampDischarge.Admission.Mr_No;
                clinicaldetailss.Ipa_No = CampDischarge.Admission.Ipa_No;
                clinicaldetailss.SiteId = CampDischarge.Admission.SiteId;
                clinicaldetailss.Room = CampDischarge.ClinicalDetail.Room;
                clinicaldetailss.Noofsuture = CampDischarge.ClinicalDetail.Noofsuture;
                clinicaldetailss.Pre_Op = CampDischarge.ClinicalDetail.Pre_Op;
                clinicaldetailss.Pre_Op1 = CampDischarge.ClinicalDetail.Pre_Op1;
                clinicaldetailss.Bp = CampDischarge.ClinicalDetail.Bp;
                clinicaldetailss.Ductre = CampDischarge.ClinicalDetail.Ductre;
                clinicaldetailss.Ductle = CampDischarge.ClinicalDetail.Ductle;
                clinicaldetailss.Tensionre = CampDischarge.ClinicalDetail.Tensionre;                                                                            
                clinicaldetailss.Tensionle = CampDischarge.ClinicalDetail.Tensionle;
                clinicaldetailss.Lenstre = CampDischarge.ClinicalDetail.Lenstre;
                clinicaldetailss.Lenstle = CampDischarge.ClinicalDetail.Lenstle;
                clinicaldetailss.Discharge_Status = "DIS";
                clinicaldetailss.Rev_Date = DateTime.Now.Date;
                clinicaldetailss.Vaatdischarge = CampDischarge.ClinicalDetail.Vaatdischarge;
                clinicaldetailss.Diabet = CampDischarge.ClinicalDetail.Diabet;
                Context.Entry(clinicaldetailss).State = EntityState.Modified;

            }
            else
            {

                //if (CampDischarge.MedicalRecord.Count() > 0)
                //{


                    var clinicaldetails = new ClinicalDetail();

                    clinicaldetails.UIN = CampDischarge.Admission.UIN;
                    clinicaldetails.Mr_No = CampDischarge.Admission.Mr_No;
                    clinicaldetails.Ipa_No = CampDischarge.Admission.Ipa_No;
                    clinicaldetails.SiteId = CampDischarge.Admission.SiteId;
                    clinicaldetails.Room = CampDischarge.ClinicalDetail.Room;
                    clinicaldetails.Noofsuture = CampDischarge.ClinicalDetail.Noofsuture;
                    clinicaldetails.Pre_Op = CampDischarge.ClinicalDetail.Pre_Op;
                    clinicaldetails.Pre_Op1 = CampDischarge.ClinicalDetail.Pre_Op1;
                    clinicaldetails.Bp = CampDischarge.ClinicalDetail.Bp;
                    clinicaldetails.Ductre = CampDischarge.ClinicalDetail.Ductre;
                    clinicaldetails.Ductle = CampDischarge.ClinicalDetail.Ductle;
                    clinicaldetails.Tensionre = CampDischarge.ClinicalDetail.Tensionre;
                    clinicaldetails.Tensionle = CampDischarge.ClinicalDetail.Tensionle;
                    clinicaldetails.Lenstre = CampDischarge.ClinicalDetail.Lenstre;
                    clinicaldetails.Lenstle = CampDischarge.ClinicalDetail.Lenstle;
                    clinicaldetails.Discharge_Status = "DIS";
                    clinicaldetails.Rev_Date = DateTime.Now.Date;
                    clinicaldetails.Vaatdischarge = CampDischarge.ClinicalDetail.Vaatdischarge;
                   clinicaldetails.Diabet = CampDischarge.ClinicalDetail.Diabet;
                //clinicaldetails.Clinical_Detail_Id =01;
                   Context.CLINICAL_DETAIL.Add(clinicaldetails);
                //}
            }

            if(CampDischarge.History.Count()>0 )
            {
                var patienthistory = new PatientHistory();
                foreach (var item in CampDischarge.History.ToList())
                {
                    patienthistory.DurationMonth = item.DurationMonth;
                    patienthistory.DurationYear = item.DurationYear;
                    patienthistory.PatientHistoryDescription = item.PatientHistoryDescription;
                    patienthistory.MR_NO = CampDischarge.Admission.Mr_No;
                    patienthistory.SiteId = CampDischarge.Admission.SiteId;
                    patienthistory.UIN = CampDischarge.Admission.UIN;
                    patienthistory.CreatedUTC = DateTime.Now.Date;
                    Context.PatientHistory.Add(patienthistory);
                }
            }
            



            var ipadmissions = Context.Ip_Admission.Where(x => x.UIN == CampDischarge.Admission.UIN && x.SiteId == CampDischarge.Admission.SiteId).FirstOrDefault();
            var ipano = ipadmissions.Ipa_No;
            if (ipadmissions != null)
            {
                ipadmissions.Discharge_Status = "DIS";
                ipadmissions.Discharge_Date = DateTime.Now;
                Context.Entry(ipadmissions).State = EntityState.Modified;

            }


            var ipaccount = Context.IP_Account.Where(x => x.Uin == CampDischarge.Admission.UIN && x.Siteid == CampDischarge.Admission.SiteId).FirstOrDefault();
            if (ipaccount != null)
            {
                ipaccount.Status = "DIS";
               Context.Entry(ipaccount).State = EntityState.Modified;
            }





            var freeaccomodmaster = _context.Free_Accommodations_Master.Where(x =>  x.ROOM_NO == CampDischarge.Admission.Room_No).FirstOrDefault();//x.ROOMTYPE == CampDischarge.Admission.Room_Type &&
            var isFreeAccomod = false;
            if (freeaccomodmaster == null)
            {
                isFreeAccomod = true;
                freeaccomodmaster = new FreeAccommodationsMaster();
                freeaccomodmaster.ROOMTYPE = CampDischarge.Admission.Room_Type;
                freeaccomodmaster.ROOM_NO = CampDischarge.Admission.Room_No;

            }

            if(freeaccomodmaster.LAST_ALLOCATION ==0)
            {
                freeaccomodmaster.LAST_ALLOCATION = 0;
            }
            else
            {
                freeaccomodmaster.LAST_ALLOCATION -= 1;
            }
            

            if (isFreeAccomod)
                _context.Free_Accommodations_Master.Add(freeaccomodmaster);
            else
                _context.Entry(freeaccomodmaster).State = EntityState.Modified;



            var additionalprocedure = new AdditionalProcedureTrans();

            if (CampDischarge.AdditionalProcedureTrans.Count() > 0)
            {

                foreach (var item in CampDischarge.AdditionalProcedureTrans.ToList())
                {

                    additionalprocedure.UIN = CampDischarge.Admission.UIN;
                    additionalprocedure.IPANO = CampDischarge.Admission.Ipa_No;
                    additionalprocedure.Siteid = CampDischarge.Admission.SiteId;
                    additionalprocedure.Test_Code = item.Test_Code;
                    Context.Additional_Procedure_Trans.Add(additionalprocedure);

                }    


                //    CampDischarge.AdditionalProcedureTrans.All(x =>
                //{
                //    x.UIN = CampDischarge.Master.UIN;
                //    x.IPANO = CampDischarge.Admission.Ipa_No;
                //    x.Siteid = 1;

                //    return true;
                //});
               
            }




            try
            {
                if (Context.SaveChanges() > 0)
                    return new
                    {
                        Success = true,
                      Message = "Patient Camp Discharge saved successfully"
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
