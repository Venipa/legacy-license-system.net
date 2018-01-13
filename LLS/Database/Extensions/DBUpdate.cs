using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Database.Extensions
{
    public static class DBUpdate
    {
        public static void Update<TEntity>(this Context context, TEntity v) where TEntity : class
        {
            context.Entry(v).State = EntityState.Modified;
        }
    }
}
