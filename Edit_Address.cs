using System;
using System.Collections.Generic;
using System.Text;

namespace IHMS.Data.Model.ViewModel
{
    public class Edit_Address
    {

        public PatientRegistrationMaster Master { get; set; }
        public PatientRegistrationMasterAddress PermanentAddress { get; set; }
        public Patient_Registration_Edit_Log PrLog { get; set; }
        public int Age { get; set; }
    }
}
