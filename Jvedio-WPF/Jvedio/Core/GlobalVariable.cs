﻿
using Jvedio.Core.Enums;
using Jvedio.Core.SimpleORM;
using Jvedio.Core.DataBase;
using Jvedio.Entity;
using Jvedio.Mapper;
using Jvedio.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jvedio.Entity.CommonSQL;

namespace Jvedio
{
    public static class GlobalVariable
    {
        static GlobalVariable()
        {
            LoadBgImage();
        }


        public delegate void ThemeChangeHandler();
        public static event ThemeChangeHandler ThemeChange;


        public const char Separator = (char)007;


        // *************** 网址 ***************
        public static readonly string ReleaseUrl = "https://github.com/hitchao/Jvedio/releases";
        public static readonly string YoudaoUrl = "https://github.com/hitchao/Jvedio/wiki/HowToSetYoudaoTranslation";
        public static readonly string BaiduUrl = "https://github.com/hitchao/Jvedio/wiki/HowToSetBaiduAI";
        public static readonly string UpgradeSource = "https://hitchao.github.io";
        public static readonly string UpdateUrl = "https://hitchao.github.io/jvedioupdate/Version";
        public static readonly string UpdateExeVersionUrl = "https://hitchao.github.io/jvedioupdate/update";
        public static readonly string UpdateExeUrl = "https://hitchao.github.io/jvedioupdate/JvedioUpdate.exe";
        public static readonly string NoticeUrl = "https://hitchao.github.io/JvedioWebPage/notice";
        public static readonly string FeedBackUrl = "https://github.com/hitchao/Jvedio/issues";
        public static readonly string WikiUrl = "https://github.com/hitchao/Jvedio/wiki";
        public static readonly string WebPageUrl = "https://hitchao.github.io/JvedioWebPage/";
        public static readonly string ThemeDIY = "https://hitchao.github.io/JvedioWebPage/theme.html";




        // *************** 目录 ***************
        public static string CurrentUserFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", Environment.UserName);// todo CurrentUserFolder 不一定能创建
        public static string oldDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase");// Jvedio 5.0 之前的
        public static string AllOldDataPath = Path.Combine(CurrentUserFolder, "olddata");


        public static string BackupPath = Path.Combine(CurrentUserFolder, "backup");
        public static string LogPath = Path.Combine(CurrentUserFolder, "log");
        public static string PicPath = Path.Combine(CurrentUserFolder, "pic");
        public static string BasePicPath = "";
        public static string ProjectImagePath = Path.Combine(CurrentUserFolder, "image", "library");
        public static string TranslateDataBasePath = Path.Combine(CurrentUserFolder, "Translate.sqlite");
        public static string BasePluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
        public static string ScanConfigPath = Path.Combine(CurrentUserFolder, "ScanPathConfig.xml");
        public static string ServersConfigPath = Path.Combine(CurrentUserFolder, "ServersConfigPath.xml");

        public static string UserConfigPath = Path.Combine(CurrentUserFolder, "user-config.xml");

        public static string[] PicPaths = new[] { "ScreenShot", "SmallPic", "BigPic", "ExtraPic", "Actresses", "Gif" };
        public static string[] InitDirs = new[] {
            BackupPath,LogPath,PicPath,ProjectImagePath,AllOldDataPath,
           Path.Combine(BasePluginsPath,"themes"), Path.Combine(BasePluginsPath,"crawlers") };//初始化文件夹

        // *************** 目录 ***************



        // *************** 数据库***************
        /// <summary>
        /// 如果是 sqlite => xxx.sqlite ；如果是 Mysql/PostgreSql => 数据库名称：xxx
        /// 使用 SQLITE 存储用户的配置，用户的数据可以采用多数据库形式
        /// DB_TABLENAME_JVEDIO_DATA ,对于 SQLITE 来说是文件名，对于 Mysql 来说是库名
        /// </summary>
        public static readonly string DB_TABLENAME_APP_CONFIG = Path.Combine(CurrentUserFolder, "app_configs");
        public static readonly string DB_TABLENAME_APP_DATAS = Path.Combine(CurrentUserFolder, "app_datas");
        public static readonly string DEFAULT_SQLITE_PATH =
            Path.Combine(CurrentUserFolder, DB_TABLENAME_APP_DATAS + ".sqlite");
        public static readonly string DEFAULT_SQLITE_CONFIG_PATH =
            Path.Combine(CurrentUserFolder, DB_TABLENAME_APP_CONFIG + ".sqlite");

