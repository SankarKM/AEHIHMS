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
    public interface ICampDischargeRepository : IRepositoryBase<Camp_Discharge>
    {
       Camp_Discharge GetCampDischarge(string no, int siteId, bool isIPNO);
        dynamic InsertCampDischarge(Camp_Discharge CampDischarge);
    }
}   
