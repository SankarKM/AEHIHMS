using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IHMS.Data.Model
{
    public class Patient_Registration
    {
        public PatientRegistrationMaster Master { get; set; }
        public PatientRegistrationDetail Detail { get; set; }
        public PatientStatus Status { get; set; }
        public PatientRegistrationMasterAddress PermanentAddress { get; set; }
        public PatientAddlInfo AddlInfo { get; set; }
        public ICollection<PatientHistory> History { get; set; }
        public ReferralPatientsHDR Referral { get; set; }
        public ICollection<CashPaid> CashPaid { get; set; }
        public InvoiceMaster Invoice { get; set; }
        public SubsidyAllocated Subsidy { get; set; }
        public CorporateCases Corporate { get; set; }
        public PaymentDetail PaymentDetail { get; set; }
        public Kiosk Kiosk { get; set; }
        public OtpHoneywell Otp_Honeywell { get; set; }
        //public MRLocationMaster MRLocationMaster { get; set; }
        public int Age { get; set; }
        public decimal Fees { get; set; }
        public string MrLocation { get; set; }
        public string BaseUnit { get; set; }
        public string LastUnitVisited { get; set; }
        public bool IsPaymentApplicable { get; set; }
        public bool IsRegisteredToday { get; set; }
        public string Message { get; set; }
        public string imagename { get; set; }
    }

    public class FunctionUnits
    {
        public string Allocation_Code { get; set; }
        public string Function_Status { get; set; }
        public string Location_Name { get; set; }
    }

    public class PatientRegistrationList
    {
        public string UIN { get; set; }
        public string MR_NO { get; set; }
        public string PatientName { get; set; }
        public DateTime DOB { get; set; }
        public string Instance { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string PayType { get; set; }
        public double Fees { get; set; }
    }
}
