using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Model.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IHMS.Data.Repository
{
    public interface IPatientAdmissionRepository : IRepositoryBase<Patient_Admission>
    {       
        Patient_Admission GetPatientDetails(string UIN, string status, int? emrSiteId);
        dynamic InsertPatientAdmission(Patient_Admission patientAdmission);
        int CheckExistingUIN(string UIN, int? emrSiteId);
        dynamic GetSurgeryDetails(string SurgeryCode);
        SurgeryCostDetails GetChargeBreakUp(string SurgeryCode, string RoomType);
        dynamic GetRoomCharge(string RoomType);
        double GetViewCollectionByOperator(string operatorCode, int siteId);
        IEnumerable<Dropdown> GetRoomPayType(string RoomPayType);
        dynamic GetRoomDetails(string roomType, int siteId ,string paytype);

        Patient_Admission GetpopDetails(string MR_NO, int? emrSiteId);  

        dynamic GetipaDetails(string ipa_no, int? emrSiteId);
        IEnumerable<Dropdown> Getsurgeryname(string surgerytype);
        

        InterimRefund GetInterimRefund(string no, bool isIpaNo, int siteId);
        dynamic ToDBInterimRefund(InterimRefund interimRefund);
        AdditionalAdvanceViewModel GetIPADetails(string no, bool isIpaNo, int siteId);
        dynamic SaveAdditionalAdvance(AdditionalAdvanceViewModel additionalAdvance);

       
    }
}
