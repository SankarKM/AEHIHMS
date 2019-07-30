using IHMS.Data.Common;
using IHMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHMS.Data.Repository
{
    public interface IRoomClearanceRepository
    {
        IEnumerable<Room_Clearance> GetAllRoomStatus(string filter,string OccupyStatus);

        dynamic UpdateRoomClearanceStatus(List<Room_Clearance> patientAdmission);
    }
}
