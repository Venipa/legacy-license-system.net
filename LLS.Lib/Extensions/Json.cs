using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LLS.Lib.Extensions
{
    public static class Converters
    {
        public static object Cast(this object obj, Type t)
        {
            try
            {
                return Convert.ChangeType(obj, t);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached) Debug.WriteLine(ex);
            }
            return null;
        }
    }
    public static class JsonStringExtension
    {
        public static object ToModel(this string json, Type type)
        {
            try
            {

                return JsonConvert.DeserializeObject(json, type);
            }
            catch (Exception jex)
            {
                if (Debugger.IsAttached) Debug.WriteLine(jex);
                return null;
            }
        }
        public static T ToModel<T>(this string json)
        {
            try
            {
                if (json == null) return default(T);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception jex)
            {
                if (Debugger.IsAttached) Debug.WriteLine(jex);
                return default(T);
            }
        }
        public static string ToJsonString<T>(this T v)
        {
            try
            {
                return JsonConvert.SerializeObject(v);
            }
            catch (Exception rex)
            {
                if (Debugger.IsAttached) Debug.WriteLine(rex);
                return null;
            }
        }
    }
}
