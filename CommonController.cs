using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IHMS.Data.Common;
using IHMS.Data.Model;
using IHMS.Data.Repository;
using IHMS.Data.Repository.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiCore.Controllers
{
    [Route("[controller]")]
    public class CommonController : Controller
    {
        private IRepositoryWrapper _repoWrapper;

        public CommonController(IRepositoryWrapper repoWrapper)
        {
            _repoWrapper = repoWrapper;
        }

        [HttpGet("getDropdown/{tableName}/{valueColumn}/{textColumn}/{whereColumn?}/{whereValue?}")]
        public async Task<IEnumerable<Dropdown>> GetDropdown(string tableName, string valueColumn, string textColumn, string whereColumn = null, string whereValue = null)
        {
            return await _repoWrapper.Common.GetDropdown(tableName, valueColumn, textColumn, whereColumn, whereValue);
        }

        [HttpGet("talukChange/{talukCode}/{isArea?}")]
        public dynamic TalukChange(string talukCode, bool isArea = false)
        {
            if (isArea)
                talukCode = _repoWrapper.Common.GetTalukCodeByAreaCode(talukCode);
            return _repoWrapper.Common.TalukChange(talukCode);
        }

        [HttpGet("getTextPlaceholders/{pageName}/{country?}/{state?}")]
        public dynamic GetTextPlaceholders(string pageName, string country = "", string state = "")
        {
            return _repoWrapper.Common.GetTextPlaceholders(pageName, country, state);
        }

        [HttpGet("getAehLocations")]
        public IEnumerable<Dropdown> GetAehLocations()
        {
            return _repoWrapper.Common.GetAehLocations();
        }
    }
}
