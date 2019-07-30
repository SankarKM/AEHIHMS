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
    public interface IAddSurgeryRepository : IRepositoryBase<Add_Surgery>
    {
       Add_Surgery GetAddSurgeryPatient(string no, int siteId,bool isMrNo);
       dynamic UpdateAddSugery(Add_Surgery AddSugery);
      
    }
}
