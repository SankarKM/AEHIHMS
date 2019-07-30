using System;
using System.Collections.Generic;
using System.Text;

namespace IHMS.Data.Model.ViewModel
{
    public class Appointment
    {


        public PatientRegistrationMaster Master { get; set; }
        public IPAdmission Admission { get; set; }


        public PatientRegistrationDetail PatientRegistrationDetail { get; set; }
        public Appointments Appointments { get; set; }
        public PatientStatus PatientStatus { get; set; }
        public EmrwfVisitPurposeServiceMaster EmrwfVisitPurposeServiceMaster { get; set; }
        public OpVisitDatewise OpVisitDatewise { get; set; }
        public int Age { get; set; }
        public int PurposeId { get; set; }

    } 
}
