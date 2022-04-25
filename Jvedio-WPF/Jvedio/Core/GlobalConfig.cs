using Jvedio.Core.Enums;
using Jvedio.Core.WindowConfig;
using Newtonsoft.Json;
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
        public static Jvedio.Core.Config.ProxyConfig ProxyConfig = Jvedio.Core.Config.ProxyConfig.createInstance();
        public static Jvedio.Core.Config.ScanConfig ScanConfig = Jvedio.Core.Config.ScanConfig.createInstance();

        static GlobalConfig()
        {
            StartUp.Read();
            Main.Read();
            Edit.Read();
            Detail.Read();
            MetaData.Read();
            ServerConfig.Read();
            Settings.Read();
            ProxyConfig.Read();
            ScanConfig.Read();
            EnsurePicPaths();// 确保 PicPaths
        }

        public static void Init()
        {
            Console.WriteLine("初始化");
        }

        static void EnsurePicPaths()
        {
            if (string.IsNullOrEmpty(Settings.PicPathJson))
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                dict.Add(PathType.Absolute.ToString(), GlobalVariable.PicPath);
                dict.Add(PathType.RelativeToApp.ToString(), "./Pic");

                Dictionary<string, string> d = new Dictionary<string, string>();
                d.Add("BigImagePath", "./.*fanart");
                d.Add("SmallImagePath", "./.*poster");
                d.Add("PreviewImagePath", "./.*预览图");
                d.Add("ScreenShotPath", "./.*截图");
                d.Add("ActorImagePath", "");
                dict.Add(PathType.RelativeToData.ToString(), d);
                Settings.PicPathJson = JsonConvert.SerializeObject(dict);
                Settings.PicPaths = dict;
            }
            else
            {
                Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Settings.PicPathJson);

                string str = dictionary[PathType.RelativeToData.ToString()].ToString();
                dictionary[PathType.RelativeToData.ToString()] = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
                Settings.PicPaths = dictionary;


            }
        }
    }
}
