using System;
using System.Collections.Generic;
using System.Text;

namespace IHMS.Data.Model.ViewModel
{
    public class InterimRefund
    {
        public ICollection<CashPaidDetails> CashPaidDetails { get; set; }

        public string IPA_No { get; set; }
        public string UIN_No { get; set; }
        public string MR_No { get; set; }
        public int Site_ID { get; set; }
        public string Operator_ID { get; set; }
        public string Module_ID { get; set; }

        public DateTime Surgery_Date { get; set; }

        public string Eye_Code { get; set; }
        public string Surgery_Code { get; set; }
        public string Doctor_Code { get; set; }
        public string Category_Code { get; set; }
        public string Clinic_Code { get; set; }

        public double TotalAmount { get; set; }
        public double TotalAdmissionAmt { get; set; }
        public double TotalRefundAmt { get; set; }
        public double RefundAmount { get; set; }

        public string Patient_Name { get; set; }
        public string Patient_LastName { get; set; }
        public string Patient_Class { get; set; }
        public DateTime Patient_DOB { get; set; }

        public string NextOfKin { get; set; }
        public string NextOfKinDetails { get; set; }
        public string PatientDetail { get; set; }
        public string Address { get; set; }

        public string Door { get; set; }
        public string Street { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string DistrictDesc { get; set; }
        public string StateDesc { get; set; }
        public string Pincode { get; set; }
        public string DischargedStatus { get; set; }

        public bool IsPatientDischarged { get; set; }

    }
     

    
public class CashPaidDetails
    {
        public string Source { get; set; }
        public string Date { get; set; }
        public double Amount { get; set; }
        public string TransationCode { get; set; }
    }



}

