using IHMS.Data.Common;
using IHMS.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IHMS.Data.Repository
{
    public interface IRepositoryBase<T>
    {
        IEnumerable<T> FindAll();
        IEnumerable<T> FindByCondition(Expression<Func<T, bool>> expression);
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
        bool Save();
        Task<IEnumerable<Dropdown>> GetDropdown(string tableName, string valueColumn, string textColumn, string whereColumn = null, string whereValue = null);
    }
}
