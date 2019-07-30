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
    public interface IEditSurgeryRepository : IRepositoryBase<Edit_Surgery>
    {
       Edit_Surgery GetEditSurgeryPatient(string no, int siteId,bool isMrNo);
       dynamic UpdateEditSugery(Edit_Surgery EditSugery);
      
    }
}
