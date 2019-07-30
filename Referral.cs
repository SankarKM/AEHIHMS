using IHMS.Data.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IHMS.Data.Model
{
    public class ReferralViewModel
    {
        public ReferralPatientsHDR Referral { get; set; }

        public string PatientName { get; set; }
        public string NextOfKin { get; set; }
        public string PatientDetail { get; set; }
        public string Address { get; set; }
        public bool Success { get; set; }
        public dynamic VisitDetails { get; set; }
    }
}
