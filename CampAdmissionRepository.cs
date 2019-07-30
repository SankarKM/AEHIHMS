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
    public class CampAdmissionRepository : RepositoryBase<CampAdmission>, ICampAdmissionRepository
    {
        private readonly IHMSContext _context;
        public CampAdmissionRepository(IHMSContext context) : base(context)
        {
            _context = context;
        }

        private string GenerateRunningCtrlNo(string rnControlCode, int? siteId = null)
        {
            var rn = Context.Running_Number_Control.FirstOrDefault(x => x.RnControl_Code == rnControlCode && x.IsActive == true && x.SiteId == siteId);
            if (rn != null)
            {
                if ( rnControlCode != "UIN")
                {
                    rn.Control_Value += 1;
                    Context.Entry(rn).State = EntityState.Modified;
                    //return $"{rn.Control_Value}";
                     return $"{rn.Control_String_Value}{rn.Control_Value}";
                }
                else

                {
                   
                        rn.Control_Value += 1;
                        Context.Entry(rn).State = EntityState.Modified;
                        return $"{rn.Control_Value}";
                         //return $"{rn.Control_String_Value}{rn.Control_Value}";

                    
                }
            }
            
           
                return "";

            
          
               
        }   






public CampAdmission GetCampAdmission(string CampCode, int siteId)
        {
             var result = new CampAdmission();
            result.Master = new PatientRegistrationMaster();
                
            result.History = new List<PatientHistory>();
            try
            {
                result.Site_ID = siteId;
                result.Patientlist = (from IPA in Context.Ip_Admission.Where(u => u.Camp_Code == CampCode && u.SiteId == siteId)
                                      join PRM in Context.Patient_Registration_Master
                                      on new { admmrno = IPA.Mr_No, admuin = IPA.UIN, IPA.SiteId } equals new { admmrno = PRM.MR_NO, admuin = PRM.UIN, PRM.SiteId }
                                      //orderby IPA.Mr_No.ToList()
                                      select new Patientlist
                                      {
                                          UIN = PRM.UIN,
                                          MRNO = PRM.MR_NO,
                                          PatientName = PRM.Patient_Name,
                                          DOB = DateTime.Now.Year - (PRM.Date_Of_Birth.Year),
                                          Gender = PRM.Sex,
                                          NextofKin = PRM.Next_Of_Kin,
                                          PhoneNo = PRM.Phone,
                                      }).ToList();

                var ST = Context.CAMP_MASTER.Where(i => i.CAMP_CODE == CampCode).FirstOrDefault();
                result.C_Place = ST.PLACE;
                result.C_TalukDesc = ST.TALUK;
                result.C_DistrictDesc = ST.DISTRICT;
                result.C_StateDesc = ST.STATE;
                result.Camp_Date = ST.CAMP_DATE;
                result.Camp_Code = ST.CAMP_CODE;
                result.C_Place = ST.PLACE;

                var C = (from IPA in Context.State_Master.Where(u => u.State_Name == ST.STATE)
                         join PRM in Context.Country_Master
                         on IPA.Country_Code equals PRM.Country_Code
                         select new { PRM.Country_Name }).FirstOrDefault();
                result.C_CountryDesc = C.Country_Name;

                //result.stateMaster = Context.State_Master.Where(x => x.State_Name == ST.STATE).FirstOrDefault();
                //result.C_CountryDesc = Context.Country_Master.Where(x => x.Country_Code == result.stateMaster.Country_Code).Select(x => x.Country_Name).FirstOrDefault();
                result.TotalAdmission = result.Patientlist.Count();
                result.TotalAdmissionMale = result.Patientlist.Where(x => x.Gender == "M").Count();
                result.TotalAdmissionFemale = result.Patientlist.Where(x => x.Gender == "F").Count();

                




            }
            catch (Exception ex)
            {
                Console.Write(ex);

            }
            return result;
        }

  







        public dynamic RoomChangeEvent(string roomType, string roomNo)
        {
            var FreeAccommodationsMaster = new FreeAccommodationsMaster();
            FreeAccommodationsMaster = Context.Free_Accommodations_Master.Where(u => u.ROOMTYPE == roomType && u.ROOM_NO == roomNo).FirstOrDefault();
            return FreeAccommodationsMaster;
        }
        public dynamic ToDBCampAdmission(CampAdmission CampAdmission)
         {

            if (CampAdmission.Patient_Name != null || CampAdmission.Patient_Name != "")
            {
                var mrNo = "";
                var uin = "";
                var aadNo = "";
                var ipano = "";
                var ipno = "";
                var invno = "";
                var patientClass = "";

                var statusDesc = Context.State_Master.Where(x => x.State_Name == CampAdmission.C_StateDesc).Select(x => x.State_Code).FirstOrDefault();
                CampAdmission.StateCode = statusDesc;


                var CountryDesc = Context.Country_Master.Where(x => x.Country_Name == CampAdmission.C_CountryDesc).Select(x => x.Country_Code).FirstOrDefault();
                CampAdmission.ContryCode = CountryDesc;

                var Districtcodes = Context.District_Master.Where(x => x.District_Name == CampAdmission.C_DistrictDesc).Select(x => x.District_Code).FirstOrDefault();
                CampAdmission.DistrictCode = Districtcodes;

                var talukcodes = Context.Taluk_Master.Where(x => x.Taluk_Name == CampAdmission.C_TalukDesc).Select(x => x.Taluk_Code).FirstOrDefault();
                CampAdmission.Taluk_Code = talukcodes;

            

                if (CampAdmission.IsCOTChecked == "Y")
                {
                         CampAdmission.bedCharges = Context.StateBedCharges.Where(o => o.StateID == CampAdmission.StateCode).Select(x => x.COTCharges).FirstOrDefault();
                }
                else if (CampAdmission.IsMATChecked == "Y")
                { 
                        CampAdmission.bedCharges = Context.StateBedCharges.Where(o => o.StateID == CampAdmission.StateCode).Select(x => x.MATCharges).FirstOrDefault();
                }


             




                mrNo = GenerateRunningCtrlNo("MR_NO_CAMP", CampAdmission.Site_ID);
  

                uin = GenerateRunningCtrlNo("UIN", CampAdmission.Site_ID);
                CampAdmission.mr_no = mrNo;
                CampAdmission.uin = uin;

                if (CampAdmission.IDProof_Desc == "Aadhar Card")
                    aadNo = CampAdmission.IDProof_Code;

                //if (CampAdmission.bedCharges > 0)
                //{
                //    ipano = GenerateRunningCtrlNo("CIPA_NO", CampAdmission.Site_ID);
                //    patientClass = "SITE03";
                //}
                //else if (CampAdmission.bedCharges == 0)
                //{
                   ipano = GenerateRunningCtrlNo("FIPA_NO", CampAdmission.Site_ID);
                    patientClass = "SITE02";
               // }

                ipno = GenerateRunningCtrlNo("CAMP_IP_NO ", CampAdmission.Site_ID);
                CampAdmission.ipno = ipno;
                invno = GenerateRunningCtrlNo("INVOICE_NO", CampAdmission.Site_ID);

                var icpl = new Ip_Camp_Place();
                icpl.mr_no = mrNo;
                icpl.ipa_no = ipano;
                icpl.Admission_place = CampAdmission.Admission_Place;
                icpl.Sysdate = DateTime.Now;
                Context.ip_camp_place.Add(icpl);

                var ipadmissions = Context.Free_Accommodations_Master.Where(x => x.ROOM_NO == CampAdmission.Room_No).FirstOrDefault();
                if (ipadmissions != null)
                {
                    ipadmissions.LAST_ALLOCATION -= 1;
                    Context.Entry(ipadmissions).State = EntityState.Modified;
                }

                var isOPeXists = Context.Op_Visit_Datewise.Where(x => x.Date == DateTime.Now.Date && x.Patient_Class == patientClass).Count();
                var opvd = Context.Op_Visit_Datewise.Where(x => x.Patient_Class == patientClass && x.Date == DateTime.Now.Date).FirstOrDefault();
                var opvdw = new OpVisitDatewise();
                opvdw.Patient_Class = patientClass;
                opvdw.Date = DateTime.Now.Date;
                opvdw.New = 0;
                opvdw.Old = 0;
              
                opvdw.Week_Day = (int)DateTime.Now.DayOfWeek;
                if (isOPeXists > 0)
                {
                    //Context.Op_Visit_Datewise.Update(opvdw);
                    if (opvd != null)
                    {
                        opvd.Admission += 1;
                        Context.Entry(opvd).State = EntityState.Modified;
                    }
                }
                //else if (isOPeXists == 0)
                //{
                //    Context.Op_Visit_Datewise.Add(opvdw);
                //}


                
             


                var PRM = new PatientRegistrationMaster();
                PRM.MR_NO = mrNo;
                PRM.IP_NO = ipno;
                PRM.Patient_Class = patientClass;
                PRM.Patient_Name = CampAdmission.Patient_Name;
                PRM.Next_Of_Kin = $"{CampAdmission.Next_Of_Kin_Prefix},{CampAdmission.Next_Of_Kin_Suffix}";
                PRM.Date_Of_Birth = CampAdmission.Patient_DOB;
                PRM.Sex = CampAdmission.Gender;
                PRM.Door = CampAdmission.Door;
                PRM.Street_Locality = CampAdmission.Street;
                PRM.Pincode = CampAdmission.Pincode;
                PRM.Town_City = CampAdmission.City;
                PRM.Taluk = CampAdmission.Taluk_Code;
                PRM.District = CampAdmission.DistrictCode;
                PRM.State = CampAdmission.StateCode;
                PRM.Country = CampAdmission.ContryCode;
                PRM.Phone = CampAdmission.Phone_No;
                PRM.Registered_Date = DateTime.Now;
                PRM.Last_Visit_Date = DateTime.Now;
                PRM.AadhaarNo = CampAdmission.Master.AadhaarNo;
                PRM.Base_Unit = "LC15";
                PRM.Last_Unit_Visited = "LC15";
                PRM.Visit_Number = 1;
                PRM.Sysdate = DateTime.Now;
                PRM.UIN = uin;
                //PRM.AadhaarNo = aadNo;
                PRM.SiteId = CampAdmission.Site_ID;
                Context.Patient_Registration_Master.Add(PRM);

                var PRD = new PatientRegistrationDetail();
                PRD.MR_NO = mrNo;
                PRD.Allocation_Code = "LC15";
                PRD.Visit_Date = DateTime.Now;
                PRD.Visit_Number = 1;
                PRD.Category_Code = "CATC002";
                PRD.Type_Code = "PTY001";
                PRD.Instance_Code = "INSC001";
                PRD.Speciality_Code = "SPC001";
                PRD.Sysdate = DateTime.Now;
                PRD.Allocation_Code = "LC007";
                PRD.Camp_Code = CampAdmission.Camp_Code;
                PRD.UIN = uin;
                PRD.SiteId = CampAdmission.Site_ID;
                Context.Patient_Registration_Detail.Add(PRD);


                var LM = new MRLocationMaster();
                LM.MR_NO = mrNo;
                LM.Location_Code = "LC15";
                LM.Patient_Name = CampAdmission.Patient_Name;
                LM.Town_City = CampAdmission.City;
                LM.UIN = uin;
                LM.SiteId = 1;
                Context.MR_Location_Master.Add(LM);


                var PS = new PatientStatus();
                PS.Visit_Date = DateTime.Now;
                PS.MR_NO = mrNo;
                PS.Assign_Doctor = "N";
                PS.UIN = uin;
                //PS.PurposeId = Convert.ToInt32( CampAdmission.Camp_Code);
                PS.PurposeId = 42;   
                PS.SiteId = CampAdmission.Site_ID;
                Context.Patient_Status.Add(PS);

                if (CampAdmission.History.Count() > 0)
                {
                    foreach (var item in CampAdmission.History.ToList())
                    {
                        var PH = new PatientHistory();
                        PH.UIN = uin;
                        PH.MR_NO = mrNo;
                        PH.PatientHistoryDescription = item.PatientHistoryDescription;
                        PH.DurationMonth = item.DurationMonth;
                        PH.DurationYear = item.DurationYear;
                        PH.CreatedUTC = DateTime.Now;
                        PH.CreatedBy = Convert.ToInt32(CampAdmission.Operator_ID);
                        PH.SiteId = CampAdmission.Site_ID;
                        Context.PatientHistory.AddRange(PH);
                    }
                }

                var IDPD = new IDProofDtl();
                IDPD.UIN = uin;
                IDPD.MR_NO = mrNo;
                IDPD.PROOF_TYPE = CampAdmission.IDProof_Code;
                IDPD.PROOF_NO = CampAdmission.IDProof_Desc;
                IDPD.CASTE_CODE = Convert.ToString(CampAdmission.Caste_Code);
                Context.IDProof_Dtl.Add(IDPD);

                var IPSD = new IpSurgeryDtl();
                IPSD.Ipa_No = ipano;
                IPSD.Surgery_Code = CampAdmission.Surgery_Code;
                IPSD.Surgery_Type_Code = "T";
                IPSD.Anaesthesia = "AN002";
                IPSD.Doctor_Code = "401";
                IPSD.Eye = CampAdmission.Eye_Code;
                IPSD.Surgery_Date = DateTime.Now.Date;
                IPSD.Surgery_Approval = "Y";
                IPSD.Surgery_Done = "N";
                IPSD.SiteId = CampAdmission.Site_ID;
                IPSD.UIN = uin;
                Context.Ip_Surgery_Dtl.Add(IPSD);

                var IPAcc = new IPAccount();
                IPAcc.Ipa_No = ipano;
                IPAcc.Mr_No = mrNo;
                IPAcc.Status = "ADM";
                IPAcc.System = DateTime.Now;
                IPAcc.Ip_No = ipno;
                IPAcc.Siteid = CampAdmission.Site_ID;
                IPAcc.Uin = uin;
                PRM.IPAccountIC = new List<IPAccount>() { IPAcc };

                var IPA = new IPAdmission();
                IPA.Ipa_No = ipano;
                IPA.Ip_No = ipno;
                IPA.Mr_No = mrNo;
                IPA.Surgery_Code = CampAdmission.Surgery_Code;
                IPA.Surgery_Type_Code = "T";
                IPA.Admission_Date = DateTime.Now;
                IPA.Room_Type = "FREETYPE7";
                IPA.Room_No = CampAdmission.Room_No;
                IPA.Expected_Discharge_Date =DateTime.Now.AddDays(2);
                IPA.Doctor_Code = "401";
                IPA.Category_Code = "CATC002";
                IPA.Anesthesia = "AN002";
                IPA.Discharge_Status = "ADM";
                IPA.Eye = CampAdmission.Eye_Code;
                //IPA.Ipa_No = ipno;
                IPA.Room_Type2 = null;
                IPA.Lens_Code = CampAdmission.Lens_Code;
                IPA.Operator_Code = CampAdmission.Operator_ID;
                IPA.Camp_Code = CampAdmission.Camp_Code;
                IPA.UIN = uin;
                IPA.SiteId = CampAdmission.Site_ID;

                PRM.IPAdmissionIC = new List<IPAdmission>() { IPA };


                if (CampAdmission.bedCharges > 0)
                {
                    var IM = new InvoiceMaster();
                    IM.Invoice_No = invno;
                    IM.Invoice_Date = DateTime.Now;
                    IM.Invoice_Value = float.Parse(CampAdmission.bedCharges.ToString());
                    IM.Invoice_Received_Value = float.Parse(CampAdmission.bedCharges.ToString());
                    IM.Module_Code = "MOD4";
                    IM.SiteId = CampAdmission.Site_ID;
                    IM.UIN = uin;
                    IM.MR_NO = mrNo;
                    PRM.Invoice_Master = new List<InvoiceMaster>() { IM };

                    var accDtl = new IpAccountDtl();
                    if (CampAdmission.bedCharges > 0)
                    {
                        accDtl.Ipa_Sl_No = Convert.ToInt32(GenerateRunningCtrlNo("IPA_SL_NO", CampAdmission.Site_ID));
                        accDtl.Ipa_No = ipano;
                        accDtl.Cash_Flow_Code = "501";
                        accDtl.Tinvoice_No = invno;
                        accDtl.Sysdate = DateTime.Now;
                        accDtl.Date = DateTime.Now.Date;
                        accDtl.Cost_Value = Convert.ToDecimal(CampAdmission.bedCharges);
                        accDtl.UIN = uin;
                        accDtl.SiteId = CampAdmission.Site_ID;
                        Context.Ip_Account_Dtl.Add(accDtl);
                    }

                    var cashPaid = new CashPaid();
                    if (CampAdmission.bedCharges > 0)
                    {

                        cashPaid.Operator_Code = CampAdmission.Operator_ID;
                        cashPaid.Module_Code = "MOD4";
                        cashPaid.MR_NO = mrNo;
                        cashPaid.IPA_NO = ipano;
                        cashPaid.Receipt_NO = GenerateRunningCtrlNo("FRES_RECEIPT_NO", CampAdmission.Site_ID);
                        cashPaid.Fees_Paid = Convert.ToDouble(CampAdmission.bedCharges);
                        cashPaid.Date = DateTime.Now.Date;
                        cashPaid.Transaction_Code = "RECPT";
                        cashPaid.Category_Code = "CATC001";
                        cashPaid.Sysdate = DateTime.Now;
                        cashPaid.OP_IP_Flag = "IP";
                        cashPaid.Paymode_Code = "PC001";
                        cashPaid.Outstanding = "N";
                        cashPaid.Invoice_No = invno;
                        cashPaid.UIN = uin;
                        cashPaid.SiteId = CampAdmission.Site_ID;
                        Context.Cash_Paid.Add(cashPaid);
                    }
                }
            }
            try     
            {
                if (Context.SaveChanges() > 0)
                    return new
                    {
                        Success = true,
                        name = CampAdmission.Patient_Name,
                        eyes = CampAdmission.Eye_Code,
                        taluks = CampAdmission.C_TalukDesc,
                        DistrictDesc = CampAdmission.C_DistrictDesc,
                        StateDesc = CampAdmission.C_StateDesc,
                        CountryDesc = CampAdmission.C_CountryDesc,
                        MRNO = CampAdmission.mr_no,
                        UIN = CampAdmission.uin,
                        Siteid=CampAdmission.Site_ID,
                       campcodes = CampAdmission.Camp_Code,
                        campnames = Context.CAMP_MASTER.Where(x => x.CAMP_CODE == CampAdmission.Camp_Code).Select(x => x.CAMP_NAME).FirstOrDefault(),
                        ages = CampAdmission.Age,
                        sex = CampAdmission.Gender,
                        kins = CampAdmission.Next_Of_Kin_Suffix,
                        room = CampAdmission.Room_No,
                        town = CampAdmission.City,
                        numer = CampAdmission.Phone_No,
                        ipnos = CampAdmission.ipno,

                        VacantBed = Context.Free_Accommodations_Master.Where(x => x.ROOM_NO == CampAdmission.Room_No).Select(x => x.LAST_ALLOCATION).FirstOrDefault(),
                        Endrange = Context.Free_Accommodations_Master.Where(x => x.ROOM_NO == CampAdmission.Room_No).Select(x => x.END_RANGE).FirstOrDefault(),


                    };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return new
            {
                Success = false,
                message = "Details not saved!"
            };
        }
        //FOR CAMP ADMISSION DETAILS END


        public CampAdmission GetpopDetails(string MR_NO, int? siteId = null)
        {
            var campadmission = new CampAdmission();
            campadmission.IPAdmission = new IPAdmission();
            campadmission.Master = new PatientRegistrationMaster();

            campadmission.Master = Context.Patient_Registration_Master.FirstOrDefault(x => x.MR_NO == MR_NO && x.SiteId == siteId);

            try
            {
                campadmission.PatientAdmissionDetails = Context.Ip_Admission.Where(x => x.Mr_No == MR_NO && x.SiteId == siteId).Select(x => new PatientAdmissionDetails { IPANo = x.Ipa_No, Admission_Date = x.Admission_Date }).ToList();
                campadmission.Master.Patient_Name = Context.Patient_Registration_Master.Where(x => x.Patient_Name == campadmission.Master.Patient_Name).Select(x => x.Patient_Name).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.Write(ex);

            }
            return campadmission;

        }


        public dynamic GetipaDetails(string ipa_no, int? siteId = null)
        {


            var campadmission = new CampAdmission();
            campadmission.Master = new PatientRegistrationMaster();

            campadmission.IPAdmission = Context.Ip_Admission.FirstOrDefault(x => x.Ipa_No == ipa_no && x.SiteId == siteId);

            if (campadmission != null)
            {
                campadmission.Room_No = Context.Ip_Admission.Where(x => x.Room_No == campadmission.IPAdmission.Room_No).Select(x => x.Room_No).FirstOrDefault();
                campadmission.Eye_Code = Context.Ip_Admission.Where(x => x.Eye == campadmission.IPAdmission.Eye).Select(x => x.Eye).FirstOrDefault();
                campadmission.mr_no = Context.Ip_Admission.Where(x => x.Mr_No == campadmission.IPAdmission.Mr_No).Select(x => x.Mr_No).FirstOrDefault();
                campadmission.uin = Context.Ip_Admission.Where(x => x.UIN == campadmission.IPAdmission.UIN).Select(x => x.UIN).FirstOrDefault();
                campadmission.C_TalukDesc = Context.CAMP_MASTER.Where(x => x.CAMP_CODE == campadmission.IPAdmission.Camp_Code).Select(x => x.TALUK).FirstOrDefault();
                campadmission.C_StateDesc = Context.CAMP_MASTER.Where(x => x.CAMP_CODE == campadmission.IPAdmission.Camp_Code).Select(x => x.STATE).FirstOrDefault();
                campadmission.C_DistrictDesc = Context.CAMP_MASTER.Where(x => x.CAMP_CODE == campadmission.IPAdmission.Camp_Code).Select(x => x.DISTRICT).FirstOrDefault();
                //campadmission.C_CountryDesc = Context.CAMP_MASTER.Where(x => x.CAMP_CODE == campadmission.IPAdmission.Camp_Code).Select(x => x.).FirstOrDefault();
                campadmission.Camp_Code = Context.CAMP_MASTER.Where(x => x.CAMP_CODE == campadmission.IPAdmission.Camp_Code).Select(x => x.CAMP_CODE).FirstOrDefault();
                campadmission.Campname = Context.CAMP_MASTER.Where(x => x.CAMP_CODE == campadmission.IPAdmission.Camp_Code).Select(x => x.CAMP_NAME).FirstOrDefault();
                campadmission.Next_Of_Kin_Suffix = Context.Patient_Registration_Master.Where(x => x.UIN == campadmission.IPAdmission.UIN).Select(x => x.Next_Of_Kin).FirstOrDefault();
                campadmission.City = Context.Patient_Registration_Master.Where(x => x.UIN == campadmission.IPAdmission.UIN).Select(x => x.Town_City).FirstOrDefault();
                campadmission.Patient_Name = Context.Patient_Registration_Master.Where(x => x.UIN == campadmission.IPAdmission.UIN).Select(x => x.Patient_Name).FirstOrDefault();
                campadmission.Gender = Context.Patient_Registration_Master.Where(x => x.UIN == campadmission.IPAdmission.UIN).Select(x => x.Sex).FirstOrDefault();
                campadmission.Master = Context.Patient_Registration_Master.Where(x => x.UIN == campadmission.IPAdmission.UIN && x.SiteId == campadmission.IPAdmission.SiteId).FirstOrDefault();
                campadmission.Phone_No = Context.Patient_Registration_Master.Where(x => x.UIN == campadmission.IPAdmission.UIN).Select(x => x.Phone).FirstOrDefault();
                campadmission.ipno = Context.Patient_Registration_Master.Where(x => x.UIN == campadmission.IPAdmission.UIN).Select(x => x.IP_NO).FirstOrDefault();

                var now = DateTime.Now;
                var dbo = campadmission.Master.Date_Of_Birth;
                var age = now.Year - dbo.Year;
                if (dbo > now.AddYears(-age))
                    age--;

                campadmission.Age = age;


            }
            return new
            {

                eyes = campadmission.Eye_Code,
                names = campadmission.Patient_Name,
                talukss = campadmission.C_TalukDesc,
                state = campadmission.C_StateDesc,
                dists = campadmission.C_DistrictDesc,
                mrnos = campadmission.mr_no,
                uins = campadmission.uin,
                ccodess = campadmission.Camp_Code,
                ccnames = campadmission.Campname,
                agess = campadmission.Age,
                sexs = campadmission.Gender,
                kinnss = campadmission.Next_Of_Kin_Suffix,
                towns = campadmission.City,
                roooms = campadmission.Room_No,
                number = campadmission.Phone_No,
                ip = campadmission.ipno,

            };
        }





    }
}
