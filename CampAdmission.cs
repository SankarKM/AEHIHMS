using System;
using System.Collections.Generic;
using System.Text;

namespace IHMS.Data.Model.ViewModel
{
    public class CampAdmission
    {     
        public ICollection<Patientlist> Patientlist { get; set; }
        public ICollection<PatientHistory> History { get; set; }
        public PatientHistory PatientHistory { get; set; }
        public FreeAccommodationsMaster FreeAccommodationsMaster { get; set; }
        public StateMaster stateMaster { get; set; }
        public IPAdmission IPAdmission { get; set; }
        public PatientRegistrationMaster Master { get; set; }
        public ICollection<PatientAdmissionDetails> PatientAdmissionDetails { get; set; }

        public string uin { get; set; }
        public string mr_no { get; set; }
        public string Campname { get; set; }
        public string ipno { get; set; }

        public int Site_ID { get; set; }
        public string Operator_ID { get; set; }
        public string Module_ID { get; set; }
        
        public string Camp_Code { get; set; }
        public DateTime Camp_Date { get; set; }
        public DateTime Admission_Date { get; set; }
        public string Admission_Code { get; set; }
        public string Admission_Place { get; set; }
        public string Room_No { get; set; }
        public string Vacant_Beds { get; set; }
        public string Allotted_Beds { get; set; }

        public string Patient_Name { get; set; }
        public int Age { get; set; }
        public DateTime Patient_DOB { get; set; }
        public string NextOfKin { get; set; }
        public string Next_Of_Kin_Prefix { get; set; }
        public string Next_Of_Kin_Suffix { get; set; }
        public string Gender { get; set; }
        public string Phone_No { get; set; }
        public string Relation_Code { get; set; }

        public string IDProof_Code { get; set; }
        public string IDProof_Desc { get; set; }

        public string Door { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Pincode { get; set; }
        public string Taluk_Code { get; set; }
        public string DistrictDesc { get; set; }
        public string StateDesc { get; set; }
        public string ContryDesc { get; set; }

        public string DistrictCode { get; set; }
        public string StateCode { get; set; }
        public string ContryCode { get; set; }

        public int Caste_Code { get; set; }

        public string Eye_Code { get; set; }
        public string Surgery_Code { get; set; }
        public string Lens_Code { get; set; }


        public string C_Place { get; set; }
        public string C_TalukDesc { get; set; }
        public string C_DistrictDesc { get; set; }
        public string C_StateDesc { get; set; }
        public string C_CountryDesc { get; set; }

        public double TotalAdmission { get; set; }
        public double TotalAdmissionMale { get; set; }
        public double TotalAdmissionFemale { get; set; }

        public string IsMATChecked { get; set; }
        public string IsCOTChecked { get; set; }

        public decimal bedCharges { get; set; }

    }

}
   
    public class Patientlist
    {
        public string UIN { get; set; }
        public string MRNO { get; set; }
        public string PatientName { get; set; }
        public int DOB { get; set; }
        //public int Age { get; set; }
        public string Gender { get; set; }
        public string NextofKin { get; set; }
        public string PhoneNo { get; set; }
    }
