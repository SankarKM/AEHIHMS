using System;
using System.Collections.Generic;
using System.Text;

namespace IHMS.Data.Model.ViewModel
{
    public class Camp_Discharge
    {

        public PatientRegistrationMaster Master { get; set; }
        public IPAdmission Admission { get; set; }
        public ICDCodeMaster ICDcode { get; set; }
        public IpSurgeryDtl Surgery { get; set; }
        public EyeMaster Eyemaster { get; set; }
        public LensMaster Lensmaster { get; set; }
        public PatientRegistrationDetail PatientRegistrationDetail { get; set; }
        public PaymentDetail PaymentDetail { get; set; }
        public SurgeryCostDetails SurgeryCostDetail { get; set; }
        public RoomMaster RoomMaster { get; set; }
        public FloorMaster FloorMaster { get; set; }
        public PatientCounselNew PatientCounselNew { get; set; }
        public CampMaster campMaster { get; set; }
        public CurrentRoomStatus CurrentRoomStatus { get; set; }
        public VisionAcuityMaster VisionAcuityMaster { get; set; }
        public PatientHistory PatientHistory { get; set; }
        public ICollection<PatientHistory> History { get; set; }
        public ICollection<MedicalRecordDtl> MedicalRecord { get; set; }
        public MedicalRecordDtl MedicalRecorddtl{ get; set; }
        public ClinicalDetail ClinicalDetail { get; set; }
        public ICollection<AdditionalProcedureTrans> AdditionalProcedureTrans { get; set; }
        public string IPANo { get; set; }
        public int Age { get; set; }
        public double RoomCost { get; set; }
        public string FloorCode { get; set; }
        public string FloorName { get; set; }
        public decimal NoofDays { get; set; }
        public string AditionalProcedure { get; set; }
        public double AdmissionAmount { get; set; }
        public string LocationCode { get; set; }
        public string Surgeryname { get; set; }
        public string Message { get; set; }

        


    }
}
