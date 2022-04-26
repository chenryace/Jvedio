﻿using Jvedio.Core.Plugins;
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
        public long PicPathMode { get; set; }
        public string PicPathJson { get; set; }

        public Dictionary<string, object> PicPaths;
        public string PluginEnabledJson { get; set; }
        public Dictionary<string, bool> PluginEnabled;
        public bool DownloadPreviewImage { get; set; }
        public bool OverrideInfo { get; set; }
        public bool AutoHandleHeader { get; set; }
        public long TabControlSelectedIndex { get; set; }
        public bool OpenDataBaseDefault { get; set; }
        public bool CloseToTaskBar { get; set; }
        public long DefaultDBID { get; set; }

        /// <summary>
        /// 0-中文 1-English 2-日語
        /// </summary>
        public long SelectedLanguage { get; set; }
        public bool SaveInfoToNFO { get; set; }
        public bool OverriteNFO { get; set; }
        public string NFOSavePath { get; set; }


    }
}
