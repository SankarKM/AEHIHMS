using IHMS.Data.Common;
using IHMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IHMS.Data.Repository
{
    public interface IPatientRepository : IRepositoryBase<Patient>
    {
        //Task<IEnumerable<Dropdown>> GetPatientsForDropDown();
    }
}
