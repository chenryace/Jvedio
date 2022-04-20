using Jvedio.Core.WindowConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio
{
    public static class GlobalConfig
    {
        public static StartUp StartUp = StartUp.createInstance();
        public static Core.WindowConfig.Main Main = Core.WindowConfig.Main.createInstance();
        public static Edit Edit = Edit.createInstance();
        public static Detail Detail = Detail.createInstance();
        public static MetaData MetaData = MetaData.createInstance();
        public static Jvedio.Core.WindowConfig.Settings Settings = Jvedio.Core.WindowConfig.Settings.createInstance();
        public static Jvedio.Core.Config.ServerConfig ServerConfig = Jvedio.Core.Config.ServerConfig.createInstance();

        static GlobalConfig()
        {
            StartUp.Read();
            Main.Read();
            Edit.Read();
            Detail.Read();
            MetaData.Read();
            ServerConfig.Read();
            Settings.Read();
        }
    }
}
