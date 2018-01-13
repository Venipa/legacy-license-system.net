using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Lib.Extensions
{
    public static class Models
    {
        public static T ToType<T>(this object v)
        {
            return (T)Convert.ChangeType(v, typeof(T));
        }
    }
}
