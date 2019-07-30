using IHMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IHMS.Data.Repository.Implementation
{
    public class RoomClearanceRepository : RepositoryBase<Room_Clearance>, IRoomClearanceRepository
    {
        private readonly IHMSContext _context;
        private readonly CommonRepository _commonRepository;
        public RoomClearanceRepository(IHMSContext context) : base(context)
        {
            _context = context;
            _commonRepository = new CommonRepository(context);
        }

        public IEnumerable<Room_Clearance> GetAllRoomStatus(string filter, string occupyStatus)
        {
            IEnumerable<Room_Clearance> room_Clearance;

            room_Clearance = (from c in _context.Current_Room_Status
                              join p in _context.Patient_Registration_Master on c.Mr_No equals p.MR_NO
                              join o in _context.OCCUPY_FLAG_MASTER on c.Occupy_Flag_Code equals o.Occupy_Flag_Code
                              select new Room_Clearance
                              {
                                  UIN = p.UIN,
                                  Mr_No = p.MR_NO,
                                  Patient_Name = p.Patient_Name,
                                  Floor_Code = c.Floor_Code,
                                  Room_No = c.Room_No,
                                  Room_Type = c.Room_Type,
                                  Occupy_Flag_Code = c.Occupy_Flag_Code,
                                  Occupy_Status = o.Flag_Description,
                                  Vacating_Time = c.Vacating_Time,
                                  Expected_Discharge_Date = c.Expected_Discharge_Date,
                                  Siteid = c.Siteid,
                                  isChecked = false
                              }).ToList();

            occupyStatus = occupyStatus == null ? string.Empty : occupyStatus;

            room_Clearance = filter == "All" ? room_Clearance.Where(r => occupyStatus.Contains(r.Occupy_Flag_Code)) : room_Clearance.Where(r => r.Floor_Code == filter && occupyStatus.Contains(r.Occupy_Flag_Code));

            return room_Clearance;
        }

        public dynamic UpdateRoomClearanceStatus(List<Room_Clearance> RoomClearance)
        {
            foreach (Room_Clearance room_Clearance in RoomClearance)
            {
                //Get Current Room Status based MR No
                var currentRoomStatus = _context.Current_Room_Status.FirstOrDefault(r => r.Mr_No == room_Clearance.Mr_No && r.Room_No == room_Clearance.Room_No);
                currentRoomStatus.Occupy_Flag_Code = room_Clearance.Occupy_Flag_Code;
                currentRoomStatus.Mr_No = null;
                currentRoomStatus.Vacating_Time = DateTime.Now;
                currentRoomStatus.Occupied_Date = null;
                currentRoomStatus.Expected_Discharge_Date = null;

                //Insert Room Clear Data in DB
                var RoomClear = new RoomClear();
                RoomClear.OPERATOR_CODE = "37";//Need to update logged in user ID
                RoomClear.ROOM_NO = room_Clearance.Room_No;
                RoomClear.DATE = DateTime.Now.Date;
                RoomClear.SYSDATE = DateTime.Now;
                _context.Room_Clear.Add(RoomClear);

            }
            try
            {
                if (Context.SaveChanges() > 0)
                    return new
                    {
                        Success = true,
                        Message = "Room clearance is successful"
                    };
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return new
            {
                Success = true,
                Message = "Unable to Update Room Clearance Details"
            };
        }
    }
}