        public static DataBaseType CurrentDataBaseType = DataBaseType.SQLite;




        // *************** 数据库***************

        //bmp,gif,ico,jpe,jpeg,jpg,png
        public static string SupportVideoFormat = $"{Jvedio.Language.Resources.NormalVedio}(*.avi, *.mp4, *.mkv, *.mpg, *.rmvb)| *.avi; *.mp4; *.mkv; *.mpg; *.rmvb|{Jvedio.Language.Resources.OtherVedio}((*.rm, *.mov, *.mpeg, *.flv, *.wmv, *.m4v)| *.rm; *.mov; *.mpeg; *.flv; *.wmv; *.m4v|{Jvedio.Language.Resources.AllFile} (*.*)|*.*";
        public static string SupportPictureFormat = $"图片(*.bmp, *.jpe, *.jpeg, *.jpg, *.png)|*.bmp;*.jpe;*.jpeg;*.jpg;*.png";




        // *************** Mapper ***************



        public static string[] FontExt = new[] { ".otf", ".ttf" };
        public static DataType CurrentDataType = DataType.Video;



        public static Stopwatch stopwatch = new Stopwatch();//计时

        //禁止的文件名符号
        //https://docs.microsoft.com/zh-cn/previous-versions/s6feh8zw(v=vs.110)?redirectedfrom=MSDN
        public static readonly char[] BANFILECHAR = { '\\', '#', '%', '&', '*', '|', ':', '"', '<', '>', '?', '/', '.' };


        public static Servers JvedioServers;
        public static Dictionary<string, string> UrlCookies;// key 网址 value 对应的 cookie


        public static List<string> Censored = new List<string>();
        public static List<string> Uncensored = new List<string>();


        public static Dictionary<string, string> Jav321IDDict = new Dictionary<string, string>();

        // 标签戳，全局缓存，避免每次都查询
        public static List<TagStamp> TagStamps = new List<TagStamp>();

        //按类别中分类
        public static string[] GenreEurope = new string[8];
        public static string[] GenreCensored = new string[7];
        public static string[] GenreUncensored = new string[8];

        //演员分隔符
        public static Dictionary<int, char[]> actorSplitDict = new Dictionary<int, char[]>();//key 分别是 123 骑兵步兵欧美

        //如果包含以下文本，则显示对应的标签戳
        public static string[] TagStrings_HD = new string[] { "hd", "高清" };
        public static string[] TagStrings_Translated = new string[] { "中文", "日本語", "Translated", "English" };
        public static string[] TagStrings_FlowOut = new string[] { "流出", "FlowOut" };

        //最近播放
        public static Dictionary<DateTime, List<string>> RecentWatched = new Dictionary<DateTime, List<string>>();

        //默认图片
        public static BitmapSource BackgroundImage;
        public static BitmapImage DefaultSmallImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_S.png", UriKind.Relative));
        public static BitmapImage DefaultBigImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_B.png", UriKind.Relative));
        public static BitmapImage DefaultActorImage = new BitmapImage(new Uri("/Resources/Picture/NoPrinting_A.png", UriKind.Relative));



        public static TimeSpan FadeInterval = TimeSpan.FromMilliseconds(150);//淡入淡出时间

        //AES加密秘钥
        public static string[] EncryptKeys = new string[] { "ShS69pNGvLac6ZF+", "Yv4x4beWwe+vhFwg", "+C+bPEbF5W4v3/H0" };

        public static string[] NeedCookie = new[] { "DB", "DMM", "MOO" };





        public static FontFamily GlobalFont = null;



