using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Plugins
{
    public class PluginInfo
    {


        public string ServerType { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Categories { get; set; }
        public string Image { get; set; }
        public string Author { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string MarkDown { get; set; }
        public string License { get; set; }
        public string PublishDate { get; set; }


        public static PluginInfo ParseDict(Dictionary<string, string> dict)
        {
            if (dict == null || dict.Count <= 0) return null;
            PluginInfo result = new PluginInfo();
            PropertyInfo[] propertyInfos = typeof(PluginInfo).GetProperties();
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                string name = propertyInfo.Name;
                if (dict.ContainsKey(name))
                    propertyInfo.SetValue(result, dict[name]);
            }
            return result;
        }

    }
}
