using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Model.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IHMS.Data.Repository
{
    public interface IPatientDischargeRepository : IRepositoryBase<Patient_Discharge>
    {
        Patient_Discharge GetPatientDetails(string UIN, int? emrSiteId);
        dynamic updatePatientDischarge(Patient_Discharge patientDischarge);
        IEnumerable<Dropdown> GetCorporates();
        IEnumerable<Dropdown> GetEmployeeGrade(string corporateCode);
        dynamic GetSelectedCategoryDetails(string categoryCode);
        double GetViewCollectionByOperator(string operatorCode);
        Patient_Discharge GetTestCostDetails(string TestCode);
        dynamic GetipaDetails(string ipa_no, int? emrSiteId);


    }
}   
