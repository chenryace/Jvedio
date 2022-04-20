using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;
using static Jvedio.GlobalVariable;
using static Jvedio.FileProcess;
using Jvedio.Utils;
using Jvedio.Entity;
using Jvedio.Core;
using Jvedio.Core.Crawler;
using Jvedio.Core.Plugins;
using Newtonsoft.Json;

namespace Jvedio.ViewModel
{
    public class VieModel_Settings : ViewModelBase
    {
        Main main;
        public VieModel_Settings()
        {
            main = ((Main)FileProcess.GetWindowByName("Main"));
            DataBase = Path.GetFileNameWithoutExtension(Properties.Settings.Default.DataBasePath);

            // todo
            // DataBases = main?.vieModel.DataBases;
            ThemeList = new ObservableCollection<Theme>();
            foreach (Theme theme in ThemeLoader.Themes)
            {
                if (theme.Name == Skin.白色.ToString() || theme.Name == Skin.黑色.ToString() || theme.Name == Skin.蓝色.ToString()) continue;
                ThemeList.Add(theme);
            }

            setServers();
            setPlugins();

        }


        public void Reset()
        {
            //读取配置文件
            ScanPath = new ObservableCollection<string>();
            // todo
            //foreach (var item in ReadScanPathFromConfig(DataBase))
            //{
            //    ScanPath.Add(item);
            //}
            if (ScanPath.Count == 0) ScanPath = null;
            //GlobalVariable.InitVariable();
            Servers = new ObservableCollection<Server>();

            //Type type = JvedioServers.GetType();
            //foreach (var item in type.GetProperties())
            //{
            //    System.Reflection.PropertyInfo propertyInfo = type.GetProperty(item.Name);
            //    Server server = (Server)propertyInfo.GetValue(JvedioServers);
            //    if (server.Url != "")
            //        Servers.Add(server);
            //}
        }


        public void setPlugins()
        {
            InstalledPlugins = new ObservableCollection<PluginInfo>();
            foreach (PluginInfo plugin in Global.Plugins.Crawlers)
            {
                InstalledPlugins.Add(plugin);
            }
        }

        public void setServers()
        {
            CrawlerServers = new Dictionary<string, ObservableCollection<CrawlerServer>>();
            List<string> serverTypes = Global.Plugins.Crawlers.Select(item => item.ServerType).ToList();
            foreach (string serverType in serverTypes)
            {
                CrawlerServer crawlerServer = GlobalConfig.ServerConfig.CrawlerServers.Where(arg => arg.ServerType.ToLower() == serverType.ToLower()).FirstOrDefault();
                if (crawlerServer == null)
                {
                    crawlerServer = new CrawlerServer();
                    crawlerServer.ServerType = serverType;
                    CrawlerServers.Add(serverType, null);
                }
                else
                {
                    ObservableCollection<CrawlerServer> crawlers = new ObservableCollection<CrawlerServer>();
                    GlobalConfig.ServerConfig.CrawlerServers.Where(arg => arg.ServerType.ToLower() == serverType.ToLower()).
                        ToList().ForEach(t => crawlers.Add(t));
                    CrawlerServers.Add(serverType, crawlers);
                }

            }


            //foreach (CrawlerServer crawlerServer in crawlerServers)
            //{

            //    if (CrawlerServers.ContainsKey(crawlerServer.ServerType))
            //    {
            //        if (!string.IsNullOrEmpty(crawlerServer.Url))
            //            CrawlerServers[crawlerServer.ServerType].Add(crawlerServer);


            //    }
            //    else
            //    {
            //        if (string.IsNullOrEmpty(crawlerServer.Url))
            //        {
            //            CrawlerServers.Add(crawlerServer.ServerType, null);
            //        }
            //        else
            //        {
            //            ObservableCollection<CrawlerServer> servers = new ObservableCollection<CrawlerServer>() { crawlerServer };
            //            CrawlerServers.Add(crawlerServer.ServerType, servers);

            //        }

            //    }
            //}
            Console.WriteLine(CrawlerServers);
        }

        public bool SaveServers(Action<string> callback = null)
        {
            List<CrawlerServer> list = new List<CrawlerServer>();
            foreach (string key in CrawlerServers.Keys)
            {
                List<CrawlerServer> crawlerServers = CrawlerServers[key]?.ToList();
                if (crawlerServers == null || crawlerServers.Count <= 0) continue;
                foreach (CrawlerServer server in crawlerServers)
                {
                    if (!server.isHeaderProper())
                    {
                        string format = "{\"UserAgent\":\"value\",...}";
                        callback?.Invoke($"【{key}】 刮削器处地址为 {server.Url} 的 Headers 不合理，格式必须为：{format}");
                        return false;
                    }
                    server.ServerType = key;
                    if (server.Headers == null) server.Headers = "";
                    list.Add(server);

                }
            }
            GlobalConfig.ServerConfig.CrawlerServers = list;
            GlobalConfig.ServerConfig.Save();
            return true;
        }





        private string _ViewRenameFormat;

        public string ViewRenameFormat
        {
            get { return _ViewRenameFormat; }
            set
            {
                _ViewRenameFormat = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Theme> _ThemeList;

        public ObservableCollection<Theme> ThemeList
        {
            get { return _ThemeList; }
            set
            {
                _ThemeList = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<PluginInfo> _InstalledPlugins;

        public ObservableCollection<PluginInfo> InstalledPlugins
        {
            get { return _InstalledPlugins; }
            set
            {
                _InstalledPlugins = value;
                RaisePropertyChanged();
            }
        }
        private PluginInfo _CurrentPlugin;

        public PluginInfo CurrentPlugin
        {
            get { return _CurrentPlugin; }
            set
            {
                _CurrentPlugin = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<Server> _Servers;

        public ObservableCollection<Server> Servers
        {
            get { return _Servers; }
            set
            {
                _Servers = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> _ScanPath;

        public ObservableCollection<string> ScanPath
        {
            get { return _ScanPath; }
            set
            {
                _ScanPath = value;
                RaisePropertyChanged();
            }
        }


        //private Skin _Themes = (Skin)Enum.Parse(typeof(Skin), "黑色", true);

        //public Skin Themes
        //{
        //    get { return _Themes; }
        //    set
        //    {
        //        _Themes = value;
        //        RaisePropertyChanged();
        //    }
        //}


        //private MyLanguage _Language = (MyLanguage)Enum.Parse(typeof(MyLanguage), Properties.Settings.Default.Language, true);

        //public MyLanguage Language
        //{
        //    get { return _Language; }
        //    set
        //    {
        //        _Language = value;
        //        RaisePropertyChanged();
        //    }
        //}

        private string _DataBase;

        public string DataBase
        {
            get { return _DataBase; }
            set
            {
                _DataBase = value;
                RaisePropertyChanged();
            }
        }


        private ObservableCollection<string> _DataBases = new ObservableCollection<string>();

        public ObservableCollection<string> DataBases
        {
            get { return _DataBases; }
            set
            {
                _DataBases = value;
                RaisePropertyChanged();
            }
        }


        private Dictionary<string, ObservableCollection<CrawlerServer>> _CrawlerServers = new Dictionary<string, ObservableCollection<CrawlerServer>>();

        public Dictionary<string, ObservableCollection<CrawlerServer>> CrawlerServers
        {
            get { return _CrawlerServers; }
            set
            {
                _CrawlerServers = value;
                RaisePropertyChanged();
            }
        }






    }
}
