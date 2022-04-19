using Jvedio.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Global
{
    public static class Plugins
    {
        public static List<PluginInfo> Crawlers;


        static Plugins()
        {
            Crawlers = new List<PluginInfo>();
        }


    }
}
