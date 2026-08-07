using System;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace RuntimeCardEngine
{
    public static class LuaHelpers
    {
        public static List<T> ToList<T>(object value)
        {
            if (value == null) return new List<T>();
            if (value is DynValue dv) value = DynValueToClr(dv);

            if (value is Table table)
            {
                var r = new List<T>();
                foreach (var pair in table.Pairs)
                {
                    object o = pair.Value.ToObject();
                    if (o is T t) r.Add(t);
                    else
                    {
                        try { r.Add((T)Convert.ChangeType(o, typeof(T))); } catch { r.Add(default); }
                    }
                }
                return r;
            }

            if (value is IEnumerable ie && !(value is string))
            {
                var r = new List<T>();
                foreach (var item in ie)
                {
                    if (item is T t) r.Add(t);
                    else
                    {
                        try { r.Add((T)Convert.ChangeType(item, typeof(T))); } catch { r.Add(default); }
                    }
                }
                return r;
            }

            try { return new List<T> { (T)Convert.ChangeType(value, typeof(T)) }; } catch { return new List<T>(); }
        }

        private static object DynValueToClr(DynValue dv)
        {
            if (dv.IsNil()) return null;
            if (dv.Type == DataType.Table) return dv.Table;
            if (dv.Type == DataType.UserData) return dv.ToObject();
            if (dv.Type == DataType.Number) return dv.Number;
            if (dv.Type == DataType.String) return dv.String;
            if (dv.Type == DataType.Boolean) return dv.Boolean;
            return dv.ToObject();
        }
    }
}