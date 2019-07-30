using IHMS.Data.Model;
using IHMS.Data.Model.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IHMS.Data.Repository.Implementation
{
    public class RepositoryWrapper : IRepositoryWrapper
    { 
        private IHMSContext _context;
        private EMRContext _emrContext;
        private ICommonRepository _common;   
        private IPatientRepository _patient;
        private IPatientRegistrationRepository _patientRegistration;
        private IPatientDischargeRepository _PatientDischarge;
        private IEditAddressRepository _EditAddress;
        private IPatientAdmissionRepository _patientAdmission;
        private IEditSurgeryRepository _EditSurgery;
        private IAddSurgeryRepository _AddSurgery;
        private IRoomClearanceRepository _roomClearance;
        private IRoomTransferRepository _roomTransfer;
        private IAppointmentRepository _Appointment;
        private ICampDischargeRepository _Campdischarge;
        private ISelfRegistrationRepository _kiosk;
        private ICampAdmissionRepository _CampAdmission;


        public RepositoryWrapper(IHMSContext context, EMRContext emrContext)
        {
            _context = context;
            _emrContext = emrContext;
        }

        public ICommonRepository Common
        {
            get
            {
                if (_common == null)
                {
                    _common = new CommonRepository(_context, _emrContext);
                }

                return _common;
            }
        }

        public IPatientRepository Patient
        {
            get
            {
                if (_patient == null)
                {
                    _patient = new PatientRepository(_context);
                }

                return _patient;
            }
        }

        public IPatientRegistrationRepository PatientRegistration
        {
            get
            {
                if (_patientRegistration == null)
                {
                    _patientRegistration = new PatientRegistrationRepository(_context);
                }

                return _patientRegistration;
            }
        }


        public IEditAddressRepository EditAddress
        {
            get
            {
                if (_EditAddress == null)
                {
                    _EditAddress = new EditAddressRepository(_context);
                }

                return _EditAddress;
            }
        }


        public IPatientDischargeRepository PatientDischarge
        {
            get
            {
                if (_PatientDischarge == null)
                {
                    _PatientDischarge = new PatientDischargeRepository(_context);
                }

                return _PatientDischarge;
            }
        }


        public IPatientAdmissionRepository PatientAdmission
        {
            get
            {
                if (_patientAdmission == null)
                {
                    _patientAdmission = new PatientAdmissionRepository(_context);
                }

                return _patientAdmission;
            }
        }


        public IEditSurgeryRepository EditSugery
        {
            get
            {
                if (_EditSurgery == null)
                {
                    _EditSurgery = new EditSurgeryRepository(_context);
                }

                return _EditSurgery;
            }
        }


        public IAddSurgeryRepository AddSugery
        {
            get
            {
                if (_AddSurgery == null)
                {
                    _AddSurgery = new AddSurgeryRepository(_context);
                }

                return _AddSurgery;
            }
        }

        public IRoomClearanceRepository RoomClearance
        {
            get
            {
                if (_roomClearance == null)
                {
                    _roomClearance = new RoomClearanceRepository(_context);
                }

                return _roomClearance;
            }
        }
        public IRoomTransferRepository RoomTransfer
        {
            get
            {
                if (_roomTransfer == null)
                {
                    _roomTransfer = new RoomTransferRepository(_context);
                }

                return _roomTransfer;
            }
        }
        public IAppointmentRepository Appointment
        {
            get
            {
                if (_Appointment == null)
                {
                    _Appointment = new AppointmentRepository(_context);
                }

                return _Appointment;
            }
        }
        public ICampDischargeRepository Campdischarge
        {
            get
            {
                if (_Campdischarge == null)
                {
                    _Campdischarge = new CampDischargeRepository(_context);
                }

                return _Campdischarge;
            }
        }
        public ISelfRegistrationRepository Kiosk
        {
            get
            {
                if (_kiosk == null)
                {
                    _kiosk = new SelfRegistrationRepository(_context);
                }

                return _kiosk;
            }
        }

        public ICampAdmissionRepository CampAdmission
        {
            get
            {
                if (_CampAdmission == null)
                {
                    _CampAdmission = new CampAdmissionRepository(_context);
                }

                return _CampAdmission;
            }
        }



    }
}