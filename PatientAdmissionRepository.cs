using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Model.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IHMS.Data.Repository.Implementation
{
    public class PatientAdmissionRepository : RepositoryBase<Patient_Admission>, IPatientAdmissionRepository
    {
        private readonly IHMSContext _context;
        private readonly CommonRepository _commonRepository;
        public PatientAdmissionRepository(IHMSContext context) : base(context)
        {
            _context = context;
            _commonRepository = new CommonRepository(context);
        }

        public int CheckExistingUIN(string UIN, int? siteId = null)
        {
            int iResult = 0;
            try
            {
                bool IsExist = Context.Ip_Admission.Any(x => x.UIN == UIN && x.SiteId == siteId && x.Discharge_Status == "ADM");
                if (IsExist == true)
                {
                    iResult = 1;
                }
                else
                {
                    iResult = 0;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return iResult;
        }

        public dynamic GetSurgeryDetails(string SurgeryCode)
        {
            //string ClinicCode = string.Empty;
            //int NoofDAys = 0;
            return new
            {
                ClinicCode = Context.ICD_Code_Master.Where(x => x.Icd_Code == SurgeryCode).Select(x => x.Icd_Code_Speciality_Group_Code).FirstOrDefault(),
                NoofDAys = Context.ICD_Code_Master.Where(x => x.Icd_Code == SurgeryCode).Select(x => x.No_Of_Days).FirstOrDefault(),
            };
        }

        public SurgeryCostDetails GetChargeBreakUp(string SurgeryCode, string RoomType)
        {
            var SurgeryCostDetail = new SurgeryCostDetails();
            SurgeryCostDetail = Context.Surgery_Cost_Detail.FirstOrDefault(x => x.Surgery_Code == SurgeryCode && x.Room_Type == RoomType);
            return SurgeryCostDetail;
        }

        public dynamic GetRoomCharge(string RoomType)
        {
            var FloorCd = Context.Room_Master.Where(x => x.Room_Type == RoomType).Select(x => x.Floor_Code).FirstOrDefault();
            return new
            {
                RoomCost = Context.Room_Master.Where(x => x.Room_Type == RoomType).Select(x => x.Room_Cost).FirstOrDefault(),
                FloorName = Context.Floor_Master.Where(x => x.Floor_Code == FloorCd).Select(x => x.Floor_Description).FirstOrDefault()
            };
        }


        public IEnumerable<Dropdown> Getsurgeryname(string surgerytype)
        {

            if (surgerytype == "S")
            {

                return Context.ICD_Code_Master.Where(x => x.Active == "Y" && x.Icd__Code_Group_Code != "T" && x.Icd_Type_Code == "T").Select(x => new Dropdown { Text = x.Icd_Description, Value = x.Icd_Code }).OrderBy(x => x.Text).ToList();
            }
            else
            {

                return Context.ICD_Code_Master.Where(x => x.Active == "Y" && x.Icd__Code_Group_Code == "T" && x.Icd_Type_Code == "T").Select(x => new Dropdown { Text = x.Icd_Description, Value = x.Icd_Code }).OrderBy(x => x.Text).ToList();
            }

            //var Suegeryname = Context.ICD_Code_Master.Where(x => x.Active=="Y"  && x.Icd_Type_Code=="T" ).Select(x => x.Icd_Code).Distinct().ToList();

        }




        public Patient_Admission GetPatientDetails(string UIN, string status, int? siteId = null)
        {
            var patientAdmision = new Patient_Admission();
            patientAdmision.CorporatePreauth = new CorporatePreauth();
            patientAdmision.IPAdmission = new IPAdmission();
            patientAdmision.CashPaid = new List<CashPaid>();
            patientAdmision.PaymentDetail = new PaymentDetail();
            patientAdmision.IPClinic = new IPClinic();
            //patientAdmision.Corporate = new CorporateCases();
            patientAdmision.Invoice = new InvoiceMaster();
            patientAdmision.Denomination = new List<DenominationDetails>();
            patientAdmision.AdditionalProcedureTrans = new List<AdditionalProcedureTrans>();
            patientAdmision.CurrentRoomStatus = new CurrentRoomStatus();

            patientAdmision.Master = Context.Patient_Registration_Master.Where(x => x.UIN == UIN && x.SiteId == siteId).OrderByDescending(x => x.Last_Visit_Date).FirstOrDefault();

            if (patientAdmision.Master != null)
            {
                patientAdmision.Master = Context.Patient_Registration_Master.Where(x => x.UIN == UIN && x.SiteId == siteId).OrderByDescending(x => x.Last_Visit_Date).FirstOrDefault();
                patientAdmision.Master.Taluk = Context.Taluk_Master.Where(x => x.Taluk_Code == patientAdmision.Master.Taluk).Select(x => x.Taluk_Name).FirstOrDefault();
                patientAdmision.Master.District = Context.District_Master.Where(x => x.District_Code == patientAdmision.Master.District).Select(x => x.District_Name).FirstOrDefault();
                patientAdmision.Master.State = Context.State_Master.Where(x => x.State_Code == patientAdmision.Master.State).Select(x => x.State_Name).FirstOrDefault();


                var now = DateTime.Now;
                var dbo = patientAdmision.Master.Date_Of_Birth;
                var age = now.Year - dbo.Year;
                if (dbo > now.AddYears(-age))

                    age--;
                patientAdmision.Age = age;




                if (patientAdmision.Master.Sex == "M")
                {
                    patientAdmision.Master.Sex = "Male";
                }
                else if (patientAdmision.Master.Sex == "F")
                {
                    patientAdmision.Master.Sex = "FeMale";
                }

                patientAdmision.CorporatePreauth = Context.Corporate_Preauth.Where(x => x.UIN == UIN && x.SiteId == siteId).OrderByDescending(x => x.Sysdate).FirstOrDefault();//.OrderByDescending(x => x.sys).;

                if (patientAdmision.CorporatePreauth != null)

                {

                    patientAdmision.CorporatePreauth = Context.Corporate_Preauth.FirstOrDefault(x => x.UIN == UIN && x.SiteId == siteId);
                    //patientAdmision.Master = Context.Patient_Registration_Master.FirstOrDefault(x => x.UIN == patientAdmision.CorporatePreauth.UIN);
                    patientAdmision.CorporateMaster = Context.Corporate_Master.FirstOrDefault(x => x.Corporate_Code == patientAdmision.CorporatePreauth.Corporate);
                    patientAdmision.ICDCodeMaster = Context.ICD_Code_Master.FirstOrDefault(x => x.Icd_Code == patientAdmision.CorporatePreauth.Surgery);
                    patientAdmision.Description = Context.Preauth_Status.Where(x => x.Code == patientAdmision.CorporatePreauth.Status).Select(x => x.Description).FirstOrDefault();



                }
                else
                {

                    //{
                    //    Success = false,
                    //    Message = "IP Admission IPA_NO or IP_NO is not available in Running Control Table"
                    //};
                }
                try
                {
                    if (status == "1")
                    {

                        patientAdmision.Status = status;

                        patientAdmision.PatientCounselNew = Context.Patient_Counsel_New.Where(x => x.Uin == UIN && x.Siteid == siteId).OrderByDescending(x => x.Counsel_Date).FirstOrDefault();

                        if (patientAdmision.PatientCounselNew != null)
                        {

                            var noww = DateTime.Now.Date;
                            var cnsl = patientAdmision.PatientCounselNew.Counsel_Date;
                            var ans = noww.Subtract(cnsl);
                            var res = ans.TotalDays;
                            patientAdmision.Couseldaysnormal = res;

                            if (res < 30)

                            {

                                patientAdmision.SurgeryCostDetail = Context.Surgery_Cost_Detail.FirstOrDefault(x => x.Surgery_Code == patientAdmision.PatientCounselNew.Surgery_Code);
                                patientAdmision.Icd_Spec_Group_Code = Context.ICD_Code_Master.Where(x => x.Icd_Code == patientAdmision.PatientCounselNew.Surgery_Code).Select(x => x.Icd_Code_Speciality_Group_Code).FirstOrDefault();
                                patientAdmision.RoomCost = Context.Room_Master.Where(x => x.Room_Type == patientAdmision.PatientCounselNew.Room_Type).Select(x => x.Room_Cost).FirstOrDefault();
                                patientAdmision.FloorCode = Context.Room_Master.Where(x => x.Room_Type == patientAdmision.PatientCounselNew.Room_Type).Select(x => x.Floor_Code).FirstOrDefault();
                                patientAdmision.FloorName = Context.Floor_Master.Where(x => x.Floor_Code == patientAdmision.FloorCode).Select(x => x.Floor_Description).FirstOrDefault();
                                patientAdmision.AditionalProcedure = Context.TEST_REPOSITORY.Where(x => x.Test_Code == patientAdmision.PatientCounselNew.Clinicprocedure).Select(x => x.Test_Name).FirstOrDefault();
                                patientAdmision.NoofDays = Context.ICD_Code_Master.Where(x => x.Icd_Code == patientAdmision.PatientCounselNew.Surgery_Code).Select(x => x.No_Of_Days).FirstOrDefault();
                                patientAdmision.Ip_Surgery_Dtl = Context.Ip_Surgery_Dtl.Where(x => x.UIN == patientAdmision.PatientCounselNew.Uin && x.SiteId == patientAdmision.PatientCounselNew.Siteid).FirstOrDefault();


                                //var NormalAdditionalProcedure = string.Join(", ", from item in Context.TEST_REPOSITORY
                                //                                                  join ptn in Context.Patient_Counsel_New on item.Test_Code equals ptn.Clinicprocedure
                                //                                                  where ptn.Uin == UIN

                                //                                                 select item.Test_Name);




                                patientAdmision.IPANo = GenerateRunningCtrlNo("IPA_NO", siteId);
                                patientAdmision.AdmissionType = "1";
                            }
                        }

                        else
                        {
                            //patientAdmision.Couseldaysnormal = res;

                        }
                    }


                    if (status == "2")

                    {
                        patientAdmision.Status = status;
                        // patientAdmision.Master = Context.Patient_Registration_Master.Where(x => x.UIN == UIN && x.SiteId== siteId).FirstOrDefault();
                        //patientAdmision.Master.Taluk = Context.Taluk_Master.Where(x => x.Taluk_Code == patientAdmision.Master.Taluk).Select(x => x.Taluk_Name).FirstOrDefault();
                        //patientAdmision.Master.District = Context.District_Master.Where(x => x.District_Code == patientAdmision.Master.District).Select(x => x.District_Name).FirstOrDefault();
                        //patientAdmision.Master.State = Context.State_Master.Where(x => x.State_Code == patientAdmision.Master.State).Select(x => x.State_Name).FirstOrDefault();

                        //var now = DateTime.Now;
                        //var dbo = patientAdmision.Master.Date_Of_Birth;
                        //var age = now.Year - dbo.Year;
                        //if (dbo > now.AddDays(-age))

                        //    age--;
                        //patientAdmision.Age = age;




                        //if (patientAdmision.Master.Sex == "M")
                        //{
                        //    patientAdmision.Master.Sex = "Male";
                        //}
                        //else if (patientAdmision.Master.Sex == "F")
                        //{
                        //    patientAdmision.Master.Sex = "FeMale";
                        //}

                        patientAdmision.IPANo = GenerateRunningCtrlNo("IPA_NO", siteId);
                        patientAdmision.AdmissionType = "1";


                    }

                }


                catch (Exception ex)
                {
                    Console.Write(ex);
                }

            }
            return patientAdmision;
        }

        private decimal GetReviewPatientFees(string specialityCode, string mrNo)
        {
            var lastVisit = Context.Patient_Registration_Master.LastOrDefault(x => x.MR_NO == mrNo);
            var regNorms = Context.Registration_Norms.FirstOrDefault(x => x.Speciality_Code == specialityCode);
            if (lastVisit.Visit_Number >= regNorms.Max_Allowed_Visits_Aregistration)
                return regNorms.Old_Pay_Cost_Value;
            else
            {
                int dateInterval = (DateTime.Now - lastVisit.Sysdate).Days;
                if (dateInterval > regNorms.Max_Inactive_days)
                    return regNorms.Old_Pay_Cost_Value;
                else
                    return 0;
            }
        }

        public dynamic InsertPatientAdmission(Patient_Admission patientAdmission)
        {




            //if (patientAdmission.Master.Patient_Class=="SITE01")
            //{
            //    if(patientAdmission.PaymentDetail.Amount >0)
            //    { 
            //    //foreach (var item in patientAdmission.CashPaid)
            //   // {
            //        patientAdmission.IPAdmission.Receipt_No = GenerateRunningCtrlNo("RES_RECEIPT_NO", 1);
            //        //if (item.Receipt_NO == null || item.Receipt_NO == "")
            //        //{
            //        //    return new
            //        //    {
            //        //        Success = false,
            //        //        Message = "CashPaid OP_RECEIPT_NO is not available in Running Control Table"
            //        //    };
            //        //}
            //    //}

            //    }


            //}
            //else
            //{

            //    if (patientAdmission.PaymentDetail.Amount > 0)
            //    {
            //        //foreach (var item in patientAdmission.CashPaid)
            //        //{
            //           patientAdmission.IPAdmission.Receipt_No = GenerateRunningCtrlNo("FRES_RECEIPT_NO", 1);
            //            //if (item.Receipt_NO == null || item.Receipt_NO == "")
            //            //{
            //            //    return new
            //            //    {
            //            //        Success = false,
            //            //        Message = "CashPaid OP_RECEIPT_NO is not available in Running Control Table"
            //            //    };
            //            //}
            //        //}
            //    }

            //}


            var IPAdmission = new IPAdmission();
            //IPAdmission.Discharge_Date ="00-00-0000";
            if (patientAdmission.IPClinic.Adm_Status == "2")

            {






                IPAdmission = patientAdmission.IPAdmission;
                IPAdmission.Admission_Date = DateTime.Now;
                IPAdmission.Anesthesia = patientAdmission.IPAdmission.Anesthesia;
                IPAdmission.Expected_Discharge_Date = patientAdmission.IPAdmission.Expected_Discharge_Date;
                IPAdmission.Receipt_No = patientAdmission.CashPaid.Select(x => x.Receipt_NO).FirstOrDefault();//patientAdmission.IPAdmission.Receipt_No;//patientAdmission.CashPaid.Select(x => x.Receipt_NO).FirstOrDefault();

                if (patientAdmission.Master.Patient_Class == "SITE02")
                {

                    IPAdmission.Ipa_No = GenerateRunningCtrlNo("FIPA_NO", 1).Trim();
                }
                else
                {

                    IPAdmission.Ipa_No = GenerateRunningCtrlNo("IPA_NO", 1).Trim();
                }

                //IPAdmission.Ip_No = GenerateRunningCtrlNo("IP_NO", 1).Trim();
                //if (IPAdmission.Ipa_No == null || IPAdmission.Ipa_No == "" && IPAdmission.Ip_No == null || IPAdmission.Ip_No == "")
                //{
                //    return new
                //    {
                //        Success = false,
                //        Message = "IP Admission IPA_NO or IP_NO is not available in Running Control Table"
                //    };
                //}

                if (patientAdmission.Master.IP_NO == null)
                {
                    IPAdmission.Ip_No = GenerateRunningCtrlNo("IP_NO", 1);
                }
                else
                {
                    IPAdmission.Ip_No = patientAdmission.Master.IP_NO;
                }


                //Context.Ip_Admission.Add(IPAdmission);

                if (patientAdmission.AdditionalProcedureTrans.Count() > 0)
                {
                    patientAdmission.AdditionalProcedureTrans.All(x =>
                    {
                        x.UIN = patientAdmission.Master.UIN;
                        x.IPANO = patientAdmission.IPAdmission.Ipa_No;
                        x.Siteid = 1;

                        return true;
                    });
                    Context.Additional_Procedure_Trans.AddRange(patientAdmission.AdditionalProcedureTrans);
                }
            }
            else
            {


                IPAdmission = patientAdmission.IPAdmission;
                IPAdmission.Counsel_Case_No = patientAdmission.PatientCounselNew.Counsel_Case_No;

                if (patientAdmission.Master.Patient_Class == "SITE02")
                {

                    IPAdmission.Ipa_No = GenerateRunningCtrlNo("FIPA_NO", 1).Trim();
                }
                else
                {

                    IPAdmission.Ipa_No = GenerateRunningCtrlNo("IPA_NO", 1).Trim();
                }



                IPAdmission.Receipt_No = patientAdmission.CashPaid.Select(x => x.Receipt_NO).FirstOrDefault();//patientAdmission.IPAdmission.Receipt_No; //patientAdmission.CashPaid.Select(x => x.Receipt_NO).FirstOrDefault();
                if (IPAdmission.Ipa_No == null || IPAdmission.Ipa_No == "" && IPAdmission.Ip_No == null || IPAdmission.Ip_No == "")
                {
                    return new
                    {
                        Success = false,
                        Message = "IP Admission IPA_NO or IP_NO is not available in Running Control Table"
                    };
                }


                if (patientAdmission.Master.IP_NO == null)
                {
                    IPAdmission.Ip_No = GenerateRunningCtrlNo("IP_NO", 1);
                }
                else
                {
                    IPAdmission.Ip_No = patientAdmission.Master.IP_NO;
                }


                IPAdmission.Admission_Date = patientAdmission.IPAdmission.Admission_Date;
                IPAdmission.Expected_Discharge_Date = patientAdmission.IPAdmission.Expected_Discharge_Date;
                IPAdmission.Anesthesia = patientAdmission.IPAdmission.Anesthesia;
                //Context.Ip_Admission.Add(IPAdmission);



                var additionalprocedure = new AdditionalProcedureTrans();
                if (patientAdmission.PatientCounselNew.Clinicprocedure != null && patientAdmission.PatientCounselNew.Clinicprocedure != "")
                {

                    additionalprocedure.Test_Code = patientAdmission.PatientCounselNew.Clinicprocedure;
                    additionalprocedure.UIN = patientAdmission.Master.UIN;
                    additionalprocedure.IPANO = patientAdmission.IPAdmission.Ipa_No;
                    additionalprocedure.Siteid = patientAdmission.IPAdmission.SiteId;
                    Context.Additional_Procedure_Trans.Add(additionalprocedure);
                }

            }


            var IPAccount = new IPAccount();
            IPAccount.Ipa_No = patientAdmission.IPAdmission.Ipa_No;
            IPAccount.Mr_No = patientAdmission.IPAdmission.Mr_No;
            IPAccount.Status = "ADM";
            IPAccount.System = DateTime.Now.Date;
            IPAccount.Ip_No = patientAdmission.IPAdmission.Ip_No;
            IPAccount.Siteid = patientAdmission.IPAdmission.SiteId;
            IPAccount.Uin = patientAdmission.Master.UIN;
            Context.IP_Account.Add(IPAccount);

            var IPAccountDetails = new IpAccountDtl();
            IPAccountDetails.Ipa_Sl_No = Convert.ToInt32(GenerateRunningCtrlNo("Ipa_Sl_No", 1));
            if (IPAccountDetails.Ipa_Sl_No == 0)
            {
                return new
                {
                    Success = false,
                    Message = "IP Account Details Ipa_Sl_No is not available in Running Control Table"
                };
            }
            IPAccountDetails.Ipa_No = patientAdmission.IPAdmission.Ipa_No;
            IPAccountDetails.Cash_Flow_Code = "501";
            //IPAccountDetails.Tinvoice_No = patientAdmission.CashPaid.Select(x => x.Receipt_NO).FirstOrDefault(); //patientAdmission.IPAdmission.Receipt_No;
            IPAccountDetails.Sysdate = DateTime.Now.Date;
            IPAccountDetails.Date = DateTime.Now.Date;
            IPAccountDetails.Cost_Value = decimal.Parse(patientAdmission.CashPaid.Sum(x => x.Fees_Paid).ToString());
            //IPAccountDetails.Cancel = "NULL";
            IPAccountDetails.SiteId = patientAdmission.IPAdmission.SiteId;
            IPAccountDetails.UIN = patientAdmission.Master.UIN;
            //Context.Ip_Account_Dtl.Add(IPAccountDetails);

            //var RoomType = Context.Room_Master.Where(x => x.Paytype == RoomPayType).Select(x => new Dropdown { Text = x.Room_Description, Value = x.Room_Type }).OrderBy(x => x.Text).ToList();

            var SurgeryGroupCode = Context.ICD_Code_Master.Where(x => x.Icd_Code == patientAdmission.IPAdmission.Surgery_Code).Select(x => x.Surgery_Group_Code).FirstOrDefault();

            var OtSurgeryHdr = new OTSurgeryHdr();
            OtSurgeryHdr.Surgery_No = Convert.ToDecimal(GenerateRunningCtrlNo("Surgery_NO", 1));
            if (OtSurgeryHdr.Surgery_No == 0)
            {
                return new
                {
                    Success = false,
                    Message = "Ot Surgery Hdr Surgery_No is not available in Running Control Table"
                };
            }
            OtSurgeryHdr.Surgery_Date = patientAdmission.SurgeryDate;
            OtSurgeryHdr.Mr_No = patientAdmission.Master.MR_NO;
            OtSurgeryHdr.Ipa_No = patientAdmission.IPAdmission.Ipa_No;
            OtSurgeryHdr.Ip_No = patientAdmission.IPAdmission.Ip_No;
            OtSurgeryHdr.Surgery_Code = patientAdmission.IPAdmission.Surgery_Code;
            OtSurgeryHdr.Eye = patientAdmission.IPAdmission.Eye;
            OtSurgeryHdr.Anesthesia_Code = patientAdmission.IPAdmission.Anesthesia;
            OtSurgeryHdr.Doctor_Code = patientAdmission.IPAdmission.Doctor_Code;
            OtSurgeryHdr.Sysdate = DateTime.Now;
            OtSurgeryHdr.SiteId = patientAdmission.IPAdmission.SiteId;
            OtSurgeryHdr.UIN = patientAdmission.Master.UIN;
            OtSurgeryHdr.Surgery_Group_Code = SurgeryGroupCode;
            if (patientAdmission.Master.Patient_Class == "SITE01")
            {
                OtSurgeryHdr.Operation_Theatre_Name = "LC0015";
            }
            else
            {
                OtSurgeryHdr.Operation_Theatre_Name = "LC0016";
            }


            Context.OT_Surgery_Hdr.Add(OtSurgeryHdr);



            var master = Context.Patient_Registration_Master.Where(x => x.UIN == patientAdmission.IPAdmission.UIN && x.SiteId == patientAdmission.IPAdmission.SiteId && x.IP_NO == null).FirstOrDefault();
            if (master != null)
            {
                master.IP_NO = patientAdmission.IPAdmission.Ip_No;
                Context.Entry(master).State = EntityState.Modified;
            }


            var CurrentRoomStatus = Context.Current_Room_Status.Where(x => x.Room_No == patientAdmission.IPAdmission.Room_No && x.Siteid == patientAdmission.IPAdmission.SiteId).ToList();
            if (CurrentRoomStatus.Count() > 0)
            {
                CurrentRoomStatus.All(x =>
                {
                    x.Mr_No = patientAdmission.IPAdmission.Mr_No; x.UIN = patientAdmission.IPAdmission.UIN;
                    x.Occupied_Date = patientAdmission.IPAdmission.Admission_Date; x.Expected_Discharge_Date = patientAdmission.IPAdmission.Expected_Discharge_Date; x.Sysdate = DateTime.Now.Date; x.Occupy_Flag_Code = "OFC001"; return true;
                });
                Context.Current_Room_Status.UpdateRange(CurrentRoomStatus);
            }





            var OtlFlag = new OtlFlag();
            OtlFlag.Surgery_No = OtSurgeryHdr.Surgery_No;
            OtlFlag.Surgery_Date = patientAdmission.SurgeryDate;
            OtlFlag.Mr_No = patientAdmission.Master.MR_NO;
            //OtlFlag.Status = patientAdmission.IPAdmission.Surgery_Type_Code;
            OtlFlag.Status = "T";
            OtlFlag.SiteId = patientAdmission.IPAdmission.SiteId;
            OtlFlag.UIN = patientAdmission.Master.UIN;
            Context.OtlFlag.Add(OtlFlag);

            var IpClinic = new IPClinic();
            IpClinic.Mr_No = patientAdmission.Master.MR_NO;
            IpClinic.Ipa_No = patientAdmission.IPAdmission.Ipa_No;
            IpClinic.Clinic_Code = patientAdmission.IPClinic.Clinic_Code;
            IpClinic.Siteid = patientAdmission.IPAdmission.SiteId;
            IpClinic.Uin = patientAdmission.Master.UIN;
            IpClinic.Adm_Status = patientAdmission.IPClinic.Adm_Status;
            Context.IP_Clinic.Add(IpClinic);



            var isOPeXists = Context.Op_Visit_Datewise.Where(x => x.Date == DateTime.Now.Date && x.Patient_Class == patientAdmission.Master.Patient_Class).Count();
            var opvd = Context.Op_Visit_Datewise.Where(x => x.Patient_Class == patientAdmission.Master.Patient_Class && x.Date == DateTime.Now.Date).FirstOrDefault();
            var opvdw = new OpVisitDatewise();
            opvdw.Patient_Class = patientAdmission.Master.Patient_Class;
            opvdw.Date = DateTime.Now.Date;
            opvdw.New = 0;
            opvdw.Old = 0;

            opvdw.Week_Day = (int)DateTime.Now.DayOfWeek;
            if (isOPeXists > 0)
            {

                if (opvd != null)
                {
                    opvd.Admission += 1;
                    Context.Entry(opvd).State = EntityState.Modified;
                }
            }
            //else if (isOPeXists == 0)
            //{
            //    opvd.Admission = 1;
            //    Context.Op_Visit_Datewise.Add(opvdw);
            //}




            //CASH CALCULATION


            //if (patientAdmission.Corporate == null)
            //{




            if (patientAdmission.PaymentDetail.Amount > 0) //patientAdmission.CashPaid.Count() > 0 &&
            {
                var subsidyDetails = patientAdmission.CashPaid.FirstOrDefault();
                if (subsidyDetails.Category_Code == "CATC001" && patientAdmission.CashPaid.Count() == 1)  //==1
                {
                }

                var invoiceNo = GenerateRunningCtrlNo("INVOICE_NO", 1);
                if (invoiceNo == null || invoiceNo == "")
                {
                    return new
                    {
                        Success = false,
                        Message = "INVOICE_NO is not available in Running Control Table"
                    };
                }
                patientAdmission.Invoice = new InvoiceMaster();
                patientAdmission.Invoice.Invoice_No = invoiceNo;
                patientAdmission.Invoice.Invoice_Date = DateTime.Now;
                patientAdmission.Invoice.Module_Code = patientAdmission.CashPaid.FirstOrDefault().Module_Code;
                patientAdmission.Invoice.Invoice_Value = float.Parse(patientAdmission.CashPaid.Sum(x => x.Fees_Paid).ToString());
                patientAdmission.Invoice.Invoice_Received_Value = patientAdmission.Invoice.Invoice_Value;
                patientAdmission.Invoice.SiteId = patientAdmission.IPAdmission.SiteId;
                patientAdmission.Invoice.UIN = patientAdmission.Master.UIN;
                patientAdmission.Invoice.MR_NO = patientAdmission.Master.MR_NO;
                //patientAdmission.Invoice.Cash_Paid = patientAdmission.CashPaid;
                Context.Invoice_Master.Add(patientAdmission.Invoice);


                foreach (var item in patientAdmission.CashPaid.ToList())
                {
                    if (item.Fees_Paid > 0)
                    {

                        var cashpaid = new CashPaid();

                        cashpaid.MR_NO = patientAdmission.Master.MR_NO;
                        cashpaid.UIN = patientAdmission.Master.UIN;
                        cashpaid.Date = DateTime.Now.Date;
                        cashpaid.Sysdate = DateTime.Now;
                        cashpaid.Invoice_No = invoiceNo;
                        cashpaid.OP_IP_Flag = "IP";
                        cashpaid.Fees_Paid = item.Fees_Paid;
                        cashpaid.IPA_NO = patientAdmission.IPAdmission.Ipa_No;
                        cashpaid.Module_Code = item.Module_Code;
                        cashpaid.Operator_Code = item.Operator_Code;
                        if (patientAdmission.Master.Patient_Class == "SITE01")
                        {
                            cashpaid.Receipt_NO = GenerateRunningCtrlNo("RES_RECEIPT_NO", 1);

                            patientAdmission.IPAdmission.Receipt_No = cashpaid.Receipt_NO;
                            cashpaid.Category_Code = item.Category_Code;
                        }
                        else
                        {
                            cashpaid.Receipt_NO = GenerateRunningCtrlNo("FRES_RECEIPT_NO", 1);
                            cashpaid.Category_Code = "CATC002";
                        }

                        // cashpaid.Receipt_NO = patientAdmission.IPAdmission.Receipt_No;//  GenerateRunningCtrlNo("RES_RECEIPT_NO", 1);

                        cashpaid.Transaction_Code = item.Transaction_Code;
                        cashpaid.Paymode_Code = item.Paymode_Code;
                        cashpaid.Outstanding = item.Outstanding;

                        cashpaid.SiteId = patientAdmission.IPAdmission.SiteId;

                        Context.Cash_Paid.AddRange(cashpaid);



                        //patientAdmission.CashPaid.All(x =>
                        //{

                        //    x.MR_NO = patientAdmission.Master.MR_NO;
                        //    x.UIN = patientAdmission.Master.UIN;
                        //    x.Date = DateTime.Now.Date;
                        //    x.Sysdate = DateTime.Now;
                        //    x.Invoice_No = invoiceNo;
                        //    x.OP_IP_Flag = "IP";
                        //    x.IPA_NO = patientAdmission.IPAdmission.Ipa_No;
                        //    return true;


                        //});
                    }
                }




                //foreach (var item in patientAdmission.CashPaid.)
                //{
                //    item.Receipt_NO = GenerateRunningCtrlNo("RES_RECEIPT_NO", 1);
                //    if (item.Receipt_NO == null || item.Receipt_NO == "")
                //    {
                //        return new
                //        {
                //            Success = false,
                //            Message = "CashPaid OP_RECEIPT_NO is not available in Running Control Table"
                //        };
                //    }
                //}





                //patientAdmission.Detail.Invoice_No = invoiceNo;

                if (patientAdmission.PaymentDetail != null)
                {
                    patientAdmission.PaymentDetail.UIN = patientAdmission.Master.UIN;
                    patientAdmission.PaymentDetail.Invoice_No = invoiceNo;
                    patientAdmission.PaymentDetail.Receipt_No = patientAdmission.IPAdmission.Receipt_No;//patientAdmission.CashPaid.Select(x => x.Receipt_NO).FirstOrDefault();
                    patientAdmission.PaymentDetail.CreatedUTC = DateTime.UtcNow;
                    patientAdmission.PaymentDetail.Id = 0;
                    patientAdmission.PaymentDetail.MR_NO = patientAdmission.Master.MR_NO;
                    patientAdmission.PaymentDetail.SiteId = patientAdmission.IPAdmission.SiteId;
                    patientAdmission.PaymentDetail.UIN = patientAdmission.Master.UIN;
                    if (patientAdmission.PaymentDetail.BankName == null)
                    {
                        patientAdmission.PaymentDetail.BankName = "";
                    }
                    if (patientAdmission.PaymentDetail.Branch == null)
                    {
                        patientAdmission.PaymentDetail.Branch = "";
                    }
                    if (patientAdmission.PaymentDetail.InstrumentNumber == null)
                    {
                        patientAdmission.PaymentDetail.InstrumentNumber = "";
                    }

                    Context.PaymentDetail.Add(patientAdmission.PaymentDetail);

                }





                //Corporate Calculation



                //if (patientAdmission.Corporate != null && patientAdmission.Corporate.Amount_Sponsored != 0)
                //{
                //    patientAdmission.Corporate.Corporate_Case_No = GenerateRunningCtrlNo("CORPORATE_CASE_NO");
                //    patientAdmission.Corporate.UIN = patientAdmission.Master.UIN;
                //    patientAdmission.Corporate.SiteId = patientAdmission.IPAdmission.SiteId;
                //    patientAdmission.Corporate.IP_No = patientAdmission.IPAdmission.Ip_No;
                //    patientAdmission.Corporate.Invoice_No = invoiceNo;
                //    patientAdmission.Corporate.Visit_Date = DateTime.Now;
                //    patientAdmission.Corporate.Sysdate = DateTime.Now;
                //    patientAdmission.Corporate.CreatedUTC = DateTime.UtcNow;
                //    patientAdmission.Corporate.MR_NO = patientAdmission.Master.MR_NO;

                //}

            }


            //}
            //else
            //{

            foreach (var item in patientAdmission.CashPaid.ToList())
            {
                if (item.Category_Code == "CATC004")
                {
                    patientAdmission.Corporate.Corporate_Case_No = GenerateRunningCtrlNo("CORPORATE_CASE_NO", 1);
                    if (patientAdmission.Corporate.Corporate_Case_No == null || patientAdmission.Corporate.Corporate_Case_No == "")
                    {
                        return new
                        {
                            Success = false,
                            Message = "Corporate Cases  CORPORATE_CASE_NO is not available in Running Control Table"
                        };
                    }
                    patientAdmission.Corporate.Corporate_Case_No = patientAdmission.Corporate.Corporate_Case_No;
                    patientAdmission.Corporate.UIN = patientAdmission.IPAdmission.UIN;
                    patientAdmission.Corporate.SiteId = patientAdmission.IPAdmission.SiteId;
                    patientAdmission.Corporate.IP_No = patientAdmission.IPAdmission.Ipa_No;
                    patientAdmission.Corporate.Invoice_No = "0";
                    patientAdmission.Corporate.Cor_Sl_No = patientAdmission.CorSlNo;
                    patientAdmission.Corporate.Visit_Date = DateTime.Now;
                    patientAdmission.Corporate.Sysdate = DateTime.Now;
                    patientAdmission.Corporate.CreatedUTC = DateTime.UtcNow;
                    patientAdmission.Corporate.Bill_No = "0";
                    patientAdmission.Corporate.Module_Code = "MOD4";
                    patientAdmission.Corporate.MR_NO = patientAdmission.IPAdmission.Mr_No;
                    patientAdmission.IPAdmission.Category_Code = "CATC004";
                    Context.Corporate_Cases.Add(patientAdmission.Corporate);
                }
            }

            //        var cashpaids = new CashPaid();
            //if (patientAdmission.CorporatePreauth != null)//patientAdmission.CorporatePreauth != null
            //    {

            //        if (patientAdmission.Corporate != null) //&& patientAdmission.CorporatePreauth.Copay != 0
            //        {
            //            //patientAdmission.Corporate = new CorporateCases();
            //            patientAdmission.Corporate.Corporate_Case_No = GenerateRunningCtrlNo("CORPORATE_CASE_NO", 1);
            //            if (patientAdmission.Corporate.Corporate_Case_No == null || patientAdmission.Corporate.Corporate_Case_No == "")
            //            {
            //                return new
            //                {
            //                    Success = false,
            //                    Message = "Corporate Cases  CORPORATE_CASE_NO is not available in Running Control Table"
            //                };
            //            }
            //            patientAdmission.Corporate.Corporate_Case_No = patientAdmission.Corporate.Corporate_Case_No;
            //            patientAdmission.Corporate.UIN = patientAdmission.IPAdmission.UIN;
            //            patientAdmission.Corporate.SiteId = patientAdmission.IPAdmission.SiteId;
            //            patientAdmission.Corporate.IP_No = patientAdmission.IPAdmission.Ipa_No;
            //            patientAdmission.Corporate.Invoice_No = "0";
            //            patientAdmission.Corporate.Cor_Sl_No = patientAdmission.CorSlNo;
            //            patientAdmission.Corporate.Visit_Date = DateTime.Now;
            //            patientAdmission.Corporate.Sysdate = DateTime.Now;
            //            patientAdmission.Corporate.CreatedUTC = DateTime.UtcNow;
            //            patientAdmission.Corporate.Bill_No = "0";
            //            patientAdmission.Corporate.Module_Code = "MOD4";
            //            patientAdmission.Corporate.MR_NO = patientAdmission.IPAdmission.Mr_No;
            //            Context.Corporate_Cases.Add(patientAdmission.Corporate);
            //        }
            //    }
            //}










            patientAdmission.IPAdmission.Camp_Code = null;
            patientAdmission.IPAdmission.Room_Type2 = null;
            patientAdmission.IPAdmission.Room_Transfer = null;

            if (patientAdmission.PaymentDetail.Amount > 0)
            {
                patientAdmission.IPAdmission.Tinvoice_No = patientAdmission.Invoice.Invoice_No;
                IPAccountDetails.Tinvoice_No = patientAdmission.Invoice.Invoice_No;
            }
            else
            {
                patientAdmission.IPAdmission.Tinvoice_No = null;
                IPAccountDetails.Tinvoice_No = null;

            }



            Context.Ip_Admission.Add(IPAdmission);
            IPAccountDetails.Tinvoice_No = patientAdmission.Invoice.Invoice_No;
            if (patientAdmission.PaymentDetail.Amount > 0)
            {

                Context.Ip_Account_Dtl.Add(IPAccountDetails);
            }



            var IpSurgeryDetails = new IpSurgeryDtl();
            IpSurgeryDetails.Ipa_No = patientAdmission.IPAdmission.Ipa_No;
            IpSurgeryDetails.Surgery_Code = patientAdmission.IPAdmission.Surgery_Code;
            IpSurgeryDetails.Surgery_Type_Code = patientAdmission.IPAdmission.Surgery_Type_Code;
            IpSurgeryDetails.Anaesthesia = patientAdmission.IPAdmission.Anesthesia;
            IpSurgeryDetails.Doctor_Code = patientAdmission.IPAdmission.Doctor_Code;
            IpSurgeryDetails.Eye = patientAdmission.IPAdmission.Eye;
            IpSurgeryDetails.Surgery_Date = patientAdmission.SurgeryDate;
            IpSurgeryDetails.Surgery_Approval = "Y";
            IpSurgeryDetails.Surgery_Done = "N";
            IpSurgeryDetails.SiteId = patientAdmission.IPAdmission.SiteId;
            IpSurgeryDetails.UIN = patientAdmission.Master.UIN;
            Context.Ip_Surgery_Dtl.Add(IpSurgeryDetails);







            if (patientAdmission.Denomination.Count() > 0)
            {
                var denomination = new DenominationDetails();
                foreach (var item in patientAdmission.Denomination.ToList())
                {
                    denomination.Rec1 = item.Rec1;
                    denomination.Rec2 = item.Rec2;
                    denomination.Rec5 = item.Rec5;
                    denomination.Rec10 = item.Rec10;
                    denomination.Rec20 = item.Rec20;
                    denomination.Rec50 = item.Rec50;
                    denomination.Rec100 = item.Rec100;
                    denomination.Rec200 = item.Rec200;
                    denomination.Rec500 = item.Rec500;
                    denomination.Rec2000 = item.Rec2000;

                    denomination.Bal1 = item.Bal1;
                    denomination.Bal2 = item.Bal2;
                    denomination.Bal5 = item.Bal5;
                    denomination.Bal10 = item.Bal10;
                    denomination.Bal20 = item.Bal20;
                    denomination.Bal50 = item.Bal50;
                    denomination.Bal100 = item.Rec100;
                    denomination.Bal200 = item.Bal200;
                    denomination.Bal500 = item.Bal500;
                    denomination.Bal2000 = item.Bal2000;
                    denomination.Uin = patientAdmission.IPAdmission.UIN;
                    denomination.Siteid = patientAdmission.IPAdmission.SiteId;
                    denomination.Mr_No = patientAdmission.IPAdmission.Mr_No;
                    denomination.Ipa_No = patientAdmission.IPAdmission.Ipa_No;
                    denomination.Date = DateTime.Now;
                    denomination.Module_Code = "MOD4";
                    denomination.Receipt_No = patientAdmission.IPAdmission.Receipt_No;//patientAdmission.CashPaid.Select(x => x.Receipt_NO).FirstOrDefault();
                    Context.Denomination_Details.Add(denomination);




                }
            }




            try
            {
                if (Context.SaveChanges() > 0)
                    return new
                    {
                        Success = true,
                        //Recedetails = patientAdmission.receiptdetails,
                        ReceiptNo = patientAdmission.PaymentDetail.Receipt_No,
                        eyes = patientAdmission.IPAdmission.Eye,
                        sexx = patientAdmission.Master.Sex,
                        roomno = patientAdmission.IPAdmission.Room_No,
                        ipno = patientAdmission.IPAdmission.Ipa_No,

                        last3mrno = patientAdmission.IPAdmission.Mr_No.Substring(patientAdmission.IPAdmission.Mr_No.Length - 3),
                        firstsetmrno = patientAdmission.IPAdmission.Mr_No.Substring(0, patientAdmission.IPAdmission.Mr_No.Length - 3),

                        //corporatecode = Context.Corporate_Preauth.Where(x => x.UIN == patientAdmission.IPAdmission.UIN && x.SiteId == patientAdmission.IPAdmission.SiteId).Select(x => x.Corporate).FirstOrDefault(),
                         corporatename = Context.Corporate_Master.Where(x => x.Corporate_Code == patientAdmission.CorporatePreauth.Corporate).Select(x => x.Corporate_Name).FirstOrDefault(),


                surgery = patientAdmission.SurgeryName = Context.ICD_Code_Master.Where(x => x.Icd_Code == patientAdmission.IPAdmission.Surgery_Code).Select(x => x.Icd_Description).FirstOrDefault(),

                        ClinicCode = patientAdmission.ClinicCode = Context.Icd_Code_Spec_Group_Code_Master.Where(x => x.Icd_Code_Speciality_Group_Code == patientAdmission.IPClinic.Clinic_Code).Select(x => x.Icd_Code_Speciality_Group_Desc).FirstOrDefault(),

                        Ages = patientAdmission.Age,
                        Admissiondate = patientAdmission.IPAdmission.Admission_Date,
                        //mnos = patientAdmission.Mr_No.Substring(0, patientAdmission.IPAdmission.Mr_No.Length - 3) + "-" + patientAdmission.IPAdmission.Mr_No.Substring(patientAdmission.IPAdmission.Mr_No.Length - 3),

                        //floorname= patientAdmission.FloorName = Context.Floor_Master.Where(x => x.Floor_Code ==patientAdmission.FloorCode).Select(x => x.Floor_Description).FirstOrDefault(),

                        Recedetails = patientAdmission.receiptdetails = (from cp in Context.Cash_Paid.Where(x => x.UIN == patientAdmission.IPAdmission.UIN)
                                                                         join sc in Context.Service_Category on cp.Category_Code equals sc.Category_Code
                                                                         join mp in Context.Mode_Of_Payment on cp.Paymode_Code equals mp.Paymode_Code
                                                                         where cp.Module_Code == "MOD4"
                                                                         select new Rdetails
                                                                         {
                                                                             Rno = cp.Receipt_NO,
                                                                             Amountpaid = cp.Fees_Paid,
                                                                             Descrip = sc.Category_Description,
                                                                             Paymode = mp.Description,
                                                                         }).ToList(),
                        totalamount = patientAdmission.receiptdetails.Sum(x => x.Amountpaid),



                     



                //        var now = DateTime.Now;
                //var dbo = patientAdmission.Master.Date_Of_Birth;
                //var age = now.Year - dbo.Year;
                //if (dbo > now.AddYears(-age))
                //    age--;

                //patientAdmission.Age = age;




                Message = "Patient Admission Details saved successfully"
                    };
            }
            catch (Exception ex)
            {
                return new
                {
                    Success = false,
                    Message = ex.ToString().Trim()
                };


            }
            return new
            {
                Success = false,
                Message = "Some data are Missing"
            };
        }


        public IEnumerable<Dropdown> GetEmployeeGrade(string corporateCode)
        {
            return Context.Corporate_Master_Dtl.Where(x => x.Corporate_Code == corporateCode).Select(x => new Dropdown { Text = x.Employee_Grade, Value = x.Cor_Sl_No }).OrderBy(x => x.Text).ToList();
        }


        private string GenerateRunningCtrlNo(string rnControlCode, int? siteId = null)
        {
            var rn = Context.Running_Number_Control.FirstOrDefault(x => x.RnControl_Code == rnControlCode && x.IsActive == true && x.SiteId == siteId);
            if (rn != null)
            {
                rn.Control_Value += 1;
                Context.Entry(rn).State = EntityState.Modified;
                return $"{rn.Control_String_Value}{rn.Control_Value}";
            }
            else
                return "";
        }


        public double GetViewCollectionByOperator(string operatorCode, int siteId)
        {
            return Context.Cash_Paid.Where(x => x.Operator_Code == operatorCode && x.Sysdate.Date == DateTime.Now.Date && x.Cancel != "Y" && x.Module_Code == "MOD4" && x.SiteId == siteId).Sum(x => x.Fees_Paid);
        }


        public IEnumerable<Dropdown> GetRoomPayType(string RoomPayType)
        {

            var RoomType = Context.Room_Master.Where(x => x.Paytype == RoomPayType).Select(x => new Dropdown { Text = x.Room_Description, Value = x.Room_Type }).OrderBy(x => x.Text).ToList();
            return RoomType;
        }

        public AdditionalAdvanceViewModel GetIPADetails(string no, bool isIpaNo, int siteId)
        {
            var result = new AdditionalAdvanceViewModel();

            result.Denomination = new List<DenominationDetails>();
            result.CashPaid = new List<CashPaid>();

            IPAdmission ipAdmission = null;
            if (isIpaNo == true)
            {
                ipAdmission = Context.Ip_Admission.Where(x => x.Ipa_No == no && x.SiteId == siteId).Include(x => x.Ip_Surgery_Dtl).AsNoTracking().FirstOrDefault();
            }

            else
            {
                ipAdmission = Context.Ip_Admission.Where(x => x.UIN == no && x.SiteId == siteId).Include(x => x.Ip_Surgery_Dtl).AsNoTracking().FirstOrDefault();
            }
            if (ipAdmission != null)
            {
                if (ipAdmission.Discharge_Status == "ADM")
                {
                    var master = Context.Patient_Registration_Master.Where(x => x.MR_NO == ipAdmission.Mr_No && x.SiteId == siteId).AsNoTracking().Include(x => x.Referral_Patients_HDR).FirstOrDefault();
                    if (master != null)
                    {
                        var now = DateTime.Now;
                        var dbo = master.Date_Of_Birth;
                        var age = now.Year - dbo.Year;
                        if (dbo > now.AddYears(-age))
                            age--;
                        var gender = "";
                        switch (master.Sex)
                        {
                            case "M":
                                gender = "Male";
                                break;
                            case "F":
                                gender = "Female";
                                break;
                            case "T":
                                gender = "Transgender";
                                break;
                        }
                        var district = Context.District_Master.Where(x => x.District_Code == master.District).Select(x => x.District_Name).FirstOrDefault();
                        var state = Context.State_Master.Where(x => x.State_Code == master.State).Select(x => x.State_Name).FirstOrDefault();

                        result.Success = true;
                        result.PatientName = master.Patient_Name;
                        result.NextOfKin = $"{master.Next_Of_Kin}, {master.MR_NO}, {master.UIN}";
                        result.PatientDetail = $"{age} yrs, {gender}";
                        result.Address = $"{district}, {state}";
                        result.PatientClass = Context.Site_Master.Where(x => x.Site_Code == master.Patient_Class).Select(x => x.Site_Name).FirstOrDefault();
                        result.Mr_No = ipAdmission.Mr_No;
                        result.UIN = master.UIN;
                        result.IPA_No = ipAdmission.Ipa_No;
                        result.SiteId = siteId;
                        result.Eye = Context.EYEMASTER.Where(x => x.EyeCode == ipAdmission.Eye).Select(x => x.Description).FirstOrDefault();
                        result.SurgeryName = Context.ICD_Code_Master.Where(x => x.Icd_Code == ipAdmission.Surgery_Code).Select(x => x.Icd_Description).FirstOrDefault();
                          result.SurgeryDate = ipAdmission.Ip_Surgery_Dtl.Surgery_Date.Date.ToString("dd/MM/yyyy");
                        result.Doctor = Context.Doctor_Master.Where(x => x.Doctor_Code == ipAdmission.Doctor_Code).Select(x => x.Doctor_Name).FirstOrDefault();
                        //result.Clinic = (from ipc in Context.IP_Clinic.Where(x => x.Ipa_No == ipAdmission.Ipa_No && x.Siteid == ipAdmission.SiteId && x.Uin == ipAdmission.UIN)
                        //                 join lm in Context.Location_Master on ipc.Clinic_Code equals lm.Location_Code
                        //                 select lm.Location_Name).FirstOrDefault();



                        var locode = Context.IP_Clinic.Where(x => x.Ipa_No == ipAdmission.Ipa_No && x.Siteid == ipAdmission.SiteId && x.Uin == ipAdmission.UIN).Select(X => X.Clinic_Code).FirstOrDefault();

                        result.Clinic = Context.Icd_Code_Spec_Group_Code_Master.Where(x => x.Icd_Code_Speciality_Group_Code == locode).Select(x => x.Icd_Code_Speciality_Group_Desc).FirstOrDefault();




                        result.IPPaymentDetail = Context.Cash_Paid
                            .Where(x => x.MR_NO == ipAdmission.Mr_No && x.SiteId == ipAdmission.SiteId && x.Module_Code == "MOD4" && x.Category_Code == "CATC001" && (x.Transaction_Code == "RECPT")).OrderBy(x => x.Receipt_NO).ThenBy(x => x.Sysdate.Date)
                            .Select(x => new IPPaymentDetail
                            {
                                Source = x.Transaction_Code == "RECPT" ? "Admission Advance" : "Interim Refund",
                                Date = x.Sysdate.ToString("dd/MM/yyyy"),
                                Amount = x.Fees_Paid,
                                TransationCode = x.Transaction_Code
                            }).ToList();
                    }
                    if (result.IPPaymentDetail.FirstOrDefault() != null)
                        //result.IPPaymentDetail.FirstOrDefault().Source = "Advance";
                        result.PatientCategory = "Full Payment";
                    result.TotalAdvance = result.IPPaymentDetail.Where(x => x.TransationCode == "RECPT").Sum(x => x.Amount);
                    result.TotalRefund = result.IPPaymentDetail.Where(x => x.TransationCode == "REFND").Sum(x => x.Amount);
                    result.TotalAmount = result.TotalAdvance - result.TotalRefund;
                }
                else
                {
                    result.IsPatientDischarged = true;
                }
            }
            return result;
        }

        public dynamic SaveAdditionalAdvance(AdditionalAdvanceViewModel additionalAdvance)
        {





            var advancereceived = new CashPaid();
            advancereceived.Fees_Paid = Context.Cash_Paid.OrderByDescending(x => x.IPA_NO == additionalAdvance.IPA_No && x.SiteId == additionalAdvance.SiteId).Select(s => s.Fees_Paid).FirstOrDefault();
            advancereceived.Receipt_NO = Context.Cash_Paid.OrderByDescending(x => x.IPA_NO == additionalAdvance.IPA_No && x.UIN == additionalAdvance.UIN).Select(s => s.Receipt_NO).FirstOrDefault();
            advancereceived.Date = Context.Cash_Paid.OrderByDescending(x => x.IPA_NO == additionalAdvance.IPA_No && x.UIN == additionalAdvance.UIN).Select(s => s.Date).FirstOrDefault();
            //var Fees_Paid = Context.Cash_Paid.OrderByDescending(x => x.IPA_NO == additionalAdvance.IPA_No && x.UIN == additionalAdvance.UIN).Select(s => s.Fees_Paid).ToList();
            //var Receipt_NO = Context.Cash_Paid.OrderBy(x => x.IPA_NO == additionalAdvance.IPA_No && x.UIN == additionalAdvance.UIN).Select(s => s.Receipt_NO).ToList();
            //var Date = Context.Cash_Paid.OrderBy(x => x.IPA_NO == additionalAdvance.IPA_No && x.UIN == additionalAdvance.UIN).Select(s => s.Date).ToList();



            var admission = new IPAdmission();
            admission.Mr_No = additionalAdvance.Mr_No;
            admission.UIN = additionalAdvance.UIN;
            admission.Ipa_No = additionalAdvance.IPA_No;
            admission.Room_No = Context.Ip_Admission.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Room_No).FirstOrDefault();
            admission.Receipt_No = Context.Ip_Admission.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Receipt_No).FirstOrDefault();
            admission.Eye = Context.Ip_Admission.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Eye).FirstOrDefault();

            var regmaster = new PatientRegistrationMaster();
            regmaster.Patient_Name = additionalAdvance.PatientName;
            regmaster.Street_Locality = Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Street_Locality).FirstOrDefault();
            regmaster.Town_City = Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Town_City).FirstOrDefault();
            regmaster.Next_Of_Kin = Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Next_Of_Kin).FirstOrDefault();
            regmaster.Sex = Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Sex).FirstOrDefault();
            regmaster.Police_Station = Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Police_Station).FirstOrDefault();
            regmaster.Phone = Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Phone).FirstOrDefault();
            regmaster.Email_Id = Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Email_Id).FirstOrDefault();
            regmaster.Pincode = Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Pincode).FirstOrDefault();
            regmaster.State = (from prm in Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN && x.SiteId == additionalAdvance.SiteId)
                               join sm in Context.State_Master on prm.State equals sm.State_Code
                               select sm.State_Name).FirstOrDefault();

            regmaster.District = (from prm in Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN && x.SiteId == additionalAdvance.SiteId)
                                  join sm in Context.District_Master on prm.District equals sm.District_Code
                                  select sm.District_Name).FirstOrDefault();
            regmaster.Country = (from prm in Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN && x.SiteId == additionalAdvance.SiteId)
                                 join sm in Context.Country_Master on prm.Country equals sm.Country_Code
                                 select sm.Country_Name).FirstOrDefault();

            regmaster.Taluk = (from prm in Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN && x.SiteId == additionalAdvance.SiteId)
                               join sm in Context.Taluk_Master on prm.Taluk equals sm.Taluk_Code
                               select sm.Taluk_Name).FirstOrDefault();

            regmaster.Date_Of_Birth = Context.Patient_Registration_Master.Where(x => x.UIN == additionalAdvance.UIN).Select(x => x.Date_Of_Birth).FirstOrDefault();


            var now = DateTime.Now;
            var dbo = regmaster.Date_Of_Birth;
            var age = now.Year - dbo.Year;
            if (dbo > now.AddYears(-age))
                age--;
            additionalAdvance.Age = age;


            var crm = new CurrentRoomStatus();
            crm.Floor_Code = (from crs in Context.Current_Room_Status.Where(x => x.UIN == additionalAdvance.UIN)
                              join sm in Context.Floor_Master on crs.Floor_Code equals sm.Floor_Code
                              select sm.Floor_Description).FirstOrDefault();

            var clinic = new IPClinic();
            clinic.Clinic_Code = (from ipc in Context.IP_Clinic.Where(x => x.Uin == additionalAdvance.UIN)
                                  join lm in Context.Location_Master on ipc.Clinic_Code equals lm.Location_Code
                                  select lm.Location_Name).FirstOrDefault();


            var modepay = new ModeOfPayment();

            modepay.Paymode_Code = (from mp in Context.Cash_Paid.Where(x => x.MR_NO == additionalAdvance.Mr_No)
                                    join lm in Context.Mode_Of_Payment on mp.Paymode_Code equals lm.Paymode_Code
                                    select lm.Description).FirstOrDefault();

            var cashPaid = new CashPaid();
            cashPaid.Receipt_NO = GenerateRunningCtrlNo("RES_RECEIPT_NO", additionalAdvance.SiteId);
            cashPaid.Operator_Code = additionalAdvance.OperatorCode.ToString();
            cashPaid.Module_Code = "MOD4";
            cashPaid.MR_NO = additionalAdvance.Mr_No;
            cashPaid.IPA_NO = additionalAdvance.IPA_No;
            cashPaid.Fees_Paid = additionalAdvance.AdvanceAmount;
            cashPaid.Date = DateTime.Now.Date;
            cashPaid.Transaction_Code = "RECPT";
            cashPaid.Category_Code = "CATC001";
            cashPaid.Paymode_Code = additionalAdvance.PaymentType;
            cashPaid.OP_IP_Flag = "IP";
            cashPaid.Outstanding = "N";
            cashPaid.Sysdate = DateTime.Now;
            cashPaid.UIN = additionalAdvance.UIN;
            cashPaid.SiteId = additionalAdvance.SiteId;

            var invoiceMaster = new InvoiceMaster();
            invoiceMaster.Invoice_No = GenerateRunningCtrlNo("INVOICE_NO", additionalAdvance.SiteId);
            invoiceMaster.Invoice_Date = DateTime.Now.Date;
            invoiceMaster.Invoice_Value = float.Parse(additionalAdvance.AdvanceAmount.ToString());
            invoiceMaster.Invoice_Received_Value = invoiceMaster.Invoice_Value;
            invoiceMaster.Module_Code = "MOD4";
            invoiceMaster.UIN = additionalAdvance.UIN;
            invoiceMaster.MR_NO = additionalAdvance.Mr_No;
            invoiceMaster.SiteId = additionalAdvance.SiteId;

            invoiceMaster.Cash_Paid = new List<CashPaid>() { cashPaid };

            var ipAccountDtl = new IpAccountDtl();
            ipAccountDtl.Ipa_Sl_No = _commonRepository.GenerateRunningCtrlNoWithoutPrefix("Ipa_Sl_No", additionalAdvance.SiteId);
            ipAccountDtl.Ipa_No = additionalAdvance.IPA_No;
            ipAccountDtl.Cash_Flow_Code = "501";
            ipAccountDtl.Tinvoice_No = invoiceMaster.Invoice_No;
            ipAccountDtl.Sysdate = DateTime.Now;
            ipAccountDtl.Date = DateTime.Now.Date;
            ipAccountDtl.Cost_Value = Convert.ToDecimal(additionalAdvance.AdvanceAmount);
            ipAccountDtl.UIN = additionalAdvance.UIN;
            ipAccountDtl.SiteId = additionalAdvance.SiteId;




            var denomination = new DenominationDetails();
            foreach (var item in additionalAdvance.Denomination.ToList())
            {
                denomination.Rec1 = item.Rec1;
                denomination.Rec2 = item.Rec2;
                denomination.Rec5 = item.Rec5;
                denomination.Rec10 = item.Rec10;
                denomination.Rec20 = item.Rec20;
                denomination.Rec50 = item.Rec50;
                denomination.Rec100 = item.Rec100;
                denomination.Rec200 = item.Rec200;
                denomination.Rec500 = item.Rec500;
                denomination.Rec2000 = item.Rec2000;

                denomination.Bal1 = item.Bal1;
                denomination.Bal2 = item.Bal2;
                denomination.Bal5 = item.Bal5;
                denomination.Bal10 = item.Bal10;
                denomination.Bal20 = item.Bal20;
                denomination.Bal50 = item.Bal50;
                denomination.Bal100 = item.Rec100;
                denomination.Bal200 = item.Bal200;
                denomination.Bal500 = item.Bal500;
                denomination.Bal2000 = item.Bal2000;
                denomination.Uin = additionalAdvance.UIN;
                denomination.Siteid = additionalAdvance.SiteId;
                denomination.Mr_No = additionalAdvance.Mr_No;
                denomination.Ipa_No = additionalAdvance.IPA_No;
                denomination.Date = DateTime.Now;
                denomination.Module_Code = "MOD4";
                denomination.Receipt_No = additionalAdvance.CashPaid.Select(x => x.Receipt_NO).FirstOrDefault();
                Context.Denomination_Details.Add(denomination);


            }


            if (additionalAdvance.PaymentDetail != null)
            {
                additionalAdvance.PaymentDetail.UIN = additionalAdvance.UIN;
                additionalAdvance.PaymentDetail.Invoice_No = invoiceMaster.Invoice_No;
                additionalAdvance.PaymentDetail.Receipt_No = cashPaid.Receipt_NO;
                additionalAdvance.PaymentDetail.CreatedUTC = DateTime.UtcNow;
                additionalAdvance.PaymentDetail.Id = 0;
                additionalAdvance.PaymentDetail.MR_NO = additionalAdvance.Mr_No;
                additionalAdvance.PaymentDetail.SiteId = additionalAdvance.SiteId;
                additionalAdvance.PaymentDetail.CreatedBy = Convert.ToInt32(additionalAdvance.OperatorCode);
                additionalAdvance.PaymentDetail.Amount = Convert.ToDecimal(additionalAdvance.AdvanceAmount);

                //additionalAdvance.PaymentDetail.UIN = additionalAdvance.UIN;
                if (additionalAdvance.PaymentDetail.BankName == null)
                {
                    additionalAdvance.PaymentDetail.BankName = "";
                }
                if (additionalAdvance.PaymentDetail.Branch == null)
                {
                    additionalAdvance.PaymentDetail.Branch = "";
                }
                if (additionalAdvance.PaymentDetail.InstrumentNumber == null)
                {
                    additionalAdvance.PaymentDetail.InstrumentNumber = "";
                }

                Context.PaymentDetail.Add(additionalAdvance.PaymentDetail);
            }


            Context.Cash_Paid.Add(cashPaid);
            Context.Invoice_Master.Add(invoiceMaster);
            Context.Ip_Account_Dtl.Add(ipAccountDtl);
            //return Context.SaveChanges() > 0;

            if (Context.SaveChanges() > 0)
                return new
                {
                    Success = true,
                    mrno = admission.Mr_No,
                    uin = admission.UIN,
                    ipano = admission.Ipa_No,
                    pname = regmaster.Patient_Name,
                    dist = regmaster.District,
                    country = regmaster.Country,
                    sex = regmaster.Sex,
                    clinic = clinic.Clinic_Code,
                    state = regmaster.State,
                    floor = crm.Floor_Code,
                    room = admission.Room_No,
                    receipt = admission.Receipt_No,
                    street = regmaster.Street_Locality,
                    town = regmaster.Town_City,
                    teluk = regmaster.Taluk,
                    mobile = regmaster.Police_Station,
                    landline = regmaster.Phone,
                    email = regmaster.Email_Id,
                    advnaceamt = cashPaid.Fees_Paid,
                    advancereceived = advancereceived.Fees_Paid,
                    age = additionalAdvance.Age,
                    advanbcerecipt = advancereceived.Receipt_NO,
                    advanbcedate = advancereceived.Date,
                    kin = regmaster.Next_Of_Kin,
                    pin = regmaster.Pincode,
                    paymode = modepay.Paymode_Code,
                    eye = admission.Eye,
                };

            else return new
            {
                Success = false,
                message = "Details not saved!"
            };
        }

        public dynamic GetRoomDetails(string roomType, int siteId, string paytype)
        {



            if (paytype == " SITE02")
            {


                var GetFreeRoomDetails = new List<RoomTypeDetails>();
                GetFreeRoomDetails = (from CRS in Context.Free_Accommodations_Master_New.Where(u => u.Siteid == siteId && u.Occupy_Flag_Code == "OFC003" && u.Room_Type == roomType)
                                      join RM in Context.Room_Master on new { crsrt = CRS.Room_Type } equals new { crsrt = RM.Room_Type }
                                      join Tol in Context.TOILET_TYPE_MASTER on new { ctsa = CRS.Toilet_Type_Code } equals new { ctsa = Tol.TOILET_TYPE_CODE }
                                      join FM in Context.Floor_Master
                                      on CRS.Floor_Code equals FM.Floor_Code

                                      orderby CRS.Vacating_Time
                                      select new RoomTypeDetails
                                      {
                                          ROOM_NO = CRS.Room_No,
                                          FLOOR_DESCRIPTION = FM.Floor_Description,
                                          FLOOR_CODE = FM.Floor_Code,
                                          TOILET_DESCRIPTION = Tol.TOILET_DESCRIPTION,

                                          Status = "VACCANT",


                                          VACATING_TIME = CRS.Vacating_Time,
                                          ROOM_COST = Convert.ToDecimal(RM.Room_Cost),
                                      }).ToList();



                return GetFreeRoomDetails;
                //             var GetFreeRoomDetails = new List<FreeAccommodationsMasterNew>();

                //GetFreeRoomDetails = (from CRS in Context.Free_Accommodations_Master_New.Where(u => u.Siteid == siteId && u.Occupy_Flag_Code == "OFC003" && u.Room_Type == roomType)
                //                                   join RM in Context.Room_Master on new { crsrt = CRS.Room_Type } equals new { crsrt = RM.Room_Type }
                //                                   join Tol in Context.TOILET_TYPE_MASTER on new { ctsa = CRS.Toilet_Type_Code } equals new { ctsa = Tol.TOILET_TYPE_CODE }
                //                                   join FM in Context.Floor_Master
                //                                   on CRS.Floor_Code equals FM.Floor_Code

                //                                   orderby CRS.Vacating_Time
                //                                   select new RoomTypeDetails
                //                                   {
                //                                       ROOM_NO = CRS.Room_No,
                //                                       FLOOR_DESCRIPTION = FM.Floor_Description,
                //                                       FLOOR_CODE = FM.Floor_Code,
                //                                       TOILET_DESCRIPTION = Tol.TOILET_DESCRIPTION,

                //                                       Status = "VACCANT",


                //                                       VACATING_TIME = CRS.Vacating_Time,
                //                                       ROOM_COST = Convert.ToDecimal(RM.Room_Cost),
                //                                   }).ToList();

                //             return GetFreeRoomDetails;
            }
            else
            {






                var GetRoomDetailss = new List<RoomTypeDetails>();
                GetRoomDetailss = (from CRS in Context.Current_Room_Status.Where(u => u.Siteid == siteId && u.Occupy_Flag_Code == "OFC003" && u.Room_Type == roomType)
                                   join RM in Context.Room_Master on new { crsrt = CRS.Room_Type } equals new { crsrt = RM.Room_Type }
                                   join Tol in Context.TOILET_TYPE_MASTER on new { ctsa = CRS.Toilet_Type_Code } equals new { ctsa = Tol.TOILET_TYPE_CODE }
                                   join FM in Context.Floor_Master
                                   on CRS.Floor_Code equals FM.Floor_Code

                                   orderby CRS.Vacating_Time
                                   select new RoomTypeDetails
                                   {
                                       ROOM_NO = CRS.Room_No,
                                       FLOOR_DESCRIPTION = FM.Floor_Description,
                                       FLOOR_CODE = FM.Floor_Code,
                                       TOILET_DESCRIPTION = Tol.TOILET_DESCRIPTION,

                                       Status = "VACCANT",


                                       VACATING_TIME = CRS.Vacating_Time,
                                       ROOM_COST = Convert.ToDecimal(RM.Room_Cost),
                                   }).ToList();




                return GetRoomDetailss;
            }




        }

        public dynamic ToDBInterimRefund(InterimRefund interimRefund)
        {
            if (interimRefund.CashPaidDetails.Count() > 0 && interimRefund.RefundAmount > 0)
            {
                var cashPaid = new CashPaid();
                cashPaid.Operator_Code = interimRefund.Operator_ID;
                cashPaid.Module_Code = "MOD4";
                cashPaid.MR_NO = interimRefund.MR_No;
                cashPaid.IPA_NO = interimRefund.IPA_No;
                cashPaid.Receipt_NO = GenerateRunningCtrlNo("REFUND_NO", interimRefund.Site_ID);
                cashPaid.Fees_Paid = interimRefund.RefundAmount;
                cashPaid.Date = DateTime.Now;
                cashPaid.Transaction_Code = "REFND";
                cashPaid.Category_Code = "CATC001";
                cashPaid.Sysdate = DateTime.Now;
                cashPaid.OP_IP_Flag = "IP";
                cashPaid.Paymode_Code = "PC001";
                cashPaid.Outstanding = "N";
                cashPaid.Invoice_No = "RFNDINVOICE";
                cashPaid.UIN = interimRefund.UIN_No;
                cashPaid.SiteId = interimRefund.Site_ID;

                Context.Cash_Paid.Add(cashPaid);

                var accDtl = new IpAccountDtl();
                accDtl.Ipa_Sl_No = Convert.ToInt32(GenerateRunningCtrlNo("IPA_SL_NO", interimRefund.Site_ID));
                accDtl.Ipa_No = interimRefund.IPA_No;
                accDtl.Cash_Flow_Code = "500";
                accDtl.Tinvoice_No = "RFNDINVOICE";
                accDtl.Sysdate = DateTime.Now;
                accDtl.Date = DateTime.Now;
                accDtl.Cost_Value = Convert.ToDecimal(interimRefund.RefundAmount);
                accDtl.UIN = interimRefund.UIN_No;
                accDtl.SiteId = interimRefund.Site_ID;
                Context.Ip_Account_Dtl.Add(accDtl);
            }
            try
            {
                if (Context.SaveChanges() > 0)
                    return new
                    {
                        Success = true,
                        message = "Saved successfully!"
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
        public InterimRefund GetInterimRefund(string ipa_No, bool isIpaNo, int siteId)
        {
            var IPA_UIN = "";
            var interimRefund = new InterimRefund();

            if (isIpaNo == true)
                IPA_UIN = Context.Ip_Admission.Where(u => u.SiteId == siteId && u.Ipa_No == ipa_No).Select(u => u.Ipa_No).FirstOrDefault();
            else
                IPA_UIN = Context.Ip_Admission.Where(u => u.SiteId == siteId && u.UIN == ipa_No).Select(u => u.Ipa_No).FirstOrDefault();

            try
            {

                interimRefund = (from IPA in Context.Ip_Admission.Where(u => u.SiteId == siteId && u.Ipa_No == IPA_UIN)
                                     //interimRefund = (from IPA in Context.Ip_Admission.Where(u => u.Discharge_Status == "ADM" && u.Discharge_Date == null && u.SiteId == siteId && u.Ipa_No == IPA_UIN)
                                 join IPSD in Context.Ip_Surgery_Dtl
                                               on new { admsc = IPA.Surgery_Code, admipa = IPA.Ipa_No, IPA.SiteId } equals new { admsc = IPSD.Surgery_Code, admipa = IPSD.Ipa_No, IPSD.SiteId }
                                 join IPC in Context.IP_Clinic
                                 on new { admipa = IPA.Ipa_No, admuin = IPA.UIN, admsid = IPA.SiteId } equals new { admipa = IPC.Ipa_No, admuin = IPC.Uin, admsid = IPC.Siteid }
                                 join PRM in Context.Patient_Registration_Master
                                 on new { admmrno = IPA.Mr_No, admuin = IPA.UIN, admsid = IPA.SiteId } equals new { admmrno = PRM.MR_NO, admuin = PRM.UIN, admsid = PRM.SiteId }
                                 join DM in Context.District_Master
                                 on new { prmdcode = PRM.District } equals new { prmdcode = DM.District_Code }
                                 join SM in Context.State_Master
                                 on new { prmscode = PRM.State } equals new { prmscode = SM.State_Code }
                                 orderby IPA.Mr_No.FirstOrDefault()
                                 select new InterimRefund
                                 {
                                     IPA_No = IPA.Ipa_No,
                                     UIN_No = IPA.UIN,
                                     Site_ID = IPA.SiteId,
                                     Operator_ID = IPA.Operator_Code,
                                     Category_Code = IPA.Category_Code,
                                     Clinic_Code = IPC.Clinic_Code,
                                     Doctor_Code = IPA.Doctor_Code,
                                     Eye_Code = IPA.Eye,
                                     MR_No = IPA.Mr_No,
                                     Surgery_Code = IPA.Surgery_Code,
                                     Surgery_Date = IPSD.Surgery_Date,

                                     Patient_Class = PRM.Patient_Class,
                                     Patient_DOB = PRM.Date_Of_Birth,

                                     Patient_Name = PRM.Patient_Name,
                                     NextOfKin = PRM.Next_Of_Kin,
                                     Patient_LastName = PRM.Patient_Lastname,
                                     Door = PRM.Door,
                                     Street = PRM.Street_Locality,
                                     Gender = PRM.Sex,
                                     DistrictDesc = DM.District_Name,
                                     StateDesc = SM.State_Name,
                                     Pincode = PRM.Pincode,
                                     DischargedStatus = IPA.Discharge_Status,
                                 }).FirstOrDefault();
                if (interimRefund.DischargedStatus == "ADM")
                {

                    switch (interimRefund.Gender)
                    {
                        case "M":
                            interimRefund.Gender = "Male";
                            break;
                        case "F":
                            interimRefund.Gender = "Female";
                            break;
                        case "T":
                            interimRefund.Gender = "Transgender";
                            break;
                    }

                    var now = DateTime.Now;
                    var dbo = interimRefund.Patient_DOB;
                    var age = now.Year - dbo.Year;
                    if (dbo > now.AddYears(-age))
                        age--;
                    interimRefund.Age = age;

                    interimRefund.NextOfKinDetails = $"{interimRefund.NextOfKin}, {interimRefund.MR_No}, {interimRefund.UIN_No}";
                    interimRefund.PatientDetail = $"{interimRefund.Age} yrs, {interimRefund.Gender}";
                    interimRefund.Address = $"{interimRefund.DistrictDesc}, {interimRefund.StateDesc}";


                    interimRefund.CashPaidDetails = Context.Cash_Paid
                    .Where(x => x.IPA_NO == interimRefund.IPA_No && x.UIN == interimRefund.UIN_No && x.SiteId == interimRefund.Site_ID && x.Module_Code == "MOD4" && (x.Transaction_Code == "REFND")).OrderBy(x => x.Receipt_NO).ThenBy(x => x.Sysdate.Date)
                    .Select(x => new CashPaidDetails
                    {
                        Source = x.Transaction_Code == "REFND" ? "Additional Advance" : "Refund",
                        Date = x.Sysdate.ToString("dd/MM/yyyy"),
                        Amount = x.Fees_Paid,
                        TransationCode = x.Transaction_Code
                    }).ToList();

                    if (interimRefund.CashPaidDetails.FirstOrDefault() != null)
                    {
                        interimRefund.CashPaidDetails.FirstOrDefault().Source = "Refund";
                        //interimRefund.TotalAmount = interimRefund.CashPaidDetails.Where(x => x.TransationCode == "RECPT").Sum(x => x.Amount);
                        //interimRefund.TotalAdmissionAmt = interimRefund.CashPaidDetails.Where(x => x.TransationCode == "RECPT").Sum(x => x.Amount);
                        interimRefund.TotalRefundAmt = interimRefund.CashPaidDetails.Where(x => x.TransationCode == "REFND").Sum(x => x.Amount);
                    }
                    //return interimRefund;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);

            }
            return interimRefund;
        }

        public Patient_Admission GetpopDetails(string MR_NO, int? siteId = null)
        {
            var patientAdmision = new Patient_Admission();
            patientAdmision.IPAdmission = new IPAdmission();
            patientAdmision.Master = new PatientRegistrationMaster();
            patientAdmision.IPAdmission = new IPAdmission();
            patientAdmision.CashPaid = new List<CashPaid>();
            patientAdmision.PaymentDetail = new PaymentDetail();

            patientAdmision.Master = Context.Patient_Registration_Master.FirstOrDefault(x => x.MR_NO == MR_NO && x.SiteId == siteId);
            if (patientAdmision.Master != null)
            {
                try
                {
                    patientAdmision.PatientAdmissionDetails = Context.Ip_Admission.Where(x => x.Mr_No == MR_NO && x.SiteId == siteId).Select(x => new PatientAdmissionDetails { IPANo = x.Ipa_No, Admission_Date = x.Admission_Date }).ToList();

                    patientAdmision.IPAdmission = Context.Ip_Admission.FirstOrDefault(x => x.Mr_No == MR_NO && x.SiteId == siteId);
                    patientAdmision.Master.Taluk = Context.Taluk_Master.Where(x => x.Taluk_Code == patientAdmision.Master.Taluk).Select(x => x.Taluk_Name).FirstOrDefault();
                    patientAdmision.Master.District = Context.District_Master.Where(x => x.District_Code == patientAdmision.Master.District).Select(x => x.District_Name).FirstOrDefault();
                    patientAdmision.Master.State = Context.State_Master.Where(x => x.State_Code == patientAdmision.Master.State).Select(x => x.State_Name).FirstOrDefault();
                    patientAdmision.SurgeryCostDetail = Context.Surgery_Cost_Detail.FirstOrDefault(x => x.Surgery_Code == patientAdmision.SurgeryCostDetail.Surgery_Code);

                    var now = DateTime.Now;
                    var dbo = patientAdmision.Master.Date_Of_Birth;
                    var age = now.Year - dbo.Year;
                    if (dbo > now.AddYears(-age))
                        age--;

                    patientAdmision.Age = age;
                    if (patientAdmision.Master.Sex == "M")
                    {
                        patientAdmision.Master.Sex = "Male";
                    }
                    else if (patientAdmision.Master.Sex == "F")
                    {
                        patientAdmision.Master.Sex = "FeMale";
                    }

                    patientAdmision.IPANo = GenerateRunningCtrlNo("MR_NO", siteId);
                    patientAdmision.AdmissionType = "N";

                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                }

            }
            return patientAdmision;
        }


        //---- Reprint ---


        public dynamic GetipaDetails(string ipa_no, int? siteId = null)
        {

            var patientAdmission = new Patient_Admission();
            patientAdmission.CashPaid = new List<CashPaid>();

            patientAdmission.IPAdmission = Context.Ip_Admission.FirstOrDefault(x => x.Ipa_No == ipa_no && x.SiteId == siteId);
            patientAdmission.IPClinic=Context.IP_Clinic.FirstOrDefault(x => x.Ipa_No == ipa_no);
            if (patientAdmission != null)
            {
                //patientAdmission.Room_No = Context.Current_Room_Status.Where(x => x.Room_No == patientAdmission.IPAdmission.Room_No).Select(x => x.Room_No).FirstOrDefault();


                patientAdmission.Room_No = Context.Ip_Admission.Where(x => x.Room_No == patientAdmission.IPAdmission.Room_No).Select(x => x.Room_No).FirstOrDefault();

                patientAdmission.IPAdmission.Admission_Date = Context.Ip_Admission.Where(x => x.Ipa_No == patientAdmission.IPAdmission.Ipa_No).Select(x => x.Admission_Date).FirstOrDefault();

                patientAdmission.uin = Context.Ip_Admission.Where(x => x.UIN == patientAdmission.IPAdmission.UIN).Select(x => x.UIN).FirstOrDefault();
                patientAdmission.Patient_Name = Context.Patient_Registration_Master.Where(x => x.UIN == patientAdmission.IPAdmission.UIN).Select(x => x.Patient_Name).FirstOrDefault();
                patientAdmission.Mr_No = Context.Ip_Admission.Where(x => x.Mr_No == patientAdmission.IPAdmission.Mr_No).Select(x => x.Mr_No).FirstOrDefault();
                patientAdmission.date = Context.Ip_Admission.Where(x => x.Admission_Date == patientAdmission.IPAdmission.Admission_Date).Select(x => x.Admission_Date).FirstOrDefault();
                patientAdmission.sex = Context.Patient_Registration_Master.Where(x => x.UIN == patientAdmission.IPAdmission.UIN).Select(x => x.Sex).FirstOrDefault();
                patientAdmission.birth = Context.Patient_Registration_Master.Where(x => x.UIN == patientAdmission.IPAdmission.UIN).Select(x => x.Date_Of_Birth).FirstOrDefault();
                patientAdmission.corporatecode = Context.Corporate_Preauth.Where(x => x.UIN == patientAdmission.IPAdmission.UIN && x.SiteId == patientAdmission.IPAdmission.SiteId).Select(x => x.Corporate).FirstOrDefault();
                patientAdmission.corporatename = Context.Corporate_Master.Where(x => x.Corporate_Code == patientAdmission.corporatecode).Select(x => x.Corporate_Name).FirstOrDefault();

                patientAdmission.IPAdmission.Receipt_No = Context.Ip_Admission.Where(x => x.Ipa_No == ipa_no && x.SiteId == siteId).Select(x => x.Receipt_No).FirstOrDefault();

                patientAdmission.AdmissionAmount = Context.Cash_Paid.Where(x => x.Module_Code == "MOD4" && x.IPA_NO == patientAdmission.IPAdmission.Ipa_No && x.SiteId == patientAdmission.IPAdmission.SiteId).Select(x => x.Fees_Paid).FirstOrDefault();
                patientAdmission.FloorCode = Context.Current_Room_Status.Where(x => x.Room_No == patientAdmission.IPAdmission.Room_No).Select(x => x.Floor_Code).FirstOrDefault();
                patientAdmission.FloorName = Context.Floor_Master.Where(x => x.Floor_Code == patientAdmission.FloorCode).Select(x => x.Floor_Description).FirstOrDefault();
                patientAdmission.paymode = Context.Cash_Paid.Where(x => x.UIN == patientAdmission.IPAdmission.UIN && x.SiteId == patientAdmission.IPAdmission.SiteId).Select(x => x.Paymode_Code).FirstOrDefault();
                patientAdmission.PaymodeDescription = Context.Mode_Of_Payment.Where(x => x.Paymode_Code == patientAdmission.paymode).Select(x => x.Description).FirstOrDefault();
                patientAdmission.eye = Context.Ip_Admission.Where(x => x.Eye == patientAdmission.IPAdmission.Eye).Select(x => x.Eye).FirstOrDefault();

                patientAdmission.surgeryname = Context.ICD_Code_Master.Where(x => x.Icd_Code == patientAdmission.IPAdmission.Surgery_Code).Select(x => x.Icd_Description).FirstOrDefault();


                //patientAdmission.ClinicCode = Context.Location_Master.Where(x => x.Location_Code == patientAdmission.IPClinic.Clinic_Code).Select(x => x.Location_Name).FirstOrDefault();

                patientAdmission.ClinicCode = Context.Icd_Code_Spec_Group_Code_Master.Where(x => x.Icd_Code_Speciality_Group_Code == patientAdmission.IPClinic.Clinic_Code).Select(x => x.Icd_Code_Speciality_Group_Desc).FirstOrDefault();

                patientAdmission.receiptdetails = (from cp in Context.Cash_Paid.Where(x => x.UIN == patientAdmission.IPAdmission.UIN)
                                                   join sc in Context.Service_Category on cp.Category_Code equals sc.Category_Code
                                                   join mp in Context.Mode_Of_Payment on cp.Paymode_Code equals mp.Paymode_Code
                                                   where cp.Module_Code == "MOD4"
                                                   select new Rdetails
                                                   {
                                                       Rno = cp.Receipt_NO,
                                                       Amountpaid = cp.Fees_Paid,
                                                       Descrip = sc.Category_Description,
                                                       Paymode = mp.Description,
                                                   }).ToList();


                var now = DateTime.Now;
                var dbo = patientAdmission.birth;
                var age = now.Year - dbo.Year;
                if (dbo > now.AddYears(-age))
                    age--;

                patientAdmission.Age = age;

            }
            return new
            {
                Rcedetails = patientAdmission.receiptdetails,
                Rectotal = patientAdmission.receiptdetails.Sum(x => x.Amountpaid),
                uins = patientAdmission.uin,
                names = patientAdmission.Patient_Name,
                //mnos = patientAdmission.Mr_No,

               last3mrno = patientAdmission.Mr_No.Substring(patientAdmission.Mr_No.Length - 3), 
             firstsetmrno = patientAdmission.Mr_No.Substring(0, patientAdmission.Mr_No.Length - 3),
                mnos = patientAdmission.Mr_No.Substring(0, patientAdmission.Mr_No.Length - 3) + "-" + patientAdmission.Mr_No.Substring(patientAdmission.Mr_No.Length - 3),


            addate = patientAdmission.date,
                sexs = patientAdmission.sex,
                birt = patientAdmission.birth,
                ages = patientAdmission.Age,
                ccode = patientAdmission.corporatecode,
                cname = patientAdmission.corporatename,

                adddate = patientAdmission.IPAdmission.Admission_Date,

                rooom = patientAdmission.Room_No,
                receipt = patientAdmission.IPAdmission.Receipt_No,
                clinic = patientAdmission.ClinicCode,
                amounts = patientAdmission.AdmissionAmount,
                floors = patientAdmission.FloorName,
                paymode = patientAdmission.PaymodeDescription,
                eyes = patientAdmission.eye,
                surgerynames = patientAdmission.surgeryname,
            };
        }
















    }
}


