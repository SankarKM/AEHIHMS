using IHMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace IHMS.Data.Repository
{
    public interface IRoomTransferRepository
    {
        Patient_Registration GetPatientDetail(string Mr_No);
        IpSurgeryDtl GetSurgeryDetail(string SurgeryCode,string IPANo);
        IEnumerable<CurrentRoomStatus> GetCurrentRoomStatus(string RoomType, string Floor, string ToiletType);
        IEnumerable<IPAdmission> GetAllPatients(string searchText);
        dynamic UpdateRoomTransferDetails(RoomTransfer roomTransfer,string UIN);
        List<RoomVacancy> GetExpectedRoomVacancy(DateTime FromDate, DateTime ToDate);
    }
}