        #region "热键"
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public const int HOTKEY_ID = 2415;
        public static uint VK;
        public static IntPtr _windowHandle;
        public static HwndSource _source;
        public static bool IsHide = false;
        public static List<string> OpeningWindows = new List<string>();
        public static List<Key> funcKeys = new List<Key>(); //功能键 [1,3] 个
        public static Key key = Key.None;//基础键 1 个
        public static List<Key> _funcKeys = new List<Key>();
        public static Key _key = Key.None;

        public enum Modifiers
        {
            None = 0x0000,
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            Win = 0x0008
        }

        public static bool IsProperFuncKey(List<Key> keyList)
        {
            bool result = true;
            List<Key> keys = new List<Key>() { Key.LeftCtrl, Key.LeftAlt, Key.LeftShift };

            foreach (Key item in keyList)
            {
                if (!keys.Contains(item))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        #endregion


        public static void LoadBgImage()
        {
            //设置背景
            GlobalVariable.BackgroundImage = null;
            if (Properties.Settings.Default.EnableBgImage)
            {
                string path = Properties.Settings.Default.BackgroundImage;
                if (!File.Exists(path)) path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "background.jpg");
                GC.Collect();

                if (File.Exists(path))
                    GlobalVariable.BackgroundImage = ImageProcess.BitmapImageFromFile(path);

            }
        }

        public static void InitDataBase()
        {
            Enum.TryParse(Properties.Settings.Default.DataBaseType, true, out DataBaseType dataBaseType);
            CurrentDataBaseType = dataBaseType;
        }

        public static void InitVariable()
        {




            //每页数目
            //Properties.Settings.Default.DisplayNumber = 100;
            //Properties.Settings.Default.FlowNum = 20;
            //Properties.Settings.Default.ActorDisplayNum = 30;
            Properties.Settings.Default.VideoType = "0";
            Properties.Settings.Default.ShowViewMode = "0";
            Properties.Settings.Default.OnlyShowPlay = false;
            Properties.Settings.Default.OnlyShowSubSection = false;

            //添加演员分隔符
            if (!actorSplitDict.ContainsKey(0)) actorSplitDict.Add(0, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(1)) actorSplitDict.Add(1, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(2)) actorSplitDict.Add(2, new char[] { ' ', '/' });
            if (!actorSplitDict.ContainsKey(3)) actorSplitDict.Add(3, new char[] { '/' });//欧美






            GenreEurope[0] = Resource_String.GenreEurope.Split('|')[0];
            GenreEurope[1] = Resource_String.GenreEurope.Split('|')[1];
            GenreEurope[2] = Resource_String.GenreEurope.Split('|')[2];
            GenreEurope[3] = Resource_String.GenreEurope.Split('|')[3];
            GenreEurope[4] = Resource_String.GenreEurope.Split('|')[4];
            GenreEurope[5] = Resource_String.GenreEurope.Split('|')[5];
            GenreEurope[6] = Resource_String.GenreEurope.Split('|')[6];
            GenreEurope[7] = Resource_String.GenreEurope.Split('|')[7];

            GenreCensored[0] = Resource_String.GenreCensored.Split('|')[0];
            GenreCensored[1] = Resource_String.GenreCensored.Split('|')[1];
            GenreCensored[2] = Resource_String.GenreCensored.Split('|')[2];
            GenreCensored[3] = Resource_String.GenreCensored.Split('|')[3];
            GenreCensored[4] = Resource_String.GenreCensored.Split('|')[4];
            GenreCensored[5] = Resource_String.GenreCensored.Split('|')[5];
            GenreCensored[6] = Resource_String.GenreCensored.Split('|')[6];

            GenreUncensored[0] = Resource_String.GenreUncensored.Split('|')[0];
            GenreUncensored[1] = Resource_String.GenreUncensored.Split('|')[1];
            GenreUncensored[2] = Resource_String.GenreUncensored.Split('|')[2];
            GenreUncensored[3] = Resource_String.GenreUncensored.Split('|')[3];
            GenreUncensored[4] = Resource_String.GenreUncensored.Split('|')[4];
            GenreUncensored[5] = Resource_String.GenreUncensored.Split('|')[5];
            GenreUncensored[6] = Resource_String.GenreUncensored.Split('|')[6];
            GenreUncensored[7] = Resource_String.GenreUncensored.Split('|')[7];



        }







    }
}
