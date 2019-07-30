using System;
using System.Collections.Generic;
using System.Text;

namespace IHMS.Data.Model.ViewModel
{
    public class Self_Registration
    {
        public PatientRegistrationMaster Master { get; set; }
        public PatientRegistrationDetail Detail { get; set; }
        public Kiosk Kiosk { get; set; }

        public string Age { get; set; }
    }
}
