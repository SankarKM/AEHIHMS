using System;
using System.Collections.Generic;
using System.Text;

namespace IHMS.Data.Model.ViewModel
{
    public class Edit_Surgery
    {



        public IPAdmission IPAdmission { get; set; }
        public IPClinic IPClinic { get; set; }
        public EyeMaster EyeMaster { get; set; }
        public OTSurgeryHdr OTSurgeryHdr { get; set; }
        public IpSurgeryDtl IpSurgeryDtl { get; set; }
        public ICDCodeMaster ICDCodeMaster { get; set; }
        public PatientRegistrationMaster Master { get; set; }
        public LocationMaster location { get; set; }
        public DoctorMaster DoctorMaster { get; set; }
        public SiteMaster SiteMaster { get; set; }
        public int Age { get; set; }
    }
}
