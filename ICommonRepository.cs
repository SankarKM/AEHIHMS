using IHMS.Data.Common;
using IHMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IHMS.Data.Repository
{
    public interface ICommonRepository : IRepositoryBase<Patient>
    {
        Task<IEnumerable<Dropdown>> GetDropdown(string tableName, string valueColumn, string textColumn, string whereColumn = null, string whereValue = null);
        IEnumerable<Dropdown> GetAehLocations();
        string GetTalukCodeByAreaCode(string areaCode);
        dynamic TalukChange(string talukCode);
        Dictionary<string, string> GetTextPlaceholders(string pageName, string country, string state);
        string GenerateRunningCtrlNo(string rnControlCode, int siteId);
    }
}
