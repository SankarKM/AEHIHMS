using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IHMS.Data.Model
{
    public class AdditionalAdvanceViewModel
    {
        public string PatientClass { get; set; }
        public string Mr_No { get; set; }
        public string UIN { get; set; }
        public string IPA_No { get; set; }
        public string PatientName { get; set; }
        public string NextOfKin { get; set; }
        public string PatientDetail { get; set; }
        public string Address { get; set; }
        public string Eye { get; set; }
        public string SurgeryName { get; set; }
        public string SurgeryDate { get; set; }
        public string Clinic { get; set; }
        public string Doctor { get; set; }
        public string PatientCategory { get; set; }

        public int Age { get; set; }
        public double TotalAmount { get; set; }
        public ICollection<IPPaymentDetail> IPPaymentDetail { get; set; }
        public ICollection<DenominationDetails> Denomination { get; set; }
        public IPAdmission IPAdmission { get; set; }
        public PatientRegistrationMaster Master { get; set; }
        public ICollection<CashPaid> CashPaid { get; set; }
        public string PaymentType { get; set; }
        public double AdvanceAmount { get; set; }
        public int SiteId { get; set; }
        public string OperatorCode { get; set; }
        public double TotalAdvance { get; set; }
        public double TotalRefund { get; set; }
        public bool Success { get; set; }
        public bool IsPatientDischarged { get; set; }
        public PaymentDetail PaymentDetail { get; set; }
    }

    public class IPPaymentDetail
    {
        public string Source { get; set; }
        public string Date { get; set; }
        public double Amount { get; set; }
        public string TransationCode { get; set; }
    }
}