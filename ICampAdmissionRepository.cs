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
    public interface ICampAdmissionRepository : IRepositoryBase<CampAdmission>
    {
        CampAdmission GetCampAdmission(string CampCode, int siteId);
        dynamic ToDBCampAdmission(CampAdmission CampAdmission);
        dynamic RoomChangeEvent(string RoomType, string RoomNo);

        CampAdmission GetpopDetails(string MR_NO, int? emrSiteId);
        dynamic GetipaDetails(string ipa_no, int? emrSiteId);
    }
}   
