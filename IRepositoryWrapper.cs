using IHMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace IHMS.Data.Repository
{
    public interface IRepositoryWrapper
    {
        ICommonRepository Common { get; }
        IPatientRepository Patient { get; }
        IPatientRegistrationRepository PatientRegistration { get; }
        IPatientDischargeRepository PatientDischarge { get; }
        IEditAddressRepository EditAddress { get; }
        IPatientAdmissionRepository PatientAdmission { get; }
        IEditSurgeryRepository EditSugery { get; }
        IAddSurgeryRepository AddSugery { get; }
        IRoomClearanceRepository RoomClearance { get; }
        IRoomTransferRepository RoomTransfer { get; }
        IAppointmentRepository Appointment { get; }  
        ICampDischargeRepository Campdischarge { get; }
        ISelfRegistrationRepository Kiosk { get; }
        ICampAdmissionRepository CampAdmission { get; }
    } 
}
