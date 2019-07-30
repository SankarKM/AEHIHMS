using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IHMS.Data.Model
{
    public class RoomTransfer
    {
        [Key]
        public string MR_NO { get; set; }
        public string IPA_NO { get; set; }
        public string FROM_TYPE { get; set; }
        public string TO_TYPE { get; set; }
        public string FROM_ROOM { get; set; }
        public string TO_ROOM { get; set; } 
        public DateTime OCCUPIED_DATE { get; set; }
        public DateTime CHANGE_DATE { get; set; }
        public string STATUS { get; set; } 
        public int? NOFTRANS { get; set; }
    }
}
