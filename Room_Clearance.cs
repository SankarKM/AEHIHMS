using System;
using System.Collections.Generic;
using System.Text;

namespace IHMS.Data.Model
{
    public class Room_Clearance
    {
        public string UIN { get; set; }
        public string Mr_No { get; set; }
        public string Patient_Name { get; set; }
        public string Floor_Code { get; set; }
        public string Room_No { get; set; }
        public string Room_Type { get; set; }
        public string Occupy_Flag_Code { get; set; }
        public string Occupy_Status { get; set; }
        public DateTime? Vacating_Time { get; set; }
        public DateTime? Expected_Discharge_Date { get; set; }
        public int Siteid { get; set; }
        public bool isChecked { get; set; }
    }
}
