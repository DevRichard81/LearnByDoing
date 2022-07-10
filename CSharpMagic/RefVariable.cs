using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CSharpMagic
{
    public class RefVariable
    {
        public MemberInfo Info { get; }
        public object? Value { get; }
        public string? NameFull { get { return Info?.Name; } }
        public string? Name 
        {
            get 
            {
                if(Info == null)
                    return string.Empty;

                var start = Info.Name.IndexOf('<')+1;
                var end = Info.Name.IndexOf('>')-1;

                if(end > start)
                    return Info?.Name.Substring(start, end); 
                else
                    return Info?.Name;
            } 
        }  
        public string? toString { get { return Info?.ToString(); } }

        public RefVariable(object source, MemberInfo info)
        {
            TryGetValue(info, source, out object? foundValue);
            this.Value = foundValue;
            this.Info = info;
        }

        public static void TryGetValue(MemberInfo info, object element, out object? value)
        {
            if (info == null)
                value = new string($"{nameof(info)}: is null");

            try
            {
                if (info is PropertyInfo) {
                    var tmpValues = ((PropertyInfo)info).GetValue(element);
                    IEnumerable enumerable = tmpValues as IEnumerable;
                    if (enumerable != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach(var itm in enumerable)
                        {
                            if (sb.Length != 0)
                                sb.Append(",");
                            sb.Append(itm);
                        }
                        value = sb.ToString();
                    }
                    else
                        value = ((PropertyInfo)info).GetValue(element); 
                }
                else if (info is FieldInfo)
                    value = ((FieldInfo)info).GetValue(element);
                else
                    value = new string($"Unknow Info type <{info?.GetType().FullName}>");
                return;
            }
            catch (Exception ex)
            {
                value = new string($"{ex.GetType().Name}: {ex.Message}");
                return;
            }
        }       
        public static bool HasInterface(MemberInfo Info, string InterrfaceName)
        {
            if (Info is PropertyInfo)
                return ((PropertyInfo)Info).PropertyType.GetInterface(InterrfaceName) != null;
            else if (Info is FieldInfo)
                return ((FieldInfo)Info).FieldType.GetInterface(InterrfaceName) != null;

            return false;           
        }
    }
}
