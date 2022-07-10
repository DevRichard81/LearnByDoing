using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace CSharpMagic
{
    public class DecodeCSharp
    {
        private BindingFlags defaultFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;
        public Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

        private List<PropertyInfo> GetPropertiesList(Type type)
        {
            return type.GetProperties(defaultFlags).ToList();
        }

        public void GetFields(Object obj)
        {
            Console.WriteLine(nameof(GetFields));
            var field = obj.GetType().GetFields(defaultFlags).OrderBy(s => s.Name).ToList();
            var propertiesAndValues = field.Select(p => new RefVariable(obj, p)).ToList();

            foreach (var propertiesAndValue in propertiesAndValues)
            {
                string tempName = String.Empty;
                object? tempValue = null;
                if (RefVariable.HasInterface(propertiesAndValue.Info, "IList"))
                {
                    var listSelectedProperties = GetProperties(obj, propertiesAndValue.Name);
                    foreach (var property in listSelectedProperties)
                    {
                        tempName = property.Name ?? String.Empty;
                        tempValue = property.Value;
                    }                    
                }
                else
                {
                    tempName = propertiesAndValue.Name ?? String.Empty;
                    tempValue = propertiesAndValue.Value;
                }
                keyValuePairs.TryAdd(tempName, Convert.ToString(tempValue));
            }
        }
        public List<RefVariable> GetProperties(Object obj, string name)
        {
            var tmpList = obj.GetType().GetProperties(defaultFlags).OrderBy(s => s.Name).ToList();
            return tmpList.Where(s=> s.Name == name).Select(p => new RefVariable(obj, p)).ToList();
        }

        public void GetProperties(Object obj, bool recal = false)
        {
            Console.WriteLine(nameof(GetProperties));
            var field = obj.GetType().GetProperties(defaultFlags).OrderBy(s => s.Name).ToList();
            var propertiesAndValues = field.Select(p => new RefVariable(obj, p)).ToList();
            foreach (var propertiesAndValue in propertiesAndValues)
            {
                string tempName = String.Empty;
                object? tempValue = null;
                if (RefVariable.HasInterface(propertiesAndValue.Info, "IList"))
                {
                    var listSelectedProperties = GetProperties(obj, propertiesAndValue.Name);
                    foreach (var property in listSelectedProperties)
                    {
                        tempName = property.Name ?? String.Empty;
                        tempValue = property.Value;
                    }
                }
                else
                {
                    tempName = propertiesAndValue.Name ?? String.Empty;
                    tempValue = propertiesAndValue.Value;
                }
                keyValuePairs.TryAdd(tempName, Convert.ToString(tempValue));
            }
        }

        public void GetObject(Object obj)
        {
            Console.WriteLine(nameof(GetObject));
            GetFields(obj);
            GetProperties(obj);
            Console.WriteLine("---- D - U - M - P ----");
            StringBuilder sb = new StringBuilder();
            var formatString = "{0,-30} <{1,0}>\r\n";
            foreach (var itm in keyValuePairs)
            {
                sb.AppendFormat(formatString, itm.Key, itm.Value);
            }
            Console.WriteLine(sb.ToString());
        }
    }
}