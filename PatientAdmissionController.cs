using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Model.ViewModel;
using IHMS.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IHMS.Services.Controllers
{
    [Route("[controller]")]
    public class PatientAdmissionController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public PatientAdmissionController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        [HttpGet("GetPatientDetails/{UIN}/{status}/{siteId?}")]
        public Patient_Admission GetPatientDetails(string UIN, string status, int? siteId = null )
        {
            return _repoWrapper.PatientAdmission.GetPatientDetails(UIN,status,siteId);
        }

        [HttpPost("InsertPatientAdmission")]
        public dynamic Post([FromBody] Patient_Admission patientAdmission)
        {
            return _repoWrapper.PatientAdmission.InsertPatientAdmission(patientAdmission);
        }

        [HttpGet("CheckExistingUIN/{UIN}/{siteId?}")]
        public int CheckExistingUIN(string UIN, int? siteId = null)
        {
            return _repoWrapper.PatientAdmission.CheckExistingUIN(UIN, siteId);
        }

        [HttpGet("GetSurgeryDetails/{SurgeryCode}")]
        public dynamic GetSurgeryDetails(string SurgeryCode)
        {
            return _repoWrapper.PatientAdmission.GetSurgeryDetails(SurgeryCode);
        }

        [HttpGet("GetChargeBreakUp/{SurgeryCode}/{RoomType}")]
        public SurgeryCostDetails GetChargeBreakUp(string SurgeryCode, string RoomType)
        {
            return _repoWrapper.PatientAdmission.GetChargeBreakUp(SurgeryCode, RoomType);
        }

        [HttpGet("GetRoomCharge/{RoomType}")]
        public dynamic GetRoomCharge(string RoomType)
        {
            return _repoWrapper.PatientAdmission.GetRoomCharge(RoomType);
        }

        [HttpGet("getViewCollectionByOperator/{operatorCode}/{siteId}")]
        public double GetViewCollectionByOperator(string operatorCode, int siteId)
        {
            return _repoWrapper.PatientAdmission.GetViewCollectionByOperator(operatorCode, siteId);
        }

        [HttpGet("GetRoomPayType/{RoomPayType}")]
        public IEnumerable<Dropdown> GetRoomPayType(string RoomPayType)
        {
            return _repoWrapper.PatientAdmission.GetRoomPayType(RoomPayType);
        }

        [HttpGet("getIPADetails/{no}/{isIpaNo}/{siteId}")]
        public AdditionalAdvanceViewModel GetIPADetails(string no, bool isIpaNo, int siteId)
        {
            return _repoWrapper.PatientAdmission.GetIPADetails(no, isIpaNo, siteId);
        }

        [HttpPost("saveAdditionalAddvance")]
        public dynamic SaveAdditionalAdvance([FromBody] AdditionalAdvanceViewModel additionalAdvance)
        {
            return _repoWrapper.PatientAdmission.SaveAdditionalAdvance(additionalAdvance);
        }
        [HttpGet("GetRoomDetails/{roomType}/{siteId}/{paytype}")]
        public dynamic GetRoomDetails(string roomType, int siteId ,string paytype)
        {
            return _repoWrapper.PatientAdmission.GetRoomDetails(roomType, siteId , paytype);
        }

        [HttpGet("getInterimRefund/{no}/{isIpaNo}/{siteId}")]
        public InterimRefund GetInterimRefund(string no, bool isIpaNo, int siteId)
        {
            return _repoWrapper.PatientAdmission.GetInterimRefund(no, isIpaNo, siteId);
        }

        [HttpPost("ToDBInterimRefund")]
        public dynamic Post([FromBody] InterimRefund interimRefund)
        {
            return _repoWrapper.PatientAdmission.ToDBInterimRefund(interimRefund);
        }
        //[HttpGet("getCampAdmission/{siteId}")]
        //public CampAdmission GetCampAdmission (int siteId)
        //{
        //    return _repoWrapper.PatientAdmission.GetCampAdmission(siteId);
        //}

        [HttpGet("GetpopDetails/{MR_NO}/{siteId?}")]
        public Patient_Admission GetpopDetails(string MR_NO, int? siteId = null)
        {
            return _repoWrapper.PatientAdmission.GetpopDetails(MR_NO, siteId);
        }




        [HttpGet("GetipaDetails/{ipa_no}/{siteId?}")]
        public dynamic GetipaDetails(string ipa_no, int? siteId = null)
        {
            return _repoWrapper.PatientAdmission.GetipaDetails(ipa_no, siteId);
        }



        [HttpGet("getsurgeryname /{surgerytype}")]
        public IEnumerable<Dropdown> Getsurgeryname(string surgerytype)
        {
            return _repoWrapper.PatientAdmission.Getsurgeryname(surgerytype);
        }



    }
}




    