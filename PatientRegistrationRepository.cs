using IHMS.Data.Common;
using IHMS.Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IHMS.Data.Repository.Implementation
{
    public class PatientRegistrationRepository : RepositoryBase<Patient_Registration>, IPatientRegistrationRepository
    {
        private readonly IHMSContext _context;

        public PatientRegistrationRepository(IHMSContext context) : base(context)
        {
            _context = context;
        }

        public double GetViewCollectionByOperator(string operatorCode, int siteId)
        {
            return Context.Cash_Paid.Where(x => x.Operator_Code == operatorCode && x.Sysdate.Date == DateTime.Now.Date && x.Cancel != "Y" && x.Module_Code == "MOD1" && x.SiteId == siteId).Sum(x => x.Fees_Paid);
        }

        public IEnumerable<Dropdown> GetAllocation(string specialityCode, string siteCode)
        {
            var allocationCode = Context.Allocation_Master.Where(x => x.Speciality_Code == specialityCode && x.SiteCode == siteCode && x.Function_Status == "Y").Select(x => x.Allocation_Code).ToList();
            return Context.Location_Master.Where(x => allocationCode.Contains(x.Location_Code)).Select(x => new Dropdown { Text = x.Location_Name, Value = x.Location_Code }).OrderBy(x => x.Text).ToList();
        }

        public dynamic AgeOrDboChange(string type, string value)
        {
            var age = 0;
            dynamic calculatedValue = null;
            if (type == "age")
            {
                age = Convert.ToInt16(value);
                calculatedValue = DateTime.Now.AddYears(-age).ToString("yyyy-01-01");
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

        public decimal GetRegistrationFees(string specialityCode, string instance, string type)
        {
            var regNorms = Context.Registration_Norms.Where(x => x.Speciality_Code == specialityCode).FirstOrDefault();
            if (regNorms != null)
            {
                if (instance.ToLower() == "new")
                {
                    switch (type.ToLower())
                    {
                        case "free":
                            return regNorms.New_Free_Cost_Value;
                        case "":
                        case "full payment":
                            return regNorms.New_Pay_Cost_Value;
                        case "vision center":
                            return regNorms.VC_New_Cost_Value;
                        case "community center":
                            return regNorms.CC_New_Cost_Value;
                    }
                }
                else if (instance.ToLower() == "review")
                {
                    switch (type.ToLower())
                    {
                        case "free":
                            return regNorms.Old_Free_Cost_Value;
                        case "":
                        case "full payment":
                            return regNorms.Old_Pay_Cost_Value;
                        case "vision center":
                            return regNorms.VC_Old_Cost_Value;
                        case "community center":
                            return regNorms.CC_Old_Cost_Value;
                    }
                }
            }
            return 0;
        }

        public IEnumerable<Dropdown> GetCorporates()
        {
            var corporateCode = Context.Corporate_Master_Dtl.Where(x => x.Contract_Expiry_Date.Date >= DateTime.Now.Date).Select(x => x.Corporate_Code).Distinct().ToList();
            return Context.Corporate_Master.Where(x => corporateCode.Contains(x.Corporate_Code)).Select(x => new Dropdown { Text = x.Corporate_Name, Value = x.Corporate_Code }).OrderBy(x => x.Text).ToList();
        }

        public IEnumerable<Dropdown> GetEmployeeGrade(string corporateCode)
        {
            return Context.Corporate_Master_Dtl.Where(x => x.Corporate_Code == corporateCode).Select(x => new Dropdown { Text = x.Employee_Grade, Value = x.Cor_Sl_No }).OrderBy(x => x.Text).ToList();
        }

        private string GetPrefix(string patientClass, int siteId, bool isMrNo)
        {
            var prefix = "";
            if (isMrNo)
            {
                if (patientClass == "SITE02")
                    prefix = Context.Running_Number_Control.Where(x => x.SiteId == siteId && x.RnControl_Code.Trim() == "MR_NO_FREE").Select(x => x.Control_String_Value.Trim()).FirstOrDefault();
                else
                    prefix = Context.Running_Number_Control.Where(x => x.SiteId == siteId && x.RnControl_Code.Trim() == "MR_NO_PAY").Select(x => x.Control_String_Value.Trim()).FirstOrDefault();
            }
            else
                prefix = Context.Running_Number_Control.Where(x => x.RnControl_Code.Trim() == "UIN").Select(x => x.Control_String_Value.Trim()).FirstOrDefault();
            return prefix;
        }

        public Patient_Registration UinSearch(string uin, int siteId)
        {
            var patientRegistration = new Patient_Registration();
            var prefix = GetPrefix("", siteId, false);
            uin = prefix + uin;
            var checkPatientExist = Context.Patient_Registration_Master.Where(x => x.UIN == uin && x.SiteId == siteId).Count() > 0;
            if (checkPatientExist)
            {
                patientRegistration.Message = "Patient already registered in this hospital!";
                return patientRegistration;
            }

            var master = Context.Patient_Registration_Master.Where(x => x.UIN == uin && x.SiteId != siteId).AsNoTracking()
                    .Include(x => x.Patient_Registration_Master_Address).Include(x => x.PatientAddlInfo).LastOrDefault();

            if (master != null)
            {
                if (master.Patient_Registration_Master_Address.Count() > 0)
                {
                    master.Patient_Registration_Master_Address.All(x => { x.Patient_Registration_Master = null; x.MR_NO = ""; return true; });
                    patientRegistration.PermanentAddress = master.Patient_Registration_Master_Address.Where(x => x.SiteId == master.SiteId).LastOrDefault();
                }
                if (patientRegistration.PermanentAddress == null)
                {
                    patientRegistration.PermanentAddress = new PatientRegistrationMasterAddress();
                    patientRegistration.PermanentAddress.Door = master.Door;
                    patientRegistration.PermanentAddress.Pincode = master.Pincode;
                    patientRegistration.PermanentAddress.Town_City = master.Town_City;
                    patientRegistration.PermanentAddress.POBox = master.POBox;
                    patientRegistration.PermanentAddress.Taluk = master.Taluk;
                    patientRegistration.PermanentAddress.District = master.District;
                    patientRegistration.PermanentAddress.State = master.State;
                    patientRegistration.PermanentAddress.Country = master.Country;
                    patientRegistration.PermanentAddress.SiteId = siteId;
                }

                if (master.PatientAddlInfo.Count() > 0)
                {
                    master.PatientAddlInfo.All(x => { x.Patient_Registration_Master = null; x.MR_NO = ""; return true; });
                    patientRegistration.AddlInfo = master.PatientAddlInfo.Where(x => x.SiteId == master.SiteId).LastOrDefault();
                }
                if (patientRegistration.AddlInfo == null)
                {
                    patientRegistration.AddlInfo = new PatientAddlInfo();
                    patientRegistration.AddlInfo.SiteId = siteId;
                    patientRegistration.AddlInfo.NormalReferral = Context.CommonMaster.Where(x => x.Description.ToLower() == "normal").Select(x => x.Id).FirstOrDefault();
                }

                master.Patient_Registration_Master_Address = null;
                master.PatientAddlInfo = null;
                master.MR_NO = "";
                master.SiteId = siteId;
                patientRegistration.Master = master;

                var now = DateTime.Now;
                var dbo = patientRegistration.Master.Date_Of_Birth;
                var age = now.Year - dbo.Year;
                if (dbo > now.AddYears(-age))
                    age--;

                patientRegistration.Age = age;
            }
            return patientRegistration;
        }

        public Patient_Registration GetReviewPatient(string no, int siteId, bool isMrNo, string patientClass)
        {
            var prefix = GetPrefix(patientClass, siteId, isMrNo);
            no = prefix + no;
            var patientRegistration = new Patient_Registration();
            if (!isMrNo)
                no = Context.Patient_Registration_Master.Where(x => x.UIN == no && x.Patient_Class == patientClass && x.SiteId == siteId).Select(x => x.MR_NO).FirstOrDefault();
            if (!string.IsNullOrEmpty(no))
            {
                var master = Context.Patient_Registration_Master.Where(x => x.MR_NO == no && x.SiteId == siteId).AsNoTracking()
                    .Include(x => x.Patient_Registration_Detail)
                    .Include(x => x.Patient_Registration_Master_Address)
                    .Include(x => x.PatientAddlInfo)
                    .Include(x => x.PatientHistory)
                    .Include(x => x.Referral_Patients_HDR)
                    .Include(x => x.Patient_Status)
                    .Include(x => x.MR_Location_Master)
                    .Include(x => x.Cash_Paid)
                    .FirstOrDefault();

                if (master != null)
                {
                    if (DateTime.Now.Date == master.Last_Visit_Date.Date)
                    {
                        patientRegistration.IsRegisteredToday = true;
                        //return patientRegistration;
                    }
                    master.Patient_Registration_Detail.All(x => { x.Patient_Registration_Master = null; return true; });
                    if (master.Patient_Registration_Master_Address.Count() > 0)
                    {
                        master.Patient_Registration_Master_Address.All(x => { x.Patient_Registration_Master = null; return true; });
                        patientRegistration.PermanentAddress = master.Patient_Registration_Master_Address.Where(x => x.SiteId == siteId).LastOrDefault();
                    }
                    else
                    {
                        patientRegistration.PermanentAddress = new PatientRegistrationMasterAddress();
                        patientRegistration.PermanentAddress.Door = master.Door;
                        patientRegistration.PermanentAddress.Pincode = master.Pincode;
                        patientRegistration.PermanentAddress.Town_City = master.Town_City;
                        patientRegistration.PermanentAddress.POBox = master.POBox;
                        patientRegistration.PermanentAddress.Taluk = master.Taluk;
                        patientRegistration.PermanentAddress.District = master.District;
                        patientRegistration.PermanentAddress.State = master.State;
                        patientRegistration.PermanentAddress.Country = master.Country;
                        patientRegistration.PermanentAddress.SiteId = master.SiteId;
                    }

                    if (master.PatientAddlInfo.Count() > 0)
                    {
                        master.PatientAddlInfo.All(x => { x.Patient_Registration_Master = null; return true; });
                        patientRegistration.AddlInfo = master.PatientAddlInfo.Where(x => x.SiteId == siteId).LastOrDefault();
                    }
                    else
                    {
                        patientRegistration.AddlInfo = new PatientAddlInfo();
                        patientRegistration.AddlInfo.SiteId = master.SiteId;
                        patientRegistration.AddlInfo.NormalReferral = Context.CommonMaster.Where(x => x.Description.ToLower() == "normal").Select(x => x.Id).FirstOrDefault();
                    }

                    if (master.Patient_Status.Count() > 0)
                    {
                        master.Patient_Status.All(x => { x.Patient_Registration_Master = null; return true; });
                        patientRegistration.Status = master.Patient_Status.Where(x => x.SiteId == siteId).LastOrDefault();
                    }
                    else
                    {
                        patientRegistration.Status = new PatientStatus();
                        patientRegistration.Status.SiteId = master.SiteId;
                    }
                    master.PatientHistory.All(x => { x.Patient_Registration_Master = null; return true; });
                    master.Referral_Patients_HDR.All(x => { x.Patient_Registration_Master = null; return true; });
                    //master.Patient_Status.All(x => { x.Patient_Registration_Master = null; return true; });
                    master.MR_Location_Master.Patient_Registration_Master = null;

                    patientRegistration.Detail = master.Patient_Registration_Detail.Where(x => x.SiteId == siteId).LastOrDefault();
                    patientRegistration.Detail.Invoice_No = null;
                    patientRegistration.History = master.PatientHistory.Where(x => x.IsDeleted == false && x.SiteId == siteId).ToList();
                    patientRegistration.Referral = master.Referral_Patients_HDR.Where(x => x.SiteId == siteId && x.Module_Code.Trim() == "MOD1").LastOrDefault();
                    //patientRegistration.Status = master.Patient_Status.Where(x => x.SiteId == siteId).LastOrDefault();
                    patientRegistration.CashPaid = new List<CashPaid>();

                    master.Patient_Registration_Detail = null;
                    master.Patient_Registration_Master_Address = null;
                    master.PatientAddlInfo = null;
                    master.PatientHistory = null;
                    master.Referral_Patients_HDR = null;
                    master.Patient_Status = null;
                    patientRegistration.Master = master;

                    var now = DateTime.Now;
                    var dbo = patientRegistration.Master.Date_Of_Birth;
                    var age = now.Year - dbo.Year;
                    if (dbo > now.AddYears(-age))
                        age--;

                    patientRegistration.Age = age;
                    patientRegistration.Fees = GetReviewPatientFees(patientRegistration.Detail.Speciality_Code, patientRegistration.Master);
                    if (patientRegistration.Fees == 0)
                    {
                        patientRegistration.IsPaymentApplicable = false;
                        //var lastPaymentDate = patientRegistration.Master.Cash_Paid.Where(x => x.MR_NO == no && x.SiteId == siteId).Select(x => x.Sysdate).LastOrDefault();
                        //patientRegistration.CashPaid = patientRegistration.Master.Cash_Paid.Where(x => x.MR_NO == no && x.SiteId == siteId && x.Sysdate.Date == lastPaymentDate.Date && x.Module_Code == "MOD1").ToList();
                        //patientRegistration.CashPaid.All(x => { x.Patient_Registration_Master = null; return true; });
                        //foreach (var item in patientRegistration.CashPaid)
                        //{
                        //    item.PatientCategory = Context.Service_Category.Where(x => x.Category_Code == item.Category_Code).Select(x => x.Category_Description).FirstOrDefault();
                        //    item.PaymentType = Context.Mode_Of_Payment.Where(x => x.Paymode_Code == item.Paymode_Code).Select(x => x.Description).FirstOrDefault();
                        //    item.ShowRemoveIcon = false;
                        //    //if (item.PatientCategory.ToLower().Contains("subsidy"))
                        //    //{
                        //    //    patientRegistration.Subsidy = Context.Subsidy_Allocated.Where(x => (x.MR_NO == no || x.UIN == no)).LastOrDefault();
                        //    //    patientRegistration.Subsidy.Cash_Paid = null;
                        //    //}
                        //    //else if (item.PatientCategory.ToLower().Contains("corporate"))
                        //    //    patientRegistration.Corporate = Context.Corporate_Cases.Where(x => (x.MR_NO == no || x.UIN == no)).LastOrDefault();
                        //}
                    }
                    else
                        patientRegistration.IsPaymentApplicable = true;

                    patientRegistration.Master.Cash_Paid = null;
                    var mrLocationCode = patientRegistration.Master.MR_Location_Master.Location_Code;
                    patientRegistration.MrLocation = Context.Location_Master.Where(x => x.Location_Code == mrLocationCode).Select(x => x.Location_Name).FirstOrDefault();
                    patientRegistration.BaseUnit = Context.Location_Master.Where(x => x.Location_Code == patientRegistration.Master.Base_Unit).Select(x => x.Location_Name).FirstOrDefault();
                    patientRegistration.LastUnitVisited = Context.Location_Master.Where(x => x.Location_Code == patientRegistration.Master.Last_Unit_Visited).Select(x => x.Location_Name).FirstOrDefault();
                }
            }
            return patientRegistration;
        }

        private decimal GetReviewPatientFees(string specialityCode, PatientRegistrationMaster master)
        {
            var regNorms = Context.Registration_Norms.Where(x => x.Speciality_Code == specialityCode).FirstOrDefault();
            if ((master.Visit_Number >= regNorms.Max_Allowed_Visits_Aregistration) || master.Visit_Number == 0)
            {
                if (master.Patient_Class == "SITE01")
                    return regNorms.Old_Pay_Cost_Value;
                else if (master.Patient_Class == "SITE02")
                    return regNorms.Old_Free_Cost_Value;
                else
                    return 0;
            }
            else
            {
                int dateInterval = (DateTime.Now - master.Last_Visit_Date).Days;
                if (dateInterval > regNorms.Max_Inactive_days)
                {
                    if (master.Patient_Class == "SITE01")
                        return regNorms.Old_Pay_Cost_Value;
                    else if (master.Patient_Class == "SITE02")
                        return regNorms.Old_Free_Cost_Value;
                    else
                        return 0;
                }
                else
                    return 0;
            }
        }

        public dynamic GetSelectedCategoryDetails(string categoryCode)
        {
            return Context.Service_Category.Where(x => x.Category_Code == categoryCode).FirstOrDefault();
        }

        private string GetAllocationCode(int age, string specialityCode, string siteCode, int siteId)
        {
            string allocationCode = null;
            allocationCode = Context.Unit3_Allocation.Where(x => x.DayDescription == DateTime.Now.ToString("dddd") && x.StAge <= age && (x.EndingAge >= age || x.EndingAge == null) && x.SiteId == siteId).Select(x => x.AllocationCode).FirstOrDefault();

            if (allocationCode != null)
            {
                var unitFunctionalStatus = Context.Allocation_Master.Where(x => x.Speciality_Code == specialityCode && x.SiteCode == siteCode && x.SiteId == siteId && x.Allocation_Code == allocationCode).Select(x => x.Function_Status).FirstOrDefault();
                if (unitFunctionalStatus != "Y")
                    allocationCode = null;
            }

            if (allocationCode == null)
            {
                var allocation = Context.Allocation_Master.Where(x => x.Speciality_Code == specialityCode && x.SiteCode == siteCode && x.Function_Status == "Y" && x.Min_Age <= age && (x.Max_Age >= age || x.Max_Age == null) && x.SiteId == siteId).OrderBy(x => x.Priority).ToList();
                if (allocation.Count() > 0)
                {
                    var nextIndex = 0;
                    var lastAllocatedItem = allocation.Where(x => x.Current_Priority == 1).FirstOrDefault();
                    if (lastAllocatedItem != null)
                    {
                        var lastAllocatedItemIndex = allocation.IndexOf(lastAllocatedItem);
                        var lastIndex = allocation.LastIndexOf(allocation.LastOrDefault());
                        if (lastIndex != lastAllocatedItemIndex)
                            nextIndex = lastAllocatedItemIndex + 1;
                    }

                    var nextItem = allocation[nextIndex];
                    allocationCode = nextItem.Allocation_Code;

                    allocation.All(x => { x.Current_Priority = 0; return true; });
                    allocation[nextIndex].Current_Priority = 1;
                    Context.Allocation_Master.UpdateRange(allocation);
                }
            }
            return allocationCode;
        }


        public dynamic SaveImage(string Imagename)
        {
            //String path = HttpContext.

            //if (!System.IO.Directory.Exists(path))
            //{
            //    System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            //}

            ////string imageName = ImgName + ".jpg";

            ////set the image path
            //string imgPath = Path.Combine(path, imageName);

            //byte[] imageBytes = Convert.FromBase64String(ImgStr);

            //File.WriteAllBytes(imgPath, imageBytes);

            return true;
        }










        public dynamic UpdatePatientRegistration(Patient_Registration patientRegistration)
        {
            var isReviewPatient = false;
            if (string.IsNullOrWhiteSpace(patientRegistration.Master.MR_NO))
            {
                if (patientRegistration.Detail.Allocation_Code == "" || patientRegistration.Detail.Allocation_Code == null)
                {
                    patientRegistration.Detail.Allocation_Code = GetAllocationCode(patientRegistration.Age, patientRegistration.Detail.Speciality_Code, patientRegistration.Master.Patient_Class, patientRegistration.Master.SiteId);
                    if (patientRegistration.Detail.Allocation_Code == null)
                        return new { Success = false, Message = "Automatic unit allocation is not configured. Please select allocation!" };
                }

                var mrNo = "";
                if (patientRegistration.Master.Patient_Class == "SITE02")
                    mrNo = GenerateRunningCtrlNo("MR_NO_FREE", patientRegistration.Master.SiteId);
                else
                    mrNo = GenerateRunningCtrlNo("MR_NO_PAY", patientRegistration.Master.SiteId);

                var uin = "";
                if (string.IsNullOrWhiteSpace(patientRegistration.Master.UIN))
                    uin = GenerateRunningCtrlNo("UIN", 0, true);
                else
                    uin = patientRegistration.Master.UIN;
                patientRegistration.Master.MR_NO = mrNo;
                patientRegistration.Master.UIN = uin;
                patientRegistration.Master.Visit_Number = 1;
                patientRegistration.Master.Registered_Date = DateTime.Now.Date;
                patientRegistration.Master.Last_Visit_Date = DateTime.Now.Date;
                patientRegistration.Master.Sysdate = DateTime.Now;
                patientRegistration.Master.Base_Unit = patientRegistration.Detail.Allocation_Code;
                patientRegistration.Master.Last_Unit_Visited = patientRegistration.Detail.Allocation_Code;
                Context.Patient_Registration_Master.Add(patientRegistration.Master);


                



                var mrLocationMaster = new MRLocationMaster();
                mrLocationMaster.UIN = patientRegistration.Master.UIN;
                mrLocationMaster.Location_Code = patientRegistration.Detail.Allocation_Code;
                mrLocationMaster.Patient_Name = patientRegistration.Master.Patient_Name;
                mrLocationMaster.Town_City = patientRegistration.Master.Town_City;
                mrLocationMaster.SiteId = patientRegistration.Master.SiteId;
                patientRegistration.Master.MR_Location_Master = mrLocationMaster;

                if (patientRegistration.Referral != null)
                {
                    patientRegistration.Referral.Referral_Case_Number = GenerateRunningCtrlNo("REFERRAL_CASE_NO", patientRegistration.Referral.SiteId);
                    patientRegistration.Referral.UIN = patientRegistration.Master.UIN;
                    patientRegistration.Referral.Visit_Date = DateTime.Now.Date;
                    patientRegistration.Referral.Sysdate = DateTime.Now;
                    patientRegistration.Master.Referral_Patients_HDR = new List<ReferralPatientsHDR>() { patientRegistration.Referral };
                }
            }
            else
            {
                isReviewPatient = true;
                var regNorms = Context.Registration_Norms.Where(x => x.Speciality_Code == patientRegistration.Detail.Speciality_Code).FirstOrDefault();
                if (patientRegistration.Master.Visit_Number >= regNorms.Max_Allowed_Visits_Aregistration)
                    patientRegistration.Master.Visit_Number = 1;
                else
                {
                    int dateInterval = (DateTime.Now - patientRegistration.Master.Sysdate).Days;
                    if (dateInterval > regNorms.Max_Inactive_days)
                        patientRegistration.Master.Visit_Number = 1;
                    else
                        patientRegistration.Master.Visit_Number += 1;
                }
                patientRegistration.Master.Last_Visit_Date = DateTime.Now.Date; //patientRegistration.Master.Sysdate;
                //patientRegistration.Master.Sysdate = DateTime.Now;
                patientRegistration.Detail.Allocation_Code = patientRegistration.Master.Last_Unit_Visited;

                var mrLocationMaster = Context.MR_Location_Master.Where(x => x.MR_NO == patientRegistration.Master.MR_NO && x.SiteId == patientRegistration.Master.SiteId).FirstOrDefault();
                if (mrLocationMaster != null)
                {
                    mrLocationMaster.Location_Code = patientRegistration.Detail.Allocation_Code;
                    Context.Entry(mrLocationMaster).State = EntityState.Modified;
                }
                Context.Entry(patientRegistration.Master).State = EntityState.Modified;

                var existingHistory = Context.PatientHistory.Where(x => x.MR_NO == patientRegistration.Master.MR_NO && x.SiteId == patientRegistration.Master.SiteId).ToList();
                if (existingHistory.Count() > 0)
                {
                    existingHistory.All(x => { x.IsDeleted = true; return true; });
                    Context.PatientHistory.UpdateRange(existingHistory);
                }
            }
            patientRegistration.Detail.UIN = patientRegistration.Master.UIN;
            patientRegistration.Detail.Visit_Date = DateTime.Now.Date;
            patientRegistration.Detail.Visit_Number = Convert.ToInt16(patientRegistration.Master.Visit_Number);
            patientRegistration.Detail.Sysdate = DateTime.Now;
            patientRegistration.PermanentAddress.UIN = patientRegistration.Master.UIN;
            patientRegistration.PermanentAddress.CreatedUTC = DateTime.UtcNow;
            patientRegistration.AddlInfo.UIN = patientRegistration.Master.UIN;
            patientRegistration.Status.UIN = patientRegistration.Master.UIN;
            patientRegistration.Status.Visit_Date = DateTime.UtcNow;

            var opVisitDatewise = _context.Op_Visit_Datewise.Where(x => x.Date == DateTime.Now.Date && x.Patient_Class == patientRegistration.Master.Patient_Class).FirstOrDefault();
            var isNewOpVisit = false;
            if (opVisitDatewise == null)
            {
                isNewOpVisit = true;
                opVisitDatewise = new OpVisitDatewise();
                opVisitDatewise.Date = DateTime.Now.Date;
                opVisitDatewise.Patient_Class = patientRegistration.Master.Patient_Class;
            }
            if (patientRegistration.Detail.Instance_Code == "INSC001")
                opVisitDatewise.New += 1;
            else if (patientRegistration.Detail.Instance_Code == "INSC002")
                opVisitDatewise.Old += 1;

            if (isNewOpVisit)
                _context.Op_Visit_Datewise.Add(opVisitDatewise);
            else
                _context.Entry(opVisitDatewise).State = EntityState.Modified;

            if (isReviewPatient)
            {
                patientRegistration.Detail.SL_NO = 0;
                patientRegistration.Detail.MR_NO = patientRegistration.Master.MR_NO;
                patientRegistration.Detail.UIN = patientRegistration.Master.UIN;
                Context.Patient_Registration_Detail.Add(patientRegistration.Detail);

                patientRegistration.Status.Id = 0;
                patientRegistration.Status.MR_NO = patientRegistration.Master.MR_NO;
                patientRegistration.Status.UIN = patientRegistration.Master.UIN;
                Context.Patient_Status.Add(patientRegistration.Status);
            }
            else
            {
                patientRegistration.Master.Patient_Registration_Detail = new List<PatientRegistrationDetail>() { patientRegistration.Detail };
                patientRegistration.Master.Patient_Registration_Master_Address = new List<PatientRegistrationMasterAddress>() { patientRegistration.PermanentAddress };
                patientRegistration.Master.PatientAddlInfo = new List<PatientAddlInfo>() { patientRegistration.AddlInfo };
                patientRegistration.Master.Patient_Status = new List<PatientStatus>() { patientRegistration.Status };
            }

            if (patientRegistration.History.Count() > 0)
            {
                patientRegistration.History.All(x =>
                {
                    x.Id = 0;
                    x.UIN = patientRegistration.Master.UIN;
                    x.CreatedUTC = DateTime.UtcNow;
                    return true;

                });
                if (isReviewPatient)
                {
                    patientRegistration.History.All(x => { x.MR_NO = patientRegistration.Master.MR_NO; return true; });
                    Context.PatientHistory.AddRange(patientRegistration.History);
                }
                else
                    patientRegistration.Master.PatientHistory = patientRegistration.History;
            }

            if (patientRegistration.CashPaid.Count() > 0 && patientRegistration.IsPaymentApplicable)
            {
                var subsidyDetails = patientRegistration.CashPaid.FirstOrDefault();
                if (subsidyDetails.Category_Code == "CATC003" && patientRegistration.CashPaid.Count() == 1)
                {
                    var cashPaid = new CashPaid();
                    cashPaid.Fees_Paid = 0;
                    cashPaid.Category_Code = subsidyDetails.Category_Code;
                    cashPaid.Paymode_Code = "PC001";
                    cashPaid.Module_Code = subsidyDetails.Module_Code;
                    cashPaid.Operator_Code = subsidyDetails.Operator_Code;
                    cashPaid.Transaction_Code = subsidyDetails.Transaction_Code;
                    cashPaid.OP_IP_Flag = subsidyDetails.OP_IP_Flag;
                    cashPaid.Outstanding = subsidyDetails.Outstanding;
                    cashPaid.SiteId = subsidyDetails.SiteId;
                    patientRegistration.CashPaid.Add(cashPaid);
                }

                var invoiceNo = GenerateRunningCtrlNo("INVOICE_NO", patientRegistration.Master.SiteId);
                patientRegistration.Invoice = new InvoiceMaster();
                patientRegistration.Invoice.Invoice_No = invoiceNo;
                patientRegistration.Invoice.Invoice_Date = DateTime.Now.Date;
                patientRegistration.Invoice.Module_Code = patientRegistration.CashPaid.FirstOrDefault().Module_Code;
                patientRegistration.Invoice.Invoice_Value = float.Parse(patientRegistration.CashPaid.Sum(x => x.Fees_Paid).ToString());
                patientRegistration.Invoice.Invoice_Received_Value = patientRegistration.Invoice.Invoice_Value;
                patientRegistration.Invoice.MR_NO = patientRegistration.Master.MR_NO;
                patientRegistration.Invoice.UIN = patientRegistration.Master.UIN;
                patientRegistration.Invoice.SiteId = patientRegistration.Master.SiteId;

                patientRegistration.CashPaid.All(x =>
                {
                    x.MR_NO = patientRegistration.Master.MR_NO;
                    x.UIN = patientRegistration.Master.UIN;
                    x.Date = DateTime.Now.Date;
                    x.Sysdate = DateTime.Now;
                    x.Invoice_No = invoiceNo;
                    return true;
                });
                foreach (var item in patientRegistration.CashPaid)
                {
                    switch (item.Paymode_Code)
                    {
                        case "PC001":
                        case "PC003":
                        case "PC004":
                        case "PC005":
                            item.Receipt_NO = GenerateRunningCtrlNo("OP_RECEIPT_NO", item.SiteId);
                            break;
                        case "PC007":
                            item.Receipt_NO = GenerateRunningCtrlNo("SUBSIDY_BILL_NO", item.SiteId);
                            break;
                        case "PC006":
                            item.Receipt_NO = GenerateRunningCtrlNo("OP_BILL_NO", item.SiteId);
                            break;
                    }
                }

                if (patientRegistration.Subsidy != null)
                {
                    patientRegistration.CashPaid.All(x =>
                    {
                        x.Category_Code = "CATC003"; return true;
                    });
                    patientRegistration.Subsidy.Subsidy_Case_No = GenerateRunningCtrlNo("SUBSIDY_CASE_NO", patientRegistration.Subsidy.SiteId);
                    patientRegistration.Subsidy.MR_NO = patientRegistration.Master.MR_NO;
                    patientRegistration.Subsidy.UIN = patientRegistration.Master.UIN;
                    patientRegistration.Subsidy.Visit_Date = DateTime.Now.Date;
                    patientRegistration.Subsidy.Sysdate = DateTime.Now;
                    patientRegistration.Subsidy.Receipt_No = patientRegistration.CashPaid.Where(x => x.Paymode_Code == "PC007").Select(x => x.Receipt_NO).FirstOrDefault();
                    patientRegistration.Subsidy.Invoice_No = invoiceNo;
                    patientRegistration.CashPaid.Where(x => x.Paymode_Code == "PC007").FirstOrDefault().Subsidy_Allocated = patientRegistration.Subsidy;
                }

                patientRegistration.Detail.Invoice_No = invoiceNo;
                patientRegistration.Invoice.Cash_Paid = patientRegistration.CashPaid;
                if (isReviewPatient)
                    Context.Invoice_Master.Add(patientRegistration.Invoice);
                else
                    patientRegistration.Master.Invoice_Master = new List<InvoiceMaster>() { patientRegistration.Invoice };

                if (patientRegistration.PaymentDetail != null)
                {
                    patientRegistration.PaymentDetail.UIN = patientRegistration.Master.UIN;
                    patientRegistration.PaymentDetail.Invoice_No = invoiceNo;
                    patientRegistration.PaymentDetail.Receipt_No = patientRegistration.CashPaid.Select(x => x.Receipt_NO).FirstOrDefault();
                    patientRegistration.PaymentDetail.CreatedUTC = DateTime.UtcNow;
                    if (isReviewPatient)
                    {
                        patientRegistration.PaymentDetail.Id = 0;
                        patientRegistration.PaymentDetail.MR_NO = patientRegistration.Master.MR_NO;
                        Context.PaymentDetail.Add(patientRegistration.PaymentDetail);
                    }
                    else
                        patientRegistration.Master.PaymentDetail = new List<PaymentDetail>() { patientRegistration.PaymentDetail };
                }

                if (patientRegistration.Corporate != null)
                {
                    patientRegistration.Corporate.Corporate_Case_No = GenerateRunningCtrlNo("CORPORATE_CASE_NO", patientRegistration.Corporate.SiteId);
                    patientRegistration.Corporate.UIN = patientRegistration.Master.UIN;
                    patientRegistration.Corporate.Invoice_No = invoiceNo;
                    patientRegistration.Corporate.Bill_No = patientRegistration.CashPaid.Where(x => x.Paymode_Code == "PC006").Select(x => x.Receipt_NO).FirstOrDefault();
                    patientRegistration.Corporate.Visit_Date = DateTime.Now.Date;
                    patientRegistration.Corporate.Sysdate = DateTime.Now;
                    patientRegistration.Corporate.CreatedUTC = DateTime.UtcNow;
                    if (isReviewPatient)
                    {
                        patientRegistration.Corporate.MR_NO = patientRegistration.Master.MR_NO;
                        Context.Corporate_Cases.Add(patientRegistration.Corporate);
                    }
                    else
                        patientRegistration.Master.Corporate_Cases = new List<CorporateCases>() { patientRegistration.Corporate };
                }



                //var OtpHoneywell = Context.OTP_HONEYWELL.Where(x => x.Otp ==patientRegistration.Otp_Honeywell.Otp).ToList();
                //if (OtpHoneywell.Count() > 0)
                //{
                //    OtpHoneywell.All(x => { x.MR_NO = patientRegistration.Master.MR_NO; x.UIN= patientRegistration.Master.UIN; return true; });
                //    Context.OTP_HONEYWELL.UpdateRange(OtpHoneywell);
                //}


              

            }

            //var allocation = Context.Allocation_Master.Where(x => x.SiteId == patientRegistration.Detail.SiteId && x.SiteCode == patientRegistration.Master.Patient_Class && x.Speciality_Code == patientRegistration.Detail.Speciality_Code && x.Function_Status == "Y" && x.Allocation_Code == patientRegistration.Detail.Allocation_Code).FirstOrDefault();
            //var lastAllocation = Context.Allocation_Master.Where(x => x.SiteId == patientRegistration.Detail.SiteId && x.SiteCode == patientRegistration.Master.Patient_Class && x.Speciality_Code == patientRegistration.Detail.Speciality_Code && x.Function_Status == "Y" && x.Current_Priority == 1).FirstOrDefault();
            //if (allocation != null)
            //{
            //    if (lastAllocation != null && (lastAllocation.Allocation_Code != allocation.Allocation_Code))
            //    {
            //        lastAllocation.Current_Priority = 0;
            //        Context.Entry(lastAllocation).State = EntityState.Modified;
            //    }
            //    allocation.Current_Priority = 1;
            //    Context.Entry(allocation).State = EntityState.Modified;
            //}

            patientRegistration.imagename = patientRegistration.Master.UIN;

            string imageName = patientRegistration.imagename + ".jpg";

            if (Context.SaveChanges() > 0)
                return new
                {
                    Success = true,
                    MrNo = patientRegistration.Master.MR_NO,
                    Uin = patientRegistration.Master.UIN,
                    ReceiptNo = patientRegistration.PaymentDetail?.Receipt_No ?? "",
                    FirstName = patientRegistration.Master.Patient_Name,
                    Sex = patientRegistration.Master.Sex,
                    Door = patientRegistration.Master.Door,
                    Street = patientRegistration.Master.Street_Locality,
                    Town = patientRegistration.Master.Town_City,
                    Pincode = patientRegistration.Master.Pincode,
                    MobileNo = patientRegistration.Master.Police_Station,
                    Age = patientRegistration.Age,
                    RegistrationAmount = patientRegistration.Invoice?.Invoice_Value ?? 0,// Context.Cash_Paid.Where(x => x.UIN == patientRegistration.Master.UIN && x.Module_Code == "MOD1" && x.SiteId == patientRegistration.Master.SiteId).Select(x => x.Fees_Paid).FirstOrDefault(),
                    District = Context.District_Master.Where(x => x.District_Code == patientRegistration.Master.District).Select(x => x.District_Name).FirstOrDefault()
                };

            return null;
        }

        public bool UploadImage(IFormFile file, string uin)
        {
            try
            {
                var currentDir = Directory.GetCurrentDirectory();
                if (!Directory.Exists(currentDir + "/PatientImages/"))
                    Directory.CreateDirectory(currentDir + "/PatientImages/");
                var fileName = $"{uin}{Path.GetExtension(file.FileName)}";
                var path = $"{currentDir}/PatientImages/{fileName}";

                if ((File.Exists(path)))
                    File.Delete(path);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                    var master = Context.Patient_Registration_Master.Where(x => x.UIN == uin).FirstOrDefault();
                    master.PatientImageName = fileName;
                    Context.Entry(master).State = EntityState.Modified;
                    return Context.SaveChanges() > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetPatientImagePath(string uin)
        {
            var imageName = Context.Patient_Registration_Master.Where(x => x.UIN == uin).Select(x => x.PatientImageName).FirstOrDefault();
            if (imageName != null)
            {
                var currentDir = Directory.GetCurrentDirectory();
                return $"{currentDir}/PatientImages/{imageName}";
            }
            return null;
        }

        private string GenerateRunningCtrlNo(string rnControlCode, int siteId = 1, bool isUin = false)
        {
            RunningNumberControl rn = null;
            if (isUin)
                rn = Context.Running_Number_Control.Where(x => x.RnControl_Code == rnControlCode && x.IsActive == true).FirstOrDefault();
            else
                rn = Context.Running_Number_Control.Where(x => x.RnControl_Code == rnControlCode && x.IsActive == true && x.SiteId == siteId).FirstOrDefault();

            if (rn != null)
            {
                rn.Control_Value += 1;
                Context.Entry(rn).State = EntityState.Modified;
                return $"{rn.Control_String_Value}{rn.Control_Value}";
            }
            else
                return "";
        }

        public List<PatientRegistrationList> GetPatientRegistrationList(int siteId)
        {
            var list = Context.Patient_Registration_Master.Where(x => x.Last_Visit_Date.Date == DateTime.Now.Date && x.SiteId == siteId)
                .Include(x => x.Patient_Registration_Detail).Include(x => x.Cash_Paid).ToList();

            var instance = Context.Patient_Instance.ToList();
            var patientClass = Context.Site_Master.ToList();
            var categories = Context.Service_Category.ToList();
            var payMode = Context.Mode_Of_Payment.ToList();
            var result = list.Select(x => new PatientRegistrationList
            {
                MR_NO = x.MR_NO,
                UIN = x.UIN,
                PatientName = x.Patient_Name,
                DOB = x.Date_Of_Birth,
                Instance = instance.Where(z => z.Instance_Code == x.Patient_Registration_Detail.Where(y => y.SiteId == siteId && y.Sysdate.Date == DateTime.Now.Date).Select(y => y.Instance_Code).LastOrDefault()).Select(z => z.Instance_Description).FirstOrDefault(),
                Type = patientClass.Where(z => z.Site_Code == x.Patient_Class).Select(z => z.Site_Name).FirstOrDefault(),
                Category = categories.Where(z => z.Category_Code == x.Cash_Paid.Where(y => y.SiteId == siteId && y.Sysdate.Date == DateTime.Now.Date).Select(y => y.Category_Code).LastOrDefault()).Select(z => z.Category_Description).FirstOrDefault(),
                PayType = payMode.Where(z => z.Paymode_Code == x.Cash_Paid.Where(y => y.SiteId == siteId && y.Sysdate.Date == DateTime.Now.Date).Select(y => y.Paymode_Code).LastOrDefault()).Select(z => z.Description).FirstOrDefault(),
                Fees = x.Cash_Paid.Where(y => y.SiteId == siteId && y.Sysdate.Date == DateTime.Now.Date).Select(y => y.Fees_Paid).FirstOrDefault(),
            }).ToList();

            return result;
        }

        public dynamic ReferralDoctorChange(string code)
        {
            return Context.Referral_Master.Where(x => x.Referral_Code == code).Select(x => new { State = x.Referral_Address1, City = x.Referral_Address2 }).FirstOrDefault();
        }

        public dynamic Recall(string mrNo)
        {
            var commonRepo = new CommonRepository(_context);
            var master = Context.Patient_Registration_Master.Include(x => x.Patient_Status).Where(x => x.MR_NO == mrNo).LastOrDefault();
            if (master != null)
            {
                var talukDetails = commonRepo.TalukChange(master.Taluk);
                var currentAddress = new
                {
                    master.Door,
                    master.Street_Locality,
                    master.Town_City,
                    master.Pincode,
                    master.POBox,
                    master.Taluk,
                    master.District,
                    master.State,
                    master.Country,
                    DistrictValue = talukDetails.District.Value,
                    StateValue = talukDetails.State.Value,
                    CountryValue = talukDetails.Country.Value,
                };

                var pAddress = Context.Patient_Registration_Master_Address.Where(x => x.MR_NO == mrNo).Select(x => new { x.Door, x.Street_Locality, x.Town_City, x.Pincode, x.POBox, x.Taluk, x.District, x.State, x.Country }).LastOrDefault();
                var paTalukDetails = commonRepo.TalukChange(master.Taluk);
                var permanentAddress = new
                {
                    pAddress.Door,
                    pAddress.Street_Locality,
                    pAddress.Town_City,
                    pAddress.Pincode,
                    pAddress.POBox,
                    pAddress.Taluk,
                    pAddress.District,
                    pAddress.State,
                    pAddress.Country,
                    DistrictValue = paTalukDetails.District.Value,
                    StateValue = paTalukDetails.State.Value,
                    CountryValue = paTalukDetails.Country.Value,
                };
                return new
                {
                    Success = true,
                    CurrentAddress = currentAddress,
                    PermanentAddress = permanentAddress,
                    AllocationCode = master.Base_Unit,
                    MobileNo = master.Police_Station,
                    PurposeOfVisit = master.Patient_Status.Select(x => x.PurposeId).LastOrDefault()
                };
            }
            else
            {
                return new
                {
                    Success = false
                };
            }
        }

        public dynamic GetAllUnits(string specialityCode, string siteCode, int siteId)
        {
            return (from allocation in Context.Allocation_Master
                    join location in Context.Location_Master on allocation.Allocation_Code equals location.Location_Code
                    where allocation.Speciality_Code == specialityCode && allocation.SiteCode == siteCode && allocation.SiteId == siteId
                    select new { allocation.Allocation_Code, allocation.Function_Status, location.Location_Name }).Distinct();
        }

        public bool UpdateFunctionalUnits(string specialityCode, int siteId, FunctionUnits[] units)
        {
            var allocationMaster = Context.Allocation_Master.AsNoTracking().Where(x => x.Speciality_Code == specialityCode && x.SiteId == siteId).ToList();
            if (units.Count() > 0 && allocationMaster.Count() > 0)
            {
                foreach (var unit in units)
                {
                    var allocation = allocationMaster.Where(x => x.Allocation_Code == unit.Allocation_Code).FirstOrDefault();
                    allocation.Function_Status = unit.Function_Status;
                }
                Context.Allocation_Master.UpdateRange(allocationMaster);
                return Context.SaveChanges() > 0;
            }
            return false;
        }

        public ReceiptCancel GetVisitCancelDetails(string no, int siteId)
        {
            var receiptCancel = new ReceiptCancel();
            var master = Context.Patient_Registration_Master.Where(x => x.MR_NO == no && x.SiteId == siteId && x.Last_Visit_Date.Date == DateTime.Now.Date)
                .Include(x => x.Patient_Registration_Detail).Include(x => x.Cash_Paid)
                .FirstOrDefault();
            if (master == null)
                receiptCancel.Message = "No Data Found!";
            else
            {
                var mrDetail = Context.Medical_Record_Dtl.Where(x => x.MR_NO == no && x.SiteId == siteId && x.Sysdate.Date == DateTime.Now.Date && x.ICD_Type_Code == "D").FirstOrDefault();
                if (mrDetail == null)
                {
                    var cashPaid = master.Cash_Paid.Where(x => x.MR_NO == no && x.SiteId == siteId && x.Module_Code == "MOD1" && x.Sysdate.Date == DateTime.Now.Date).ToList();
                    if (cashPaid.FirstOrDefault()?.Cancel == "Y")
                    {
                        receiptCancel.Message = "Already visit has been cancelled.";
                    }
                    else
                    {
                        receiptCancel.Module_Code = "MOD1";
                        receiptCancel.Receipt_No = cashPaid.FirstOrDefault()?.Receipt_NO ?? null;
                        receiptCancel.Fees_Paid = float.Parse(cashPaid.Sum(x => x.Fees_Paid).ToString());
                        receiptCancel.UIN = master.UIN;
                        receiptCancel.MR_NO = master.MR_NO;
                        receiptCancel.SiteId = siteId;
                        receiptCancel.PatientName = master.Patient_Name;
                        receiptCancel.VisitDate = master.Last_Visit_Date.ToString("dd/MM/yyyy");
                        receiptCancel.Success = true;
                    }
                }
                else
                    receiptCancel.Message = "The patient has undergone diagnosis.";
            }

            return receiptCancel;
        }

        public bool SaveCancelVisit(ReceiptCancel receiptCancel)
        {
            var master = Context.Patient_Registration_Master
                .Where(x => x.MR_NO == receiptCancel.MR_NO && x.SiteId == receiptCancel.SiteId && x.Last_Visit_Date.Date == DateTime.Now.Date).AsNoTracking()
                .Include(x => x.Patient_Registration_Detail).Include(x => x.Cash_Paid)
                .Include(x => x.Corporate_Cases).Include(x => x.Subsidy_Allocated).Include(x => x.Referral_Patients_HDR)
                .FirstOrDefault();

            if (master != null)
            {
                var cashPaid = master.Cash_Paid.Where(x => x.MR_NO == master.MR_NO && x.SiteId == master.SiteId && x.Module_Code == "MOD1" && x.Sysdate.Date == DateTime.Now.Date).ToList();
                if (cashPaid.Count() > 0)
                {
                    receiptCancel.Sysdate = DateTime.Now;
                    Context.Receipt_Cancel.Add(receiptCancel);
                }

                var detailToDelete = master.Patient_Registration_Detail.Where(x => x.MR_NO == master.MR_NO && x.SiteId == master.SiteId && x.Sysdate.Date == DateTime.Now.Date).LastOrDefault();
                master.Patient_Registration_Detail.Remove(detailToDelete);

                var last = master.Patient_Registration_Detail.Where(x => x.MR_NO == master.MR_NO && x.SiteId == master.SiteId).OrderBy(x => x.Visit_Date.Date).LastOrDefault();
                if (last != null)
                    master.Last_Visit_Date = last.Visit_Date;
                master.Visit_Number -= 1;

                cashPaid.All(x => { x.Cancel = "Y"; return true; });
                Context.Cash_Paid.UpdateRange(cashPaid);
                Context.Patient_Registration_Detail.Remove(detailToDelete);

                var subsidy = master.Subsidy_Allocated.Where(x => x.MR_NO == master.MR_NO && x.SiteId == master.SiteId && x.Sysdate.Date == DateTime.Now.Date).LastOrDefault();
                if (subsidy != null)
                    Context.Subsidy_Allocated.Remove(subsidy);

                var corporate = master.Corporate_Cases.Where(x => x.MR_NO == master.MR_NO && x.SiteId == master.SiteId && x.Sysdate.Date == DateTime.Now.Date).LastOrDefault();
                if (corporate != null)
                    Context.Corporate_Cases.Remove(corporate);

                var referral = master.Referral_Patients_HDR.Where(x => x.MR_NO == master.MR_NO && x.SiteId == master.SiteId && x.Sysdate.Date == DateTime.Now.Date).LastOrDefault();
                if (referral != null)
                    Context.Referral_Patients_HDR.Remove(referral);

                Context.Entry(master).State = EntityState.Modified;
                var opVisitDatewise = _context.Op_Visit_Datewise.Where(x => x.Date == DateTime.Now.Date && x.Patient_Class == master.Patient_Class).FirstOrDefault();
                if (opVisitDatewise != null)
                {
                    if (detailToDelete.Instance_Code == "INSC001")
                        opVisitDatewise.New -= 1;
                    else if (detailToDelete.Instance_Code == "INSC002")
                        opVisitDatewise.Old -= 1;
                    _context.Entry(opVisitDatewise).State = EntityState.Modified;
                }
                return Context.SaveChanges() > 0;
            }
            return false;
        }

        public ReferralViewModel GetReferralDetails(string no, bool isMrNo, int siteId)
        {
            var result = new ReferralViewModel();
            var master = new PatientRegistrationMaster();
            if (isMrNo)
                master = Context.Patient_Registration_Master.Where(x => x.MR_NO == no && x.SiteId == siteId).AsNoTracking().Include(x => x.Patient_Registration_Detail).FirstOrDefault();
            else
                master = Context.Patient_Registration_Master.Where(x => x.UIN == no && x.SiteId == siteId).AsNoTracking().Include(x => x.Patient_Registration_Detail).FirstOrDefault();

            if (master != null)
            {
                var visitDetails = master.Patient_Registration_Detail
                    .Where(x => x.SiteId == siteId)
                    .OrderBy(x => x.Visit_Date.Date).Select(x => new { Date = x.Visit_Date.Date.ToString("dd/MM/yyyy"), Type = x.Instance_Code == "INSC001" ? "New" : "Review" }).ToList();
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
                result.Referral = new ReferralPatientsHDR();
                result.Referral.MR_NO = master.MR_NO;
                result.Referral.UIN = master.UIN;
                result.Referral.SiteId = master.SiteId;

                result.PatientName = master.Patient_Name;
                result.NextOfKin = $"{master.Next_Of_Kin}, {master.MR_NO}, {master.UIN}";
                result.PatientDetail = $"{age} yrs, {gender}";
                result.Address = $"{district}, {state}";
                result.VisitDetails = visitDetails;
                return result;
            }
            return result;
        }

        public dynamic SaveReferral(ReferralPatientsHDR referral)
        {
            var detail = Context.Patient_Registration_Detail.Where(x => x.MR_NO == referral.MR_NO && x.SiteId == referral.SiteId && x.Visit_Date.Date == referral.Reference_Date.Date).FirstOrDefault();
            if (detail != null)
            {
                var checkReferralEntry = Context.Referral_Patients_HDR.Where(x => x.MR_NO == referral.MR_NO && x.SiteId == referral.SiteId && x.Reference_Date.Date == referral.Reference_Date.Date).FirstOrDefault();
                if (checkReferralEntry == null)
                {
                    referral.Referral_Case_Number = GenerateRunningCtrlNo("REFERRAL_CASE_NO", referral.SiteId);
                    referral.Module_Code = "MOD1";
                    referral.Visit_Date = referral.Reference_Date;
                    referral.Sysdate = DateTime.Now;
                    Context.Referral_Patients_HDR.Add(referral);
                    Context.SaveChanges();
                    return new { Success = true, Message = "Referral details updated successfully." };
                }
                else
                {
                    return new { Success = false, Message = "Already referral details has been added for the selected Reference Date" };
                }
            }
            else
            {
                return new { Success = false, Message = "Patient not visitied on the selected Reference Date" };
            }
        }

        public dynamic Translate(string patientName, string langCode)
        {
            var translatedName = Context.Translate.Where(x => x.Patient_Name == patientName && x.Lang_Code == langCode).Select(x => x.PatientName_Tamil).FirstOrDefault();
            if (translatedName != null)
                return new { Success = true, TranslatedName = translatedName };
            return new { Success = false };
        }




        public dynamic Reprint(string mrNo)
        {


            var commonRepo = new CommonRepository(_context);
            var master = Context.Patient_Registration_Master.Include(x => x.Patient_Status).Where(x => x.MR_NO == mrNo).LastOrDefault();
            if (master != null)
            {

                var talukDetails = commonRepo.TalukChange(master.Taluk);
                var pAddress = Context.Patient_Registration_Master_Address.Where(x => x.MR_NO == mrNo).Select(x => new { x.Door, x.Street_Locality, x.Town_City, x.Pincode, x.POBox, x.Taluk, x.District, x.State, x.Country }).LastOrDefault();
                var paTalukDetails = commonRepo.TalukChange(master.Taluk);
                var cash = Context.Cash_Paid.Where(x => x.MR_NO == mrNo && x.Module_Code == "MOD1").Select(x => new { x.Fees_Paid, x.Receipt_NO }).FirstOrDefault();
                var patientRegistration = new Patient_Registration();

                var now = DateTime.Now;
                var dbo = master.Date_Of_Birth;
                var age = now.Year - dbo.Year;
                if (dbo > now.AddYears(-age))
                    age--;

                var Age = age;


                return new
                {
                    Success = true,
                    MobileNo = master.Police_Station,
                    MrNos = master.MR_NO,
                    Uin = master.UIN,
                    Firstname = master.Patient_Name,
                    Sexs = master.Sex,
                    Agess = Age,
                    Doors = master.Door,
                    Street = master.Street_Locality,
                    Town = master.Town_City,
                    Pincodes = master.Pincode,
                    RegAmounts = cash.Fees_Paid,
                    ReceiptNO = cash.Receipt_NO,
                    RegistrationDate = master.Registered_Date.Date,
                    Disricts = paTalukDetails.District.Value,
                    //  RegisteredDate = master.Registered_Date;


                };
            }
            else
            {
                return false;

            }
        }



        public dynamic OTP(string PIN)
        {

            var selfRegistration = new Patient_Registration();



            if (PIN != null)
            {

                var selfkiosk = Context.patient.Where(x => x.PIN == PIN).FirstOrDefault();
                selfRegistration.Kiosk = selfkiosk;

                //selfRegistration.Master.Patient_Name = selfRegistration.Kiosk.Patient_Name;
                //selfRegistration.Master.Phone = selfRegistration.Kiosk.MobileNo;

            }


            return selfRegistration;
           


        }



    }
}
