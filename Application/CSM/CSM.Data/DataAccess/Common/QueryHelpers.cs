using CSM.Entity.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CSM.Data.DataAccess.Common
{
    public class QueryHelpers
    {
        public static IQueryable<TEntity> ApplyPaging<TEntity>(IQueryable<TEntity> source, Pager pager)
        {
            // paging
            int startPageIndex = (pager.PageNo - 1) * pager.PageSize;
            pager.TotalRecords = source.Count();
            if (startPageIndex >= pager.TotalRecords)
            {
                startPageIndex = 0;
                pager.PageNo = 1;
            }

            // skip/take
            source = source.Skip(startPageIndex).Take(pager.PageSize);
            return source;
        }

        public static IQueryable<TEntity> SortIQueryable<TEntity>(IQueryable<TEntity> source, string orderByProperty, bool desc)
        {
            string command = desc ? "OrderByDescending" : "OrderBy";
            var type = typeof(TEntity);
            var property = type.GetProperty(orderByProperty);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);
            var resultExpression = Expression.Call(typeof(Queryable), command, new Type[] { type, property.PropertyType },
                                          source.Expression, Expression.Quote(orderByExpression));
            return source.Provider.CreateQuery<TEntity>(resultExpression);
        }
    }
}
