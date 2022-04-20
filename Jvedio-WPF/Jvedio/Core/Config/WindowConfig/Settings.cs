using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jvedio.Core.WindowConfig
{
    public class Settings : AbstractConfig
    {
        private Settings() : base($"WindowConfig.Settings")
        {

        }

        private static Settings _instance = null;

        public static Settings createInstance()
        {
            if (_instance == null) _instance = new Settings();

            return _instance;
        }
        public long CrawlerSelectedIndex { get; set; }


    }
}
