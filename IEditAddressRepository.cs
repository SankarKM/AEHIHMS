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
    public interface IEditAddressRepository : IRepositoryBase<Edit_Address>
    {
       Edit_Address GetReviewPatient(string no, int siteId);
        dynamic UpdateEditAddress(Edit_Address EditAddress);
        dynamic AgeOrDboChange(string type, string value);
    }
}
