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
    public interface ISelfRegistrationRepository : IRepositoryBase<Self_Registration>
    {

        dynamic UpdateSelfregistration(Self_Registration Kiosk);
        dynamic AgeOrDboChange(string type, string value);

        dynamic UploadImage(IFormFile file, string patientname);

        string GetPatientImagePath(string patientname);
    }
}
