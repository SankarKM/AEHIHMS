using IHMS.Data.Common;
using IHMS.Data.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IHMS.Data.Repository.Implementation
{
    public class PatientRepository : RepositoryBase<Patient>, IPatientRepository
    {
        private readonly IHMSContext _context;
        public PatientRepository(IHMSContext context) : base(context)
        {
            _context = context;
        }

        //public async Task<IEnumerable<Dropdown>> GetPatientsForDropDown()
        //{
        //    return await _context.Patients.Select(x => new Dropdown { Text = x.Patient_Name, Value = Convert.ToString(x.Id) }).ToListAsync();
        //}

    }
}
