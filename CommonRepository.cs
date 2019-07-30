using IHMS.Data.Common;
using IHMS.Data.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IHMS.Data.Repository.Implementation
{
    public class CommonRepository : RepositoryBase<Patient>, ICommonRepository
    {
        private readonly IHMSContext _context;
        private readonly EMRContext _emrContext;
        public CommonRepository(IHMSContext context, EMRContext emrContext = null) : base(context)
        {
            _context = context;
            _emrContext = emrContext;
        }

        #region Dynamic Dropdown

        public async Task<IEnumerable<Dropdown>> GetDropdown(string tableName, string valueColumn, string textColumn, string whereColumn = null, string whereValue = null)
        {
            try
            {
                var table = (IQueryable)Context.GetType().GetProperty(tableName).GetValue(Context, null);

                KeyValuePair<PropertyInfo, PropertyInfo> sourceDestPropMap1 = new KeyValuePair<PropertyInfo, PropertyInfo>(
                    typeof(Dropdown).GetProperty("Text"), table.ElementType.GetProperty(textColumn));

                KeyValuePair<PropertyInfo, PropertyInfo> sourceDestPropMap2 = new KeyValuePair<PropertyInfo, PropertyInfo>(
                    typeof(Dropdown).GetProperty("Value"), table.ElementType.GetProperty(valueColumn));

                var paramExpr = Expression.Parameter(table.ElementType, "x");
                var propertyA = Expression.Property(paramExpr, sourceDestPropMap1.Value);
                var propertyB = Expression.Property(paramExpr, sourceDestPropMap2.Value);
                var propertyBToString = Expression.Call(propertyB, typeof(object).GetMethod("ToString"));

                object query = null;
                if (!string.IsNullOrWhiteSpace(whereColumn) && !string.IsNullOrWhiteSpace(whereValue))
                {
                    var whereProp = Expression.Property(paramExpr, whereColumn);
                    dynamic value;
                    if (whereProp.Type.FullName.Contains("System.Int"))
                        value = Convert.ToInt32(whereValue);
                    else
                        value = whereValue;
                    var filter = Expression.Lambda(Expression.Equal(Expression.Property(paramExpr, whereColumn), Expression.Constant(value)), paramExpr);
                    query = Call(Where.MakeGenericMethod(paramExpr.Type), table, filter);
                }
                var createObject = Expression.New(typeof(Dropdown));
                var initializePropertiesOnObject = Expression.MemberInit(
                    createObject,
                    new[]
                    {
                        Expression.Bind(sourceDestPropMap1.Key, propertyA),
                        Expression.Bind(sourceDestPropMap2.Key, propertyBToString)
                    });

                var selectExpression = Expression.Lambda(initializePropertiesOnObject, paramExpr);

                query = Call(Select.MakeGenericMethod(paramExpr.Type, typeof(Dropdown)), query != null ? query : table, selectExpression);
                var result = (IEnumerable<Dropdown>)query;
                return result.OrderBy(x => x.Text);
            }
            catch (Exception e)
            {

                throw;
            }
        }
        private static readonly MethodInfo Select = GetGenericMethodDefinition<
            Func<IQueryable<object>, Expression<Func<object, object>>, IQueryable<object>>>((source, selector) =>
            Queryable.Select(source, selector));
        private static readonly MethodInfo Where = GetGenericMethodDefinition<
            Func<IQueryable<object>, Expression<Func<object, bool>>, object>>((source, predicate) =>
            Queryable.Where(source, predicate));
        private static readonly MethodInfo FirstOrDefault = GetGenericMethodDefinition<
            Func<IQueryable<object>, object>>(source =>
            Queryable.FirstOrDefault(source));
        private static MethodInfo GetGenericMethodDefinition<TDelegate>(Expression<TDelegate> e)
        {
            return ((MethodCallExpression)e.Body).Method.GetGenericMethodDefinition();
        }
        private static object Call(MethodInfo method, params object[] parameters)
        {
            return method.Invoke(null, parameters);
        }
        #endregion

        public IEnumerable<Dropdown> GetAehLocations()
        {
            return _emrContext.EmrSiteConfiguration.Select(x => new Dropdown { Text = x.Location, Value = x.SiteId.ToString() }).OrderBy(x => x.Text).ToList();
        }

        public string GetTalukCodeByAreaCode(string areaCode)
        {
            var key = Convert.ToInt16(areaCode);
            return Context.Area_Master.Where(x => x.Area_Code == key).Select(x => x.Taluk_Code).FirstOrDefault();
        }

        public dynamic TalukChange(string talukCode)
        {
            var districtCode = _context.Taluk_Master.Where(x => x.Taluk_Code == talukCode).Select(x => x.District_Code).FirstOrDefault();
            var districtValue = _context.District_Master.Where(x => x.District_Code == districtCode).Select(x => new { key = x.District_Code, value = x.District_Name, stateCode = x.State_Code }).FirstOrDefault();
            var stateValue = _context.State_Master.Where(x => x.State_Code == districtValue.stateCode).Select(x => new { key = x.State_Code, value = x.State_Name, countryCode = x.Country_Code, x.Lang_Code }).FirstOrDefault();
            var countryValue = _context.Country_Master.Where(x => x.Country_Code == stateValue.countryCode).Select(x => new { key = x.Country_Code, value = x.Country_Name }).FirstOrDefault();

            return new
            {
                TalukCode = talukCode,
                District = new KeyValuePair<string, string>(districtValue.key, districtValue.value),
                State = new KeyValuePair<string, string>(stateValue.key, stateValue.value),
                Country = new KeyValuePair<string, string>(countryValue.key, countryValue.value),
                LangCode = stateValue.Lang_Code
            };
        }

        public Dictionary<string, string> GetTextPlaceholders(string pageName, string country, string state)
        {
            var textPlaceholderGlobal = Context.TextPlaceholderGlobal.Where(x => x.PageName == pageName).ToList();
            var globalText = textPlaceholderGlobal.ToDictionary(x => x.Text, y => y.Text);
            if (!string.IsNullOrWhiteSpace(country))
            {
                var globalTextId = textPlaceholderGlobal.Select(x => x.Id).ToList();
                var textPlaceholderConfig = Context.TextPlaceholderConfig.Where(x => globalTextId.Contains(x.TextPlaceholderGlobalId) && x.Country == country && x.State == (string.IsNullOrWhiteSpace(state) ? null : state)).ToDictionary(x => x.TextPlaceholderGlobal.Text, y => y.Text);
                var configText = textPlaceholderConfig.Select(x => x.Key).ToList();
                var unConfigText = globalText.Where(x => !configText.Contains(x.Key)).ToDictionary(x => x.Key, y => y.Value);
                return unConfigText.Union(textPlaceholderConfig).ToDictionary(x => x.Key, y => y.Value);
            }
            return globalText;
        }

        public string GenerateRunningCtrlNo(string rnControlCode, int siteId = 1)
        {
            var rn = Context.Running_Number_Control.FirstOrDefault(x => x.RnControl_Code == rnControlCode && x.IsActive == true && x.SiteId == siteId);
            rn.Control_Value += 1;
            Context.Entry(rn).State = EntityState.Modified;
            return $"{rn.Control_String_Value}{rn.Control_Value}";
        }

        public int GenerateRunningCtrlNoWithoutPrefix(string rnControlCode, int siteId = 1)
        {
            var rn = Context.Running_Number_Control.FirstOrDefault(x => x.RnControl_Code == rnControlCode && x.IsActive == true && x.SiteId == siteId);
            rn.Control_Value += 1;
            Context.Entry(rn).State = EntityState.Modified;
            return Convert.ToInt32(rn.Control_Value);
        }
    }
}
