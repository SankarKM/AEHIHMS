using IHMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IHMS.Data.Repository.Implementation
{
    public class RoomTransferRepository : RepositoryBase<IPAdmission>, IRoomTransferRepository
    {
        private readonly IHMSContext _context;
        private readonly CommonRepository _commonRepository;
        public RoomTransferRepository(IHMSContext context) : base(context)
        {
            _context = context;
            _commonRepository = new CommonRepository(context);
        }

        public Patient_Registration GetPatientDetail(string Mr_No)
        {
            Patient_Registration patientRegistration = new Patient_Registration();
            patientRegistration.Master = Context.Patient_Registration_Master.Where(r => r.MR_NO == Mr_No).FirstOrDefault();

            patientRegistration.Master.District = Context.District_Master.Where(x => x.District_Code == patientRegistration.Master.District).Select(x => x.District_Name).FirstOrDefault();
            patientRegistration.Master.State = Context.State_Master.Where(x => x.State_Code == patientRegistration.Master.State).Select(x => x.State_Name).FirstOrDefault();

            patientRegistration.PermanentAddress = Context.Patient_Registration_Master_Address.Where(r => r.MR_NO == Mr_No).FirstOrDefault();
            return patientRegistration;
        }

        public IEnumerable<IPAdmission> GetAllPatients(string SearchText)
        {
            return Context.Ip_Admission.Where(r => r.Discharge_Status == "ADM" && (r.Mr_No.Contains(SearchText) || r.UIN.Contains(SearchText)));
        }

        public IEnumerable<CurrentRoomStatus> GetCurrentRoomStatus(string RoomType, string Floor, string ToiletType)
        {

            return (string.IsNullOrEmpty(RoomType) && string.IsNullOrEmpty(Floor) && string.IsNullOrEmpty(ToiletType)) ? Context.Current_Room_Status : Context.Current_Room_Status.Where(r => r.Room_Type == RoomType && r.Floor_Code == Floor && r.Toilet_Type_Code == ToiletType);

        }

        public IpSurgeryDtl GetSurgeryDetail(string SurgeryCode,string IPANo)
        {
            return Context.Ip_Surgery_Dtl.Where(r => r.Surgery_Code == SurgeryCode && r.Ipa_No == IPANo).FirstOrDefault();
        }

        public List<RoomVacancy> GetExpectedRoomVacancy(DateTime FromDate, DateTime ToDate)
        {
            List<RoomVacancy> roomVacancies = new List<RoomVacancy>();
            RoomVacancy rm = new RoomVacancy();
            var roomVacancy = Context.Current_Room_Status.Where(r => r.Expected_Discharge_Date.Value == FromDate.Date).GroupBy(x => x.Room_Type);
            var roomVacancyTo = Context.Current_Room_Status.Where(r => r.Expected_Discharge_Date.Value == ToDate.Date).GroupBy(x => x.Room_Type);
            foreach (var x in roomVacancy)
            {
                rm = new RoomVacancy();
                rm.RoomType = x.Key;
                rm.FromDate = x.ToList().Count().ToString();
                rm.ToDate = "0";
                roomVacancies.Add(rm);
            }
            foreach (var y in roomVacancyTo)
            {
                rm = new RoomVacancy();
                rm.RoomType = y.Key;
                rm.FromDate = "0";
                rm.ToDate = y.ToList().Count().ToString();
                roomVacancies.Add(rm);
            }
            return roomVacancies;
        }

        public dynamic UpdateRoomTransferDetails(RoomTransfer roomTransfer,string UIN)
        {
            try
            {
                roomTransfer.NOFTRANS = 1;
                var OldTransferDetails = _context.Room_Transfer.Where(r => r.MR_NO == roomTransfer.MR_NO && r.IPA_NO == roomTransfer.IPA_NO).LastOrDefault();
                if (OldTransferDetails != null && OldTransferDetails.NOFTRANS != null)
                {
                    roomTransfer.NOFTRANS += OldTransferDetails.NOFTRANS;
                    Context.Entry(OldTransferDetails).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                }



                //Add Room Transfer Details
                roomTransfer.OCCUPIED_DATE = DateTime.Now;
                roomTransfer.CHANGE_DATE = DateTime.Now;
                roomTransfer.STATUS = "ADM";
                Context.Room_Transfer.Add(roomTransfer);

                //Update IP Admission Detail
                var IpAdmission = _context.Ip_Admission.Where(r => r.Mr_No == roomTransfer.MR_NO && r.Ipa_No == roomTransfer.IPA_NO).FirstOrDefault();
                IpAdmission.Room_No = roomTransfer.TO_ROOM;
                IpAdmission.Room_Type = roomTransfer.TO_TYPE;
                IpAdmission.Room_Transfer = "Y";

                //Update Old Room Status
                var oldRoomStatus = _context.Current_Room_Status.Where(r => r.Mr_No == roomTransfer.MR_NO).FirstOrDefault();
                if (oldRoomStatus != null)
                {
                    oldRoomStatus.UIN = null;
                    oldRoomStatus.Mr_No = null;
                    oldRoomStatus.Occupy_Flag_Code = "OFC003";
                    oldRoomStatus.Occupied_Date = null;
                    oldRoomStatus.Expected_Discharge_Date = null;
                }

                if (Context.SaveChanges() > 0)
                {
                    //update Selected Room Details with New Patient
                    var CurrentRoomStatus = _context.Current_Room_Status.Where(r => r.Room_No == roomTransfer.TO_ROOM).FirstOrDefault();
                    CurrentRoomStatus.Mr_No = roomTransfer.MR_NO;
                    CurrentRoomStatus.UIN = UIN;
                    CurrentRoomStatus.Occupied_Date = DateTime.Now;
                    CurrentRoomStatus.Occupy_Flag_Code = "OFC001";// Flag Code for Occupied Status

                    if (Context.SaveChanges() > 0)
                    {
                        return new
                        {
                            Success = true,
                            Message = "Room Transfer is successful"
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return new
            {
                Success = true,
                Message = "Unable to Transfer Room"
            };
        }
    }
}
