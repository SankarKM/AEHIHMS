using System;
using System.Collections.Generic;
using System.Text;

namespace IHMS.Data.Model.ViewModel
{
    public class Patient_Discharge
    {

        public PatientRegistrationMaster Master { get; set; }
        public IPAdmission Admission { get; set; }
        public IpAccountGrp ACgroup { get; set; }
        public CurrentRoomStatus CRooms { get; set; }
        public ICDCodeMaster ICDcode { get; set; }
        //public IpAccountDtl IPAC { get; set; }
        public ICollection<IpAccountDtl> IPAC { get; set; }   
        public ICollection<CashPaid> CashPaid { get; set; }
        public IpSurgeryDtl Surgery { get; set; }
        public EyeMaster Eyemaster { get; set; }
        public LensMaster Lensmaster { get; set; }
        public SubsidyAllocated Subsidy { get; set; }
        public CorporateCases Corporate { get; set; }
        public PatientRegistrationDetail PatientRegistrationDetail { get; set; }

        public SurgeryCostDetails SurgeryCostDetail { get; set; }
        public RoomMaster RoomMaster { get; set; }
        public RoomTransfer RoomTransfer { get; set; }

        public FloorMaster FloorMaster { get; set; }
        public PatientCounselNew PatientCounselNew { get; set; }
        public IpCashFlowMaster ipCashFlowMaster { get; set; }
        public IPAccount IPAccounts { get; set; }
        public InvoiceMaster Invoice { get; set; }
        public CorporatePreauth CorporatePreauth { get; set; }
        public CorporateMaster Corporatemaster { get; set; }
        public ServiceCategory ServiceCategory { get; set; }

        public TestRepository TestRepository { get; set; }


        public double Result { get; set; }
        //public DenominationDetails DenominationDetails { get; set; }
        public ICollection<DenominationDetails> Denomination { get; set; }

        public ICollection<PayDetail> PayDetail { get; set; }

        public ICollection<Roomlist> RoomList { get; set; }

        public ICollection<RoomDetail> RoomDetail { get; set; }     

        public ICollection<ChargeDetail> ChargeDetail { get; set; }
        public ICollection<AdvanceAmount> AdvanceAmount { get; set; }

        public ICollection<SubsidyAllocated> SubsidyAllocated{ get; set; }

        public PaymentDetail PaymentDetail { get; set; }

        public string Description { get; set; }
        public string DischargeStatus { get; set; }
        public DateTime OccupiedDate { get; set; }
        public decimal Roomtotalamt { get; set; }
        public object Debit0 { get; set; }
        public object ALL { get; set; }
        public object Debit3 { get; set; }
        public object Debit2 { get; set; }



        public string IPANo { get; set; }
        public int Age { get; set; }
        public int ispayable { get; set; }

        public double RoomCost { get; set; }
        public string FloorCode { get; set; }
        public string FloorName { get; set; }
        public decimal NoofDays { get; set; }
        public string AditionalProcedure { get; set; }
        public double AdmissionAmount { get; set; }
        public double Receiveamtformpatient  { get; set; }
        public double RefaundAmount { get; set; }
        public decimal SubsidyAmount { get; set; }
        public decimal CorporatAmount { get; set; }
        
        public decimal tot { get; set; }
        public double tot1 { get; set; }


        public RoomTransfer firstroomtransfer { get; set; }
        public RoomTransfer secondtransfer { get; set; }
        public RoomTransfer thirdtransfer { get; set; }
        public double TotalDays { get; set; }
        public double totalamt { get; set; }
        public double totalamt1 { get; set; }
        public double totalamt2 { get; set; }
        public double totalamt3 { get; set; }

        

    }

    public class PayDetail
    {
        public string Source { get; set; }
        public string Date { get; set; }
        public double Amount { get; set; }

    }


    public class Roomlist
    {
        public dynamic costroomdate { get; set; }

        public dynamic costroomNo { get; set; }
        public dynamic costroomcost { get; set; }

    }
    public class RoomDetail
    {
        public string RoomNo { get; set; }
        public string ToRoomNo { get; set; }
        public string Floor { get; set; }
        public DateTime OcuupiedAt { get; set; }
        public DateTime ChangeDate { get; set; }
        public string Amount { get; set; }
        public string Amount2 { get; set; }
        public TimeSpan calc { get; set; }
        public double calc1 { get; set; }
        public double totamt { get; set; }


    }

    public class ChargeDetail
    {
        public string Datee { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }

        public decimal Debitsum { get; set; }

    }


    public class AdvanceAmount
    {
        public string Datee { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }

        public decimal Debitsum { get; set; }

    }

}
