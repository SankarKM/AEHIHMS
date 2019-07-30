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
    public class PatientDischargeRepository : RepositoryBase<Patient_Discharge>, IPatientDischargeRepository
    {
        private readonly IHMSContext _context;
        public PatientDischargeRepository(IHMSContext context) : base(context)
        {
            _context = context;
        }
        public Patient_Discharge GetPatientDetails(string UIN, int? emrSiteId = null) 

        {

            var patientDischarge = new Patient_Discharge();
            patientDischarge.CashPaid = new List<CashPaid>();
            patientDischarge.SubsidyAllocated = new List<SubsidyAllocated>();
            patientDischarge.PatientRegistrationDetail = new PatientRegistrationDetail();
            patientDischarge.Admission = new IPAdmission();
            patientDischarge.CRooms = new CurrentRoomStatus();
            patientDischarge.ipCashFlowMaster = new IpCashFlowMaster();
            patientDischarge.IPAC = new List<IpAccountDtl>();
            patientDischarge.Corporate = new CorporateCases();
            patientDischarge.PatientCounselNew = new PatientCounselNew();
            patientDischarge.Denomination = new List<DenominationDetails>();
            patientDischarge.Admission = Context.Ip_Admission.FirstOrDefault(x => x.UIN == UIN && x.SiteId == emrSiteId && x.Discharge_Status != "DIS");
            patientDischarge.DischargeStatus = Context.Ip_Admission.Where(x => x.UIN == UIN && x.SiteId == emrSiteId).OrderByDescending(x => x.Admission_Date).Select(x => x.Discharge_Status).FirstOrDefault(); 

            if (patientDischarge.Admission != null)
            {

                patientDischarge.CorporatePreauth = Context.Corporate_Preauth.FirstOrDefault(x => x.UIN == UIN && x.SiteId == emrSiteId);

                if (patientDischarge.CorporatePreauth != null)

                {

                    patientDischarge.CorporatePreauth = Context.Corporate_Preauth.FirstOrDefault(x => x.UIN == UIN && x.SiteId == emrSiteId);
                    patientDischarge.Master = Context.Patient_Registration_Master.FirstOrDefault(x => x.UIN == patientDischarge.CorporatePreauth.UIN);
                    patientDischarge.Corporatemaster = Context.Corporate_Master.FirstOrDefault(x => x.Corporate_Code == patientDischarge.CorporatePreauth.Corporate);
                    patientDischarge.ICDcode = Context.ICD_Code_Master.FirstOrDefault(x => x.Icd_Code == patientDischarge.CorporatePreauth.Surgery);
                    patientDischarge.Description = Context.Preauth_Status.Where(x => x.Code == patientDischarge.CorporatePreauth.Status).Select(x => x.Description).FirstOrDefault();



                }

                try
                {

                    patientDischarge.Master = Context.Patient_Registration_Master.FirstOrDefault(x => x.UIN == UIN && x.SiteId == emrSiteId);

                    patientDischarge.Master.Taluk = Context.Taluk_Master.Where(x => x.Taluk_Code == patientDischarge.Master.Taluk).Select(x => x.Taluk_Name).FirstOrDefault();
                    patientDischarge.Master.District = Context.District_Master.Where(x => x.District_Code == patientDischarge.Master.District).Select(x => x.District_Name).FirstOrDefault();
                    patientDischarge.Master.State = Context.State_Master.Where(x => x.State_Code == patientDischarge.Master.State).Select(x => x.State_Name).FirstOrDefault();
                    var now = DateTime.Now;
                    var dbo = patientDischarge.Master.Date_Of_Birth;
                    var age = now.Year - dbo.Year;
                    if (dbo > now.AddYears(-age))
                        age--;
                    patientDischarge.Age = age;


                    if (patientDischarge.Master.Sex == "M")
                    {
                        patientDischarge.Master.Sex = "Male";
                    }
                    else if (patientDischarge.Master.Sex == "F")
                    {
                        patientDischarge.Master.Sex = "FeMale";
                    }


                    patientDischarge.Surgery = Context.Ip_Surgery_Dtl.FirstOrDefault(x => x.UIN == UIN && x.SiteId == emrSiteId);
                    patientDischarge.PatientCounselNew = Context.Patient_Counsel_New.FirstOrDefault(x => x.Uin == UIN && x.Siteid == emrSiteId);
                    patientDischarge.SurgeryCostDetail = Context.Surgery_Cost_Detail.FirstOrDefault(x => x.Surgery_Code == patientDischarge.Admission.Surgery_Code);
                    patientDischarge.NoofDays = Context.ICD_Code_Master.Where(x => x.Icd_Code == patientDischarge.Admission.Surgery_Code).Select(x => x.No_Of_Days).FirstOrDefault();
                    patientDischarge.PatientCounselNew = Context.Patient_Counsel_New.FirstOrDefault(x => x.Uin == patientDischarge.Admission.UIN);
                    if (patientDischarge.PatientCounselNew != null)
                    {

                        patientDischarge.AditionalProcedure = Context.TEST_REPOSITORY.Where(x => x.Test_Code == patientDischarge.PatientCounselNew.Clinicprocedure).Select(x => x.Test_Name).FirstOrDefault();

                    }

                 



                    var ChargeDetail = new List<ChargeDetail>();


                    patientDischarge.ChargeDetail = (from IAD in Context.Ip_Account_Dtl.Where(u => u.Ipa_No == patientDischarge.Admission.Ipa_No)
                                                     join ICFM in Context.IP_CASH_FLOW_MASTER.Where(x=> x.Cash_Flow_Code!="501")//x => x.Debit_Credit_Code == "CREDIT" &&
                                                     on IAD.Cash_Flow_Code equals ICFM.Cash_Flow_Code
                                                     orderby IAD.Date

                                                     select new ChargeDetail
                                                     {
                                                         Datee = IAD.Date.ToString("dd/MM/yyyy"),
                                                         Description = ICFM.Cash_Flow_Description,
                                                         Debit = IAD.Cost_Value,

                                                     }



                                                    ).ToList();

                    var total = patientDischarge.ChargeDetail.Sum(x => x.Debit);

                    patientDischarge.tot = total;

                    var RoomDetail = new List<RoomDetail>();

                    
                    patientDischarge.PayDetail = Context.Cash_Paid
                            .Where(x => x.IPA_NO == patientDischarge.Admission.Ipa_No && x.SiteId == patientDischarge.Admission.SiteId && x.Module_Code == "MOD4" && x.Category_Code == "CATC001" && (x.Transaction_Code == "RECPT")).OrderBy(x => x.Receipt_NO).ThenBy(x => x.Sysdate.Date)
                            .Select(x => new PayDetail
                            {
                                Source = x.Transaction_Code == "RECPT" ? "Admission" : "Interim Refund",
                                Date = x.Sysdate.ToString("dd/MM/yyyy"),
                                Amount = x.Fees_Paid,

                            }).ToList();

                    var admtotal = patientDischarge.PayDetail.Sum(x => x.Amount);
                    patientDischarge.AdmissionAmount = admtotal;

                 





                    if (patientDischarge.Admission.Room_Transfer == "Y")
                    {




                        patientDischarge.RoomDetail = (from RT in Context.Room_Transfer.Where(u => u.MR_NO == patientDischarge.Admission.Mr_No)
                                                       join RM in Context.Room_Master
                                                         //on new { crsrt = RT.FROM_TYPE } equals new { crsrt = RM.Room_Type }
                                                         on RT.FROM_TYPE equals RM.Room_Type
                                                       join CRS in Context.Room_Master
                                                       on RT.TO_TYPE equals CRS.Room_Type
                                                       //on RT.FROM_ROOM equals CRS.Room_No
                                                       orderby RT.OCCUPIED_DATE
                                                       select new RoomDetail
                                                       {
                                                           OcuupiedAt = RT.OCCUPIED_DATE,

                                                           ChangeDate = RT.CHANGE_DATE,

                                                           RoomNo = RT.FROM_ROOM,

                                                           ToRoomNo = RT.TO_ROOM,

                                                           Amount = Convert.ToString(RM.Room_Cost),

                                                           Amount2 = Convert.ToString(CRS.Room_Cost),


                                                       }



                                                       ).ToList();

                    }

                    else
                    {


                        patientDischarge.RoomDetail = (from CRS in Context.Current_Room_Status.Where(u => u.UIN == patientDischarge.Admission.UIN)
                                                       join RM in Context.Room_Master
                                                         //on new { crsrt = RT.FROM_TYPE } equals new { crsrt = RM.Room_Type }
                                                         on CRS.Room_Type equals RM.Room_Type
                                                       //join CRS in Context.Current_Room_Status
                                                       //on RT.FROM_ROOM equals CRS.Room_No

                                                       select new RoomDetail
                                                       {
                                                           OcuupiedAt = Convert.ToDateTime(CRS.Occupied_Date),

                                                           ChangeDate = DateTime.Now,

                                                           RoomNo = CRS.Room_No,

                                                           Amount = Convert.ToString(RM.Room_Cost),

                                                           



                                                       }



                                                       ).ToList();



                    }




                    if (patientDischarge.Admission.Room_Transfer != "Y")
                    {

                        var dates = new List<DateTime>();
                        var roomm = new List<string>();
                        var cost = new List<string>();

                        var lstRoomDetailsAndCost = new List<Roomlist>();

                        foreach (var item2 in patientDischarge.RoomDetail.ToList())
                        {


                            var t1 = DateTime.Now.TimeOfDay;

                            if (t1 > System.TimeSpan.Parse("13:00:00"))

                            {

                                for (var dt = item2.OcuupiedAt.Date; dt <= item2.ChangeDate.Date; dt = dt.AddDays(1))
                                {

                                    var RoomObject = new Roomlist();
                                    RoomObject.costroomdate = dt;
                                    RoomObject.costroomNo = item2.RoomNo;
                                    RoomObject.costroomcost = item2.Amount;

                                    lstRoomDetailsAndCost.Add(RoomObject);


                                }
                            }

                            else
                            {
                                for (var dt = item2.OcuupiedAt.Date; dt < item2.ChangeDate.Date; dt = dt.AddDays(1))
                                {

                                    var RoomObject = new Roomlist();
                                    RoomObject.costroomdate = dt;
                                    RoomObject.costroomNo = item2.RoomNo;
                                    RoomObject.costroomcost = item2.Amount;

                                    lstRoomDetailsAndCost.Add(RoomObject);


                                }
                            }


                        }


                        patientDischarge.RoomList = lstRoomDetailsAndCost;

                    }


                    else
                    {


                        var dates = new List<DateTime>();
                        var roomm = new List<string>();
                        var cost = new List<string>();

                        var lstRoomDetailsAndCost = new List<Roomlist>();

                        patientDischarge.firstroomtransfer = Context.Room_Transfer.Where(x => x.IPA_NO == patientDischarge.Admission.Ipa_No && x.NOFTRANS == 1).AsNoTracking().FirstOrDefault();

                        var FirstRoom = Context.Room_Master.Where(x => x.Room_Type == patientDischarge.firstroomtransfer.FROM_TYPE).AsNoTracking().FirstOrDefault();

                        if (patientDischarge.firstroomtransfer.NOFTRANS == 1)
                        {
                            for (var dt = patientDischarge.firstroomtransfer.OCCUPIED_DATE.Date; dt < patientDischarge.firstroomtransfer.CHANGE_DATE.Date; dt = dt.AddDays(1))
                            {
                                var RoomObject = new Roomlist();
                                RoomObject.costroomdate = dt;
                                RoomObject.costroomNo = patientDischarge.firstroomtransfer.FROM_ROOM;
                                RoomObject.costroomcost = FirstRoom.Room_Cost;

                                lstRoomDetailsAndCost.Add(RoomObject);
                            }
                        }

                        patientDischarge.secondtransfer = Context.Room_Transfer.Where(x => x.IPA_NO == patientDischarge.Admission.Ipa_No && x.NOFTRANS == 2).AsNoTracking().FirstOrDefault();

                        if (patientDischarge.secondtransfer != null)
                        {
                            var secRoom = Context.Room_Master.Where(x => x.Room_Type == patientDischarge.secondtransfer.FROM_TYPE).AsNoTracking().FirstOrDefault();

                            if (patientDischarge.secondtransfer.NOFTRANS == 2)
                            {

                                for (var dt = patientDischarge.secondtransfer.OCCUPIED_DATE.Date; dt < patientDischarge.secondtransfer.CHANGE_DATE.Date; dt = dt.AddDays(1))
                                {
                                    var RoomObject = new Roomlist();
                                    RoomObject.costroomdate = dt;
                                    RoomObject.costroomNo = patientDischarge.secondtransfer.FROM_ROOM;
                                    RoomObject.costroomcost = secRoom.Room_Cost;
                                    lstRoomDetailsAndCost.Add(RoomObject);

                                }

                            }

                            var thirdRoom = Context.Room_Master.Where(x => x.Room_Type == patientDischarge.secondtransfer.TO_TYPE).AsNoTracking().FirstOrDefault();



                            var t1 = DateTime.Now.TimeOfDay;

                            if (t1 > System.TimeSpan.Parse("13:00:00"))

                            {

                                for (var dt = patientDischarge.secondtransfer.CHANGE_DATE.Date; dt <= DateTime.Now.Date; dt = dt.AddDays(1))
                                {
                                    var RoomObject = new Roomlist();
                                    RoomObject.costroomdate = dt;
                                    RoomObject.costroomNo = patientDischarge.secondtransfer.TO_ROOM;
                                    RoomObject.costroomcost = thirdRoom.Room_Cost;
                                    lstRoomDetailsAndCost.Add(RoomObject);

                                }
                            }

                            else
                            {

                                for (var dt = patientDischarge.secondtransfer.CHANGE_DATE.Date; dt < DateTime.Now.Date; dt = dt.AddDays(1))
                                {
                                    var RoomObject = new Roomlist();
                                    RoomObject.costroomdate = dt;
                                    RoomObject.costroomNo = patientDischarge.secondtransfer.TO_ROOM;
                                    RoomObject.costroomcost = thirdRoom.Room_Cost;
                                    lstRoomDetailsAndCost.Add(RoomObject);

                                }


                            }



                        }

                        else
                        {

                            var lastroom = Context.Room_Master.Where(x => x.Room_Type == patientDischarge.firstroomtransfer.TO_TYPE).AsNoTracking().FirstOrDefault();



                            var t1 = DateTime.Now.TimeOfDay;

                            if (t1 > System.TimeSpan.Parse("13:00:00"))

                            {

                                for (var dt = patientDischarge.firstroomtransfer.CHANGE_DATE.Date; dt <= DateTime.Now.Date; dt = dt.AddDays(1))
                                {
                                    var RoomObject = new Roomlist();
                                    RoomObject.costroomdate = dt;
                                    RoomObject.costroomNo = patientDischarge.firstroomtransfer.TO_ROOM;
                                    RoomObject.costroomcost = lastroom.Room_Cost;
                                    lstRoomDetailsAndCost.Add(RoomObject);

                                }
                            }
                            else
                            {
                                for (var dt = patientDischarge.firstroomtransfer.CHANGE_DATE.Date; dt < DateTime.Now.Date; dt = dt.AddDays(1))
                                {
                                    var RoomObject = new Roomlist();
                                    RoomObject.costroomdate = dt;
                                    RoomObject.costroomNo = patientDischarge.firstroomtransfer.TO_ROOM;
                                    RoomObject.costroomcost = lastroom.Room_Cost;
                                    lstRoomDetailsAndCost.Add(RoomObject);

                                }
                            }

                        }


                        patientDischarge.RoomList = lstRoomDetailsAndCost;

                    }



                    if (patientDischarge.Admission.Room_Transfer == "Y")
                    {

                        var rt = new RoomTransfer();

                        patientDischarge.firstroomtransfer = Context.Room_Transfer.Where(x => x.IPA_NO == patientDischarge.Admission.Ipa_No && x.NOFTRANS == 1).AsNoTracking().FirstOrDefault();

                        var FirstRoom = Context.Room_Master.Where(x => x.Room_Type == patientDischarge.firstroomtransfer.FROM_TYPE).AsNoTracking().FirstOrDefault();

                        if (patientDischarge.firstroomtransfer.NOFTRANS == 1)
                        {
                            var ocupaid = Convert.ToDateTime(patientDischarge.firstroomtransfer.OCCUPIED_DATE).Date;
                            var current = Convert.ToDateTime(patientDischarge.firstroomtransfer.CHANGE_DATE).Date;
                            var anss = current.Subtract(ocupaid);
                            var resl = anss.TotalDays;
                            var TotalFday = resl;
                            patientDischarge.totalamt3 = TotalFday * FirstRoom.Room_Cost;
                        }


                        patientDischarge.secondtransfer = Context.Room_Transfer.Where(x => x.IPA_NO == patientDischarge.Admission.Ipa_No && x.NOFTRANS == 2).AsNoTracking().FirstOrDefault();

                        if (patientDischarge.secondtransfer != null)
                        {
                            var secRoom = Context.Room_Master.Where(x => x.Room_Type == patientDischarge.secondtransfer.FROM_TYPE).AsNoTracking().FirstOrDefault();

                            if (patientDischarge.secondtransfer.NOFTRANS == 2)
                            {

                                var ocupaidtwo = Convert.ToDateTime(patientDischarge.secondtransfer.OCCUPIED_DATE).Date;
                                var currenttwo = Convert.ToDateTime(patientDischarge.secondtransfer.CHANGE_DATE).Date;
                                var anstwo = currenttwo.Subtract(ocupaidtwo);
                                var resltwo = anstwo.TotalDays;
                                var Totalsday = resltwo;
                                patientDischarge.totalamt = Totalsday * secRoom.Room_Cost;

                            }

                            var thirdRoom = Context.Room_Master.Where(x => x.Room_Type == patientDischarge.secondtransfer.TO_TYPE).AsNoTracking().FirstOrDefault();

                            var t1 = DateTime.Now.TimeOfDay;

                            if (t1 > System.TimeSpan.Parse("13:00:00"))

                            {

                                var ocupaidthree = Convert.ToDateTime(patientDischarge.secondtransfer.CHANGE_DATE).Date;
                                var currentthree = Convert.ToDateTime(DateTime.Now).Date;
                                var ansthree = currentthree.Subtract(ocupaidthree);
                                var reslthree = ansthree.TotalDays + 1;
                                var Totalsday1 = reslthree;
                                patientDischarge.totalamt1 = Totalsday1 * thirdRoom.Room_Cost;

                                patientDischarge.RoomCost = patientDischarge.totalamt + patientDischarge.totalamt1 + patientDischarge.totalamt3;                                                       

                            }

                            else
                            {
                                var ocupaidthree = Convert.ToDateTime(patientDischarge.secondtransfer.CHANGE_DATE).Date;
                                var currentthree = Convert.ToDateTime(DateTime.Now).Date;
                                var ansthree = currentthree.Subtract(ocupaidthree);
                                var reslthree = ansthree.TotalDays;
                                var Totalsday1 = reslthree;
                                patientDischarge.totalamt1 = Totalsday1 * thirdRoom.Room_Cost;

                                patientDischarge.RoomCost = patientDischarge.totalamt + patientDischarge.totalamt1 + patientDischarge.totalamt3;


                            }

                        }


                        else
                        {

                            var lastroom = Context.Room_Master.Where(x => x.Room_Type == patientDischarge.firstroomtransfer.TO_TYPE).AsNoTracking().FirstOrDefault();


                            var t1 = DateTime.Now.TimeOfDay;

                            if (t1 > System.TimeSpan.Parse("13:00:00"))
                            {

                                var ocupaidone = Convert.ToDateTime(patientDischarge.firstroomtransfer.CHANGE_DATE).Date;
                                var currentone = Convert.ToDateTime(DateTime.Now).Date;
                                var ansone = currentone.Subtract(ocupaidone);
                                var reslone = ansone.TotalDays + 1;
                                var Totalsday2 = reslone;
                                patientDischarge.totalamt2 = Totalsday2 * lastroom.Room_Cost;

                                patientDischarge.RoomCost = patientDischarge.totalamt2 + patientDischarge.totalamt3;
                            }

                            else
                            {
                                var ocupaidone = Convert.ToDateTime(patientDischarge.firstroomtransfer.CHANGE_DATE).Date;
                                var currentone = Convert.ToDateTime(DateTime.Now).Date;
                                var ansone = currentone.Subtract(ocupaidone);
                                var reslone = ansone.TotalDays;
                                var Totalsday2 = reslone;
                                patientDischarge.totalamt2 = Totalsday2 * lastroom.Room_Cost;

                                patientDischarge.RoomCost = patientDischarge.totalamt2 + patientDischarge.totalamt3;


                            }




                        }

                    }

                    else
                    {
                        patientDischarge.CRooms = Context.Current_Room_Status.Where(x => x.UIN == patientDischarge.Admission.UIN && x.Siteid == patientDischarge.Admission.SiteId).FirstOrDefault();
                        patientDischarge.RoomMaster = Context.Room_Master.Where(x => x.Room_Type == patientDischarge.CRooms.Room_Type).FirstOrDefault();


                        var t1 = DateTime.Now.TimeOfDay;

                        if (t1 > System.TimeSpan.Parse("13:00:00"))
                        {

                            var ocdt = Convert.ToDateTime(patientDischarge.CRooms.Occupied_Date).Date;
                            var chdt = DateTime.Now.Date;
                            var ans = chdt.Subtract(ocdt);
                            var res = ans.TotalDays + 1;
                            patientDischarge.TotalDays = Convert.ToInt32(res);
                            var amt = patientDischarge.TotalDays * patientDischarge.RoomMaster.Room_Cost;
                            patientDischarge.RoomCost = amt;
                        }

                        else
                        {
                            var ocdt = Convert.ToDateTime(patientDischarge.CRooms.Occupied_Date).Date;
                            var chdt = DateTime.Now.Date;
                            var ans = chdt.Subtract(ocdt);
                            var res = ans.TotalDays;
                            patientDischarge.TotalDays = Convert.ToInt32(res);
                            var amt = patientDischarge.TotalDays * patientDischarge.RoomMaster.Room_Cost;
                            patientDischarge.RoomCost = amt;

                        }

                    }


                    patientDischarge.Roomtotalamt = Convert.ToDecimal(patientDischarge.RoomCost);




                }




                catch (Exception ex)
                {


                }
            }
            return patientDischarge;
        }

        public Patient_Discharge GetTestCostDetails(string TestCode)

        {
            var TestCodeDetails = new Patient_Discharge();

            TestCodeDetails.TestRepository = Context.TEST_REPOSITORY.Where(x => x.Test_Code == TestCode).FirstOrDefault();

            TestCodeDetails.ipCashFlowMaster = Context.IP_CASH_FLOW_MASTER.Where(x => x.Account_Head == TestCodeDetails.TestRepository.Account_Head).FirstOrDefault();

            return TestCodeDetails;
        }

        public dynamic updatePatientDischarge(Patient_Discharge patientDischarge)
        {
           











            var CurrentRoomStatus = Context.Current_Room_Status.Where(x => x.UIN == patientDischarge.Admission.UIN && x.Siteid == patientDischarge.Admission.SiteId).ToList();
                if (CurrentRoomStatus.Count() > 0)
                {
                    CurrentRoomStatus.All(x => { x.Vacating_Time = DateTime.Now; x.Occupy_Flag_Code = "OFC006"; x.UIN = ""; return true; });
                    Context.Current_Room_Status.UpdateRange(CurrentRoomStatus);
                }
                var ipaccount = Context.IP_Account.Where(x => x.Ipa_No == patientDischarge.Admission.Ipa_No ).FirstOrDefault();
                if (ipaccount != null)
                {
                    ipaccount.Status = "DIS";
                    Context.Entry(ipaccount).State = EntityState.Modified;
                }









                






           // if (patientDischarge.CashPaid.Count() > 0 )
            //{
                var subsidyDetails = patientDischarge.CashPaid.FirstOrDefault();
                //if (subsidyDetails.Category_Code == "CATC001" )  //==1    && patientDischarge.CashPaid.Count() == 1
                //{
                //}

                var invoiceNo = GenerateRunningCtrlNo("INVOICE_NO", 1);
                if (invoiceNo == null || invoiceNo == "")
                {
                    return new
                    {
                        Success = false,
                        Message = "INVOICE_NO is not available in Running Control Table"
                    };
                }
                patientDischarge.Invoice = new InvoiceMaster();

                foreach (var item in patientDischarge.CashPaid.ToList())
                {
                    if (item.Category_Code == "CATC004")
                    {
                        patientDischarge.Invoice.Invoice_No = invoiceNo;
                        patientDischarge.Invoice.Invoice_Date = DateTime.Now;
                        patientDischarge.Invoice.Module_Code = patientDischarge.CashPaid.FirstOrDefault().Module_Code;
                        patientDischarge.Invoice.Invoice_Value = float.Parse((0).ToString());
                        patientDischarge.Invoice.Invoice_Received_Value =  float.Parse((0).ToString());
                        patientDischarge.Invoice.SiteId = patientDischarge.Admission.SiteId;
                        patientDischarge.Invoice.UIN = patientDischarge.Master.UIN;
                        patientDischarge.Invoice.MR_NO = patientDischarge.Master.MR_NO;
                        Context.Invoice_Master.Add(patientDischarge.Invoice);
                    }
                    else
                    {


                        patientDischarge.Invoice.Invoice_No = invoiceNo;
                        patientDischarge.Invoice.Invoice_Date = DateTime.Now;
                        patientDischarge.Invoice.Module_Code = patientDischarge.CashPaid.FirstOrDefault().Module_Code;
                        patientDischarge.Invoice.Invoice_Value = float.Parse(patientDischarge.CashPaid.Sum(x => x.Fees_Paid).ToString());
                        patientDischarge.Invoice.Invoice_Received_Value = patientDischarge.Invoice.Invoice_Value;
                        patientDischarge.Invoice.SiteId = patientDischarge.Admission.SiteId;
                        patientDischarge.Invoice.UIN = patientDischarge.Master.UIN;
                        patientDischarge.Invoice.MR_NO = patientDischarge.Master.MR_NO;
                        //patientAdmission.Invoice.Cash_Paid = patientAdmission.CashPaid;
                        Context.Invoice_Master.Add(patientDischarge.Invoice);

                    }
                  

                }


            //}


            foreach (var item in patientDischarge.CashPaid.ToList())
            {
                if (item.Category_Code == "CATC004")
                {


                    patientDischarge.Corporate.Corporate_Case_No = GenerateRunningCtrlNo("CORPORATE_CASE_NO", 1);
                    if (patientDischarge.Corporate.Corporate_Case_No == null || patientDischarge.Corporate.Corporate_Case_No == "")
                    {
                        return new
                        {
                            Success = false,
                            Message = "Corporate Cases  CORPORATE_CASE_NO is not available in Running Control Table"
                        };
                    }
                    patientDischarge.Corporate.Corporate_Case_No = patientDischarge.Corporate.Corporate_Case_No;
                    patientDischarge.Corporate.UIN = patientDischarge.Admission.UIN;
                    patientDischarge.Corporate.SiteId = patientDischarge.Admission.SiteId;
                    patientDischarge.Corporate.IP_No = patientDischarge.Admission.Ipa_No;
                    patientDischarge.Corporate.Invoice_No = patientDischarge.Invoice.Invoice_No;
                    //patientDischarge.Corporate.Cor_Sl_No = patientDischarge.CorSlNo;
                    patientDischarge.Corporate.Visit_Date = DateTime.Now;
                    patientDischarge.Corporate.Sysdate = DateTime.Now;
                    patientDischarge.Corporate.CreatedUTC = DateTime.UtcNow;
                    patientDischarge.Corporate.Bill_No = GenerateRunningCtrlNo("COR_BILL_NO", 1);
                    patientDischarge.Corporate.Module_Code = "MOD5";
                    patientDischarge.Corporate.MR_NO = patientDischarge.Admission.Mr_No;
                    Context.Corporate_Cases.Add(patientDischarge.Corporate);


                }

            }

            

                foreach (var item in patientDischarge.CashPaid.ToList())
                {
                    if (item.Fees_Paid > 0)
                    {

                        var cashpaid = new CashPaid();

                        cashpaid.MR_NO = patientDischarge.Master.MR_NO;
                        cashpaid.UIN = patientDischarge.Master.UIN;
                        cashpaid.Date = DateTime.Now.Date;
                        cashpaid.Sysdate = DateTime.Now;
                        cashpaid.Invoice_No = patientDischarge.Invoice.Invoice_No;
                        cashpaid.OP_IP_Flag = "IP";
                        cashpaid.Fees_Paid = item.Fees_Paid;
                        cashpaid.IPA_NO = patientDischarge.Admission.Ipa_No;
                        cashpaid.Module_Code = item.Module_Code;
                        cashpaid.Operator_Code = item.Operator_Code;
                                              
                            if (patientDischarge.Master.Patient_Class == "SITE01")
                            {

                                if (patientDischarge.ispayable == 2)
                                {
                                     cashpaid.Transaction_Code = "REFND";
                                }
                                else
                                {
                                     cashpaid.Transaction_Code = item.Transaction_Code;
                                }
                            if (item.Category_Code == "CATC004")
                            {
                                cashpaid.Receipt_NO = patientDischarge.Corporate.Bill_No;
                                patientDischarge.Admission.Receipt_No = cashpaid.Receipt_NO;
                                cashpaid.Category_Code = item.Category_Code;
                                cashpaid.Transaction_Code = item.Transaction_Code;
                        }
                            else
                            {

                            if (patientDischarge.Corporate.Corporate_Case_No != null)
                            {
                                cashpaid.Category_Code = "CATC004";

                            }
                            else
                            {
                                cashpaid.Category_Code = item.Category_Code;
                            }


                            cashpaid.Receipt_NO = GenerateRunningCtrlNo("IP_RECEIPT_NO", 1);
                            patientDischarge.Admission.Receipt_No = cashpaid.Receipt_NO;
                              
                            }

                        }
                            else
                            {
                                cashpaid.Receipt_NO = GenerateRunningCtrlNo("FRES_RECEIPT_NO", 1);
                                cashpaid.Category_Code = "CATC002";
                                cashpaid.Transaction_Code = item.Transaction_Code;
                            }
                        
                  
                        cashpaid.Paymode_Code = item.Paymode_Code;
                        cashpaid.Outstanding = item.Outstanding;

                        cashpaid.SiteId = patientDischarge.Admission.SiteId;

                        Context.Cash_Paid.AddRange(cashpaid);

                        
                    }
                }

                if (patientDischarge.PaymentDetail != null)
                {
                    patientDischarge.PaymentDetail.UIN = patientDischarge.Master.UIN;
                if (patientDischarge.Invoice == null)
                {
                    patientDischarge.PaymentDetail.Invoice_No = "0";
                }
                else
                {
                    patientDischarge.PaymentDetail.Invoice_No = patientDischarge.Invoice.Invoice_No;
                }
                   
                    patientDischarge.PaymentDetail.Receipt_No = patientDischarge.Admission.Receipt_No;//patientAdmission.CashPaid.Select(x => x.Receipt_NO).FirstOrDefault();
                    patientDischarge.PaymentDetail.CreatedUTC = DateTime.UtcNow;
                    patientDischarge.PaymentDetail.Id = 0;
                    patientDischarge.PaymentDetail.MR_NO = patientDischarge.Master.MR_NO;
                    patientDischarge.PaymentDetail.SiteId = patientDischarge.Admission.SiteId;
                    patientDischarge.PaymentDetail.UIN = patientDischarge.Master.UIN;
                    if (patientDischarge.PaymentDetail.BankName == null)
                    {
                        patientDischarge.PaymentDetail.BankName = "";
                    }
                    if (patientDischarge.PaymentDetail.Branch == null)
                    {
                        patientDischarge.PaymentDetail.Branch = "";
                    }
                    if (patientDischarge.PaymentDetail.InstrumentNumber == null)
                    {
                        patientDischarge.PaymentDetail.InstrumentNumber = "";
                    }

                    Context.PaymentDetail.Add(patientDischarge.PaymentDetail);

                }


            




            foreach (var item in patientDischarge.SubsidyAllocated.ToList())
            {
                if (item.Subsidy_Granted > 0)
                {
                    var subsidy = new SubsidyAllocated();
                    subsidy.Subsidy_Case_No = GenerateRunningCtrlNo("SUBSIDY_CASE_NO", 1);
                    subsidy.Module_Code = item.Module_Code;
                    subsidy.MR_NO = patientDischarge.Admission.Mr_No;
                    subsidy.Authority_Code = item.Authority_Code;
                    subsidy.Subsidy_Granted = item.Subsidy_Granted;
                    subsidy.Visit_Date = DateTime.Now.Date;
                    subsidy.IP_No = patientDischarge.Admission.Ipa_No;
                    subsidy.Sysdate = DateTime.Now;
                    subsidy.Receipt_No = patientDischarge.Admission.Receipt_No;
                    if(patientDischarge.Invoice ==null)
                    {
                        subsidy.Invoice_No = "0";
                    }
                    else
                    {
                        subsidy.Invoice_No = patientDischarge.Invoice.Invoice_No;
                    }
                    
                    subsidy.Remarks = item.Remarks;
                    subsidy.Reason = item.Reason;
                    subsidy.UIN = patientDischarge.Admission.UIN;
                    subsidy.SiteId = patientDischarge.Admission.SiteId;
                    Context.Subsidy_Allocated.Add(subsidy);

                }

            }






          

            var ipadmissions = Context.Ip_Admission.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).FirstOrDefault();
            var ipano = ipadmissions.Ipa_No;
            if (ipadmissions != null)
            {
                ipadmissions.Discharge_Status = "DIS";
                ipadmissions.Discharge_Date = DateTime.Now;
                if(patientDischarge.PaymentDetail != null && patientDischarge.PaymentDetail.Amount > 0)
                {
                    ipadmissions.Tinvoice_No = patientDischarge.Invoice.Invoice_No;
                }

                ipadmissions.Tinvoice_No = null;
                ipadmissions.Receipt_No = patientDischarge.Admission.Receipt_No;
                Context.Entry(ipadmissions).State = EntityState.Modified;

            }





            if (patientDischarge.IPAC.Count() > 0)
            {
                foreach (var item in patientDischarge.IPAC.ToList())

                {


                    var IpAccountDtl = new IpAccountDtl();
                    IpAccountDtl.Ipa_Sl_No = Convert.ToInt32(GenerateRunningCtrlNo("Ipa_Sl_No", 1));
                    IpAccountDtl.Ipa_No = patientDischarge.Admission.Ipa_No;
                    IpAccountDtl.Cash_Flow_Code = item.Cash_Flow_Code;
                    IpAccountDtl.Cost_Value = item.Cost_Value;
                    IpAccountDtl.UIN = patientDischarge.Admission.UIN;
                    IpAccountDtl.SiteId = patientDischarge.Admission.SiteId;
                    IpAccountDtl.Sysdate = DateTime.Now.Date;
                    IpAccountDtl.Date = DateTime.Now;
                    Context.Ip_Account_Dtl.AddRange(IpAccountDtl);

                }
            }

            Context.SaveChanges();



            var flowcodelistest = Context.Ip_Account_Dtl.Where(x => x.Ipa_No == patientDischarge.Admission.Ipa_No && x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).ToList();

            var flowcodelist = Context.Ip_Account_Dtl.Where(x => x.Ipa_No == patientDischarge.Admission.Ipa_No && x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId && x.Cost_Value >0)
            .GroupBy(f => new { f.Cash_Flow_Code, f.Ipa_No, f.UIN, f.SiteId })
            .Select(group => new {
                fee = group.Key,
                total = group.Sum(f => f.Cost_Value),
                uin = group.Key.UIN,
                Siteid = group.Key.SiteId,
                ipa_no = group.Key.Ipa_No,
                CashFlowCode = group.Key.Cash_Flow_Code
            }).ToList();


           

            if (flowcodelist.Count() > 0)
            {


                for (int i = 0; i < flowcodelist.Count; i++)
                {


                    var accountheadcode = Context.IP_CASH_FLOW_MASTER.Where(x => x.Cash_Flow_Code == flowcodelist[i].CashFlowCode).Select(x => x.Account_Head).FirstOrDefault();


                    
                    var IpAccountgrp = new IpAccountGrp();
                    IpAccountgrp.Ipa_No = flowcodelist[i].ipa_no;
                    IpAccountgrp.Cash_Flow_Code = flowcodelist[i].CashFlowCode;
                    IpAccountgrp.Date = DateTime.Now.Date;
                    IpAccountgrp.Account_Head = accountheadcode;
                    IpAccountgrp.Amount = flowcodelist[i].total;
                    IpAccountgrp.Uin = flowcodelist[i].uin;
                    IpAccountgrp.Siteid = flowcodelist[i].Siteid;
                    if (patientDischarge.Corporate.Bill_No != null)
                    {
                        IpAccountgrp.Receipt_No = patientDischarge.Corporate.Bill_No;

                    }
                    else
                    {


                        IpAccountgrp.Receipt_No = patientDischarge.Admission.Receipt_No;

                    }




                    Context.Ip_Account_Grp.Add(IpAccountgrp);
                }

            }


        


            var denomination = new DenominationDetails();
                foreach (var item in patientDischarge.Denomination.ToList())
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
                    denomination.Uin = patientDischarge.Admission.UIN;
                    denomination.Siteid = patientDischarge.Admission.SiteId;
                    denomination.Mr_No = patientDischarge.Admission.Mr_No;
                    denomination.Ipa_No = patientDischarge.Admission.Ipa_No;
                    denomination.Date = DateTime.Now;
                    denomination.Module_Code = "MOD5";
                   denomination.Receipt_No = patientDischarge.Admission.Receipt_No;
                    Context.Denomination_Details.Add(denomination);
                }



               // Print 



          //  var ChargeDetail = new List<ChargeDetail>();

        






            try
            {

                if (Context.SaveChanges() > 0)


                    patientDischarge.ChargeDetail = (from IAD in Context.Ip_Account_Grp.Where(u => u.Ipa_No == patientDischarge.Admission.Ipa_No && u.Amount > 0)
                                                     join ICFM in Context.IP_CASH_FLOW_MASTER.Where(x => x.Cash_Flow_Code != "501" )// x.Debit_Credit_Code == "CREDIT" &&
                                                     on IAD.Cash_Flow_Code equals ICFM.Cash_Flow_Code
                                                     orderby IAD.Date

                                                     select new ChargeDetail
                                                     {
                                                         Datee = IAD.Date.ToString("dd/MM/yyyy"),
                                                         Description = ICFM.Cash_Flow_Description,
                                                         Debit = Convert.ToDecimal(IAD.Amount),

                                                     }

                                                ).ToList();




                patientDischarge.AdvanceAmount = (from IAD in Context.Ip_Account_Grp.Where(u => u.Ipa_No == patientDischarge.Admission.Ipa_No && u.Amount > 0)
                                                                                  join ICFM in Context.IP_CASH_FLOW_MASTER.Where(x => x.Cash_Flow_Code == "501")
                                                                                  on IAD.Cash_Flow_Code equals ICFM.Cash_Flow_Code
                                                                                  orderby IAD.Date

                                                                                  select new AdvanceAmount
                                                                                  {
                                                                                      Datee = IAD.Date.ToString("dd/MM/yyyy"),
                                                                                      Description = ICFM.Cash_Flow_Description,
                                                                                      Debit = Convert.ToDecimal(IAD.Amount),

                                                                                  }

                                                ).ToList();



                var total = patientDischarge.ChargeDetail.Sum(x => x.Debit);


                patientDischarge.SubsidyAmount = Context.Subsidy_Allocated.Where(x => x.IP_No == patientDischarge.Admission.Ipa_No && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.Subsidy_Granted).FirstOrDefault();
                patientDischarge.CorporatAmount = Context.Corporate_Cases.Where(x => x.IP_No == patientDischarge.Admission.Ipa_No && x.SiteId == patientDischarge.Admission.SiteId && x.Module_Code == "MOD5").Select(x => x.Amount_Sponsored).FirstOrDefault();

         


                var receiptno = Context.Ip_Account_Dtl.Where(X => X.Ipa_No == patientDischarge.Admission.Ipa_No && X.Cash_Flow_Code == "501").Select(x => x.Tinvoice_No).ToList(); 

           patientDischarge.Receiveamtformpatient = Context.Cash_Paid.Where(x => x.IPA_NO == patientDischarge.Admission.Ipa_No && x.Module_Code == "MOD5" && x.Transaction_Code == "RECPT").Sum(x => x.Fees_Paid);

                patientDischarge.RefaundAmount = Context.Cash_Paid.Where(x => x.IPA_NO == patientDischarge.Admission.Ipa_No && x.Module_Code == "MOD5" && x.Transaction_Code == "REFND").Sum(x => x.Fees_Paid);

                //var date11 = new List<string>();
                //var desc = new List<string>();
                //var sum = new List<decimal>();
                //foreach (var item3 in patientDischarge.ChargeDetail)
                //{

                //    date11.Add(item3.Datee);
                //    desc.Add(item3.Description);
                //    sum.Add(item3.Debit);
                //}

                //patientDischarge.Debit0 = date11;
                //patientDischarge.Debit2 = desc;
                //patientDischarge.Debit3 = sum;
                var totalchargebill = patientDischarge.ChargeDetail.Sum(x => x.Debit);
                var prm = new PatientRegistrationMaster();

                prm.Patient_Name = Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.Patient_Name).FirstOrDefault();

                prm.Door = Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.Door).FirstOrDefault();

                prm.Street_Locality = Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.Street_Locality).FirstOrDefault();

                prm.Town_City = Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.Town_City).FirstOrDefault();

                prm.State = (from prr in Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId)
                             join sm in Context.State_Master on prr.State equals sm.State_Code
                             select sm.State_Name).FirstOrDefault();

                prm.Taluk = (from prr in Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId)
                             join sm in Context.Taluk_Master on prr.Taluk equals sm.Taluk_Code
                             select sm.Taluk_Name).FirstOrDefault();

                prm.District = Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.District).FirstOrDefault();




                return new
                {
                    Success = true,





                    //datecharge = patientDischarge.Debit0,
                    //DEBITcharge = patientDischarge.Debit2,
                    //DESCcharge = patientDischarge.Debit3,
                    mrno = ipadmissions.Mr_No,
                    ipnoo = ipadmissions.Ip_No,
                    names = prm.Patient_Name,
                    town = prm.Town_City,
                    taluks = prm.Taluk,
                    sta = prm.State,
                    // coun = prm.Country,
                    Dis = prm.District,
                    recp = ipadmissions.Receipt_No,
                    addate = ipadmissions.Admission_Date,
                    disdate = ipadmissions.Discharge_Date,
                    room = ipadmissions.Room_No,
                    occuplieddate = patientDischarge.OccupiedDate,
                    totalamt = patientDischarge.Roomtotalamt,
                    totalchargeamt = totalchargebill,

                    Chargesbeakupdetaisl = patientDischarge.ChargeDetail,
                    Advanceamount = patientDischarge.AdvanceAmount,
                    AmtReceiveformpatients = patientDischarge.Receiveamtformpatient,
                    subsidyamounrs = patientDischarge.SubsidyAmount,
                    CorporateAmounts = patientDischarge.CorporatAmount,
                    RefaundAmounts = patientDischarge.RefaundAmount,

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
              //  Message = "Some data are Missing"
            };



        }





        public dynamic GetipaDetails(string ipa_no, int? siteId = null)
        {


            


           var patientDischarge = new Patient_Discharge();


            patientDischarge.Admission = Context.Ip_Admission.Where(x => x.Ipa_No == ipa_no && x.SiteId == siteId).FirstOrDefault();

            patientDischarge.ChargeDetail = (from IAD in Context.Ip_Account_Grp.Where(u => u.Ipa_No == patientDischarge.Admission.Ipa_No && u.Amount > 0)
                                             join ICFM in Context.IP_CASH_FLOW_MASTER.Where(x => x.Cash_Flow_Code != "501")// x.Debit_Credit_Code == "CREDIT" &&
                                             on IAD.Cash_Flow_Code equals ICFM.Cash_Flow_Code
                                             orderby IAD.Date

                                             select new ChargeDetail
                                             {
                                                 Datee = IAD.Date.ToString("dd/MM/yyyy"),
                                                 Description = ICFM.Cash_Flow_Description,
                                                 Debit = Convert.ToDecimal(IAD.Amount),

                                             }

                                              ).ToList();




            patientDischarge.AdvanceAmount = (from IAD in Context.Ip_Account_Grp.Where(u => u.Ipa_No == patientDischarge.Admission.Ipa_No && u.Amount > 0)
                                              join ICFM in Context.IP_CASH_FLOW_MASTER.Where(x => x.Cash_Flow_Code == "501")
                                              on IAD.Cash_Flow_Code equals ICFM.Cash_Flow_Code
                                              orderby IAD.Date

                                              select new AdvanceAmount
                                              {
                                                  Datee = IAD.Date.ToString("dd/MM/yyyy"),
                                                  Description = ICFM.Cash_Flow_Description,
                                                  Debit = Convert.ToDecimal(IAD.Amount),

                                              }

                                            ).ToList();



            var total = patientDischarge.ChargeDetail.Sum(x => x.Debit);


            patientDischarge.SubsidyAmount = Context.Subsidy_Allocated.Where(x => x.IP_No == patientDischarge.Admission.Ipa_No && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.Subsidy_Granted).FirstOrDefault();
            patientDischarge.CorporatAmount = Context.Corporate_Cases.Where(x => x.IP_No == patientDischarge.Admission.Ipa_No && x.SiteId == patientDischarge.Admission.SiteId && x.Module_Code == "MOD5").Select(x => x.Amount_Sponsored).FirstOrDefault();

            var receiptno = Context.Ip_Account_Dtl.Where(X => X.Ipa_No == patientDischarge.Admission.Ipa_No && X.Cash_Flow_Code == "501").Select(x => x.Tinvoice_No).ToList();

            patientDischarge.Receiveamtformpatient = Context.Cash_Paid.Where(x => x.IPA_NO == patientDischarge.Admission.Ipa_No && x.Module_Code == "MOD5" && x.Transaction_Code == "RECPT").Sum(x => x.Fees_Paid);


            patientDischarge.RefaundAmount = Context.Cash_Paid.Where(x => x.IPA_NO == patientDischarge.Admission.Ipa_No && x.Module_Code == "MOD5" && x.Transaction_Code == "REFND").Sum(x => x.Fees_Paid);



            var totalchargebill = patientDischarge.ChargeDetail.Sum(x => x.Debit);
            var prm = new PatientRegistrationMaster();

            prm.Patient_Name = Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.Patient_Name).FirstOrDefault();

            prm.Door = Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.Door).FirstOrDefault();

            prm.Street_Locality = Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.Street_Locality).FirstOrDefault();

            prm.Town_City = Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.Town_City).FirstOrDefault();

            prm.State = (from prr in Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId)
                         join sm in Context.State_Master on prr.State equals sm.State_Code
                         select sm.State_Name).FirstOrDefault();

            prm.Taluk = (from prr in Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId)
                         join sm in Context.Taluk_Master on prr.Taluk equals sm.Taluk_Code
                         select sm.Taluk_Name).FirstOrDefault();

            prm.District = Context.Patient_Registration_Master.Where(x => x.UIN == patientDischarge.Admission.UIN && x.SiteId == patientDischarge.Admission.SiteId).Select(x => x.District).FirstOrDefault();




            return new
            {
                Success = true,





                //datecharge = patientDischarge.Debit0,
                //DEBITcharge = patientDischarge.Debit2,
                //DESCcharge = patientDischarge.Debit3,
                mrno =patientDischarge.Admission.Mr_No,
                ipnoo = patientDischarge.Admission.Ip_No,
                names = prm.Patient_Name,
                town = prm.Town_City,
                taluks = prm.Taluk,
                sta = prm.State,
                // coun = prm.Country,
                Dis = prm.District,
                recp = patientDischarge.Admission.Receipt_No,
                addate = patientDischarge.Admission.Admission_Date,
                disdate = patientDischarge.Admission.Discharge_Date,
                room = patientDischarge.Admission.Room_No,
                occuplieddate = patientDischarge.OccupiedDate,
                totalamt = patientDischarge.Roomtotalamt,
                totalchargeamt = totalchargebill,

                Chargesbeakupdetaisl = patientDischarge.ChargeDetail,
                Advanceamount = patientDischarge.AdvanceAmount,
                AmtReceiveformpatients = patientDischarge.Receiveamtformpatient,
                subsidyamounrs = patientDischarge.SubsidyAmount,
                CorporateAmounts = patientDischarge.CorporatAmount,
                RefaundAmounts = patientDischarge.RefaundAmount,
            };
        }

           



            public IEnumerable<Dropdown> GetCorporates()
        {
            var corporateCode = Context.Corporate_Master_Dtl.Where(x => x.Contract_Expiry_Date.Date >= DateTime.Now.Date).Select(x => x.Corporate_Code).Distinct().ToList();
            return Context.Corporate_Master.Where(x => corporateCode.Contains(x.Corporate_Code)).Select(x => new Dropdown { Text = x.Corporate_Name, Value = x.Corporate_Code }).ToList();
        }

        public double GetViewCollectionByOperator(string operatorCode)
        {
        //    return Context.Cash_Paid.Where(x => x.Operator_Code == operatorCode && x.Sysdate.Date == DateTime.Now.Date && x.Module_Code == "MOD5" && x.SiteId==1).Sum(x => x.Fees_Paid);
            return Context.Cash_Paid.Where(x => x.Operator_Code == operatorCode && x.Sysdate.Date == DateTime.Now.Date && x.Cancel != "Y" && x.Module_Code == "MOD4" && x.SiteId == 1).Sum(x => x.Fees_Paid);
        }

        public IEnumerable<Dropdown> GetEmployeeGrade(string corporateCode)
        {
            return Context.Corporate_Master_Dtl.Where(x => x.Corporate_Code == corporateCode).Select(x => new Dropdown { Text = x.Employee_Grade, Value = x.Cor_Sl_No }).ToList();
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

        public dynamic GetSelectedCategoryDetails(string categoryCode)
        {
            return Context.Service_Category.FirstOrDefault(x => x.Category_Code == categoryCode);
        }


    }
}
