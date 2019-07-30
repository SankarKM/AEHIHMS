using System;
using System.Collections.Generic;
using System.Text;

namespace IHMS.Data.Model
{
    public class Patient_Admission
    {
        public PatientRegistrationMaster Master { get; set; }
        public PatientCounselNew PatientCounselNew { get; set; }
        public SurgeryCostDetails SurgeryCostDetail { get; set; }
        public ICollection<CashPaid> CashPaid { get; set; }
        public InvoiceMaster Invoice { get; set; }
        public CorporateCases Corporate { get; set; }
        public PaymentDetail PaymentDetail { get; set; }
        public IPAdmission IPAdmission { get; set; }
        public IPClinic IPClinic { get; set; }
        public IpSurgeryDtl Ip_Surgery_Dtl { get; set; }
        public CorporatePreauth CorporatePreauth { get; set; }
        public CorporateMaster CorporateMaster { get; set; }
        public ICDCodeMaster ICDCodeMaster { get; set; }
        public CurrentRoomStatus CurrentRoomStatus { get; set; }
        public PreauthStatus PreauthStatus { get; set; }
        public ICollection<DenominationDetails> Denomination { get; set; }
        public ICollection<AdditionalProcedureTrans> AdditionalProcedureTrans { get; set; }
        public ICollection<Rdetails> receiptdetails { get; set; }
        public string Icd_Spec_Group_Code { get; set; }


     

        public DateTime birth { get; set; }
        public DateTime date { get; set; }
        public string uin { get; set; } 
        public string sex { get; set; }
        //public string eye { get; set; }
        public string surgeryname { get; set; }

        public int Age { get; set; }
        public int counseldays { get; set; }

        public string corporatecode { get; set; }
        public string corporatename { get; set; }

        public DateTime Date { get; set; }
        public string Sex { get; set; }
        public DateTime Birth { get; set; }
        public string Uin { get; set; }
        public string surgeryna { get; set; }
        public string eye { get; set; }
        public double Couseldaysnormal { get; set; }
        public string Status { get; set; }


        public string IPANo { get; set; }
        public double RoomCost { get; set; }
        public string FloorCode { get; set; }
        public string FloorName { get; set; }
        public DateTime SurgeryDate { get; set; }
        public decimal NoofDays { get; set; }
        public string AditionalProcedure { get; set; }
        public string CorporateName { get; set; }
        public string CorSlNo { get; set; }
        public decimal CorporateAmount { get; set; }
        public string Remark { get; set; }
        public string Eye { get; set; }
        public string Surgery { get; set; }
        public string Corporatee { get; set; }
        public decimal CopayAmount { get; set; }
        public string SurgeryName { get; set; } 
        public string paymode { get; set; }
        public string ClinicCode { get; set; }

        public string PaymodeDescription { get; set; }
        public double AdmissionAmount { get; set; }
        public string Room_No { get; set; }
        public string masterdata { get; set; }

        // public DateTime addate { get; set; }


        public string Patient_Name { get; set; }
        public string Mr_No { get; set; }
        public decimal Amount { get; set; }
        public decimal Copay { get; set; }
        public decimal Ex_Amount { get; set; }
        public DateTime Preauth_Sent_dt { get; set; }
        public DateTime Valid_Fdate { get; set; }
        public DateTime Valid_Tdate { get; set; }

        public string CorporateRemarks { get; set; }
        public string AdmissionType { get; set; }
        public decimal PaidAmount { get; set; }
        public string Description { get; set; }
        public ICollection<PatientAdmissionDetails> PatientAdmissionDetails { get; set; }
        public ICollection<RoomTypeDetails> RoomTypeDetails { get; set; }
       // public ICollection<CorporatePreauth> CorporatePreauth { get; set; }
    }
    public class RoomTypeDetails
    {
        //public RoomMaster Master { get; set; }

        public string ROOM_NO { get; set; }
        public string FLOOR_DESCRIPTION { get; set; }

        public string FLOOR_CODE { get; set; }
        public string TOILET_DESCRIPTION { get; set; }
        public string Status { get; set; }
        public DateTime VACATING_TIME { get; set; }
        public decimal ROOM_COST { get; set; } 
     
        //public virtual ICollection<CurrentRoomStatus> Current_Room_Status { get; set; }
        //public virtual ICollection<FloorMaster> Floor_Master { get; set; }
        //public virtual ICollection<TOILET_TYPE_MASTER> TOILET_TYPE_MASTER { get; set; }
    }

    public class PatientAdmissionDetails
    {
        public string IPANo { get; set; }
        public DateTime Admission_Date { get; set; }
    }

    public class Rdetails
    {
        public string Rno { get; set; }
        public string Paymode { get; set; }
        public string Descrip { get; set; }
        public double Amountpaid { get; set; }
    }


}





