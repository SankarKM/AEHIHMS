using IHMS.Data.Common;
using IHMS.Data.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IHMS.Data.Repository
{
    public interface IPatientRegistrationRepository : IRepositoryBase<Patient_Registration>
    {
        double GetViewCollectionByOperator(string operatorCode, int siteId);
        dynamic SaveImage(string Imagename);
        IEnumerable<Dropdown> GetAllocation(string specialityCode, string siteCode);
        dynamic AgeOrDboChange(string type, string value);
        decimal GetRegistrationFees(string specialityCode, string instance, string type);
        IEnumerable<Dropdown> GetCorporates();
        IEnumerable<Dropdown> GetEmployeeGrade(string corporateCode);
        Patient_Registration UinSearch(string uin, int siteId);
        Patient_Registration GetReviewPatient(string no, int siteId, bool isMrNo, string patientClass);
        dynamic GetSelectedCategoryDetails(string categoryCode);
        dynamic UpdatePatientRegistration(Patient_Registration patientRegistration);
        bool UploadImage(IFormFile file, string uin);
        string GetPatientImagePath(string uin);
        List<PatientRegistrationList> GetPatientRegistrationList(int siteId);
        dynamic ReferralDoctorChange(string code);
        dynamic Recall(string mrNo);
        dynamic GetAllUnits(string specialityCode, string siteCode, int siteId);
        bool UpdateFunctionalUnits(string specialityCode, int siteId, FunctionUnits[] units);
        ReceiptCancel GetVisitCancelDetails(string no, int siteId);
        bool SaveCancelVisit(ReceiptCancel receiptCancel);
        ReferralViewModel GetReferralDetails(string no, bool isMrNo, int siteId);
        dynamic SaveReferral(ReferralPatientsHDR referral);
        dynamic Translate(string patientName, string langCode);
        dynamic Reprint(string mrNo);
        dynamic OTP(string PIN);
    }
}
