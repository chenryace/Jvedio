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
            List<CrawlerServer> crawlerServers = GlobalConfig.ServerConfig.CrawlerServers;
            foreach (CrawlerServer crawlerServer in crawlerServers)
            {

                if (CrawlerServers.ContainsKey(crawlerServer.ServerType))
                {
                    if (!string.IsNullOrEmpty(crawlerServer.Url))
                        CrawlerServers[crawlerServer.ServerType].Add(crawlerServer);


                }
                else
                {
                    if (string.IsNullOrEmpty(crawlerServer.Url))
                    {
                        CrawlerServers.Add(crawlerServer.ServerType, null);
                    }
                    else
                    {
                        ObservableCollection<CrawlerServer> servers = new ObservableCollection<CrawlerServer>() { crawlerServer };
                        CrawlerServers.Add(crawlerServer.ServerType, servers);

                    }

                }
            }
            Console.WriteLine(CrawlerServers);
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


        private Skin _Themes = (Skin)Enum.Parse(typeof(Skin), "黑色", true);

        public Skin Themes
        {
            get { return _Themes; }
            set
            {
                _Themes = value;
                RaisePropertyChanged();
            }
        }


        private MyLanguage _Language = (MyLanguage)Enum.Parse(typeof(MyLanguage), Properties.Settings.Default.Language, true);

        public MyLanguage Language
        {
            get { return _Language; }
            set
            {
                _Language = value;
                RaisePropertyChanged();
            }
        }

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
