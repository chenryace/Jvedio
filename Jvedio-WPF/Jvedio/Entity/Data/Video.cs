using Jvedio.Core.Attributes;
using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Core.SimpleORM;
using Jvedio.Entity.CommonSQL;
using Jvedio.Utils;
using Jvedio.Utils.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Jvedio.Entity
{

    [Table(tableName: "metadata_video")]
    public class Video : MetaData
    {


        public Video() : this(true) { }

        public Video(bool _initDefaultImage = true)
        {
            if (_initDefaultImage)
                initDefaultImage();
        }

        // 延迟加载图片
        public void initDefaultImage()
        {
            SmallImage = GlobalVariable.DefaultSmallImage;
            BigImage = GlobalVariable.DefaultBigImage;
            GifUri = new Uri("pack://application:,,,/Resources/Picture/NoPrinting_G.gif");
            PreviewImageList = new ObservableCollection<BitmapSource>();
        }

        public static SelectWrapper<Video> initWrapper()
        {
            SelectWrapper<Video> wrapper = new SelectWrapper<Video>();
            wrapper.Eq("metadata.DBId", GlobalConfig.Main.CurrentDBId)
                .Eq("metadata.DataType", 0);
            return wrapper;
        }

        public static void setTagStamps(ref Video video)
        {
            if (!string.IsNullOrEmpty(video.TagIDs))
            {
                List<long> list = video.TagIDs.Split(',').Select(arg => long.Parse(arg)).ToList();
                if (list != null && list.Count > 0)
                {
                    video.TagStamp = new ObservableCollection<TagStamp>();
                    foreach (var item in GlobalVariable.TagStamps.Where(arg => list.Contains(arg.TagID)).ToList())
                    {
                        video.TagStamp.Add(item);
                    }
                }


            }
        }

        public static void handleEmpty(ref Video video)
        {
            if (Properties.Settings.Default.ShowFileNameIfTitleEmpty
                && !string.IsNullOrEmpty(video.Path) && string.IsNullOrEmpty(video.Title))
                video.Title = System.IO.Path.GetFileNameWithoutExtension(video.Path);
            if (Properties.Settings.Default.ShowCreateDateIfReleaseDateEmpty
                && !string.IsNullOrEmpty(video.LastScanDate) && string.IsNullOrEmpty(video.ReleaseDate))
                video.ReleaseDate = DateHelper.toLocalDate(video.LastScanDate);



        }


        [TableId(IdType.AUTO)]
        public long MVID { get; set; }
        public long DataID { get; set; }
        public string VID { get; set; }

        public string Series { get; set; }
        private VideoType _VideoType;

        public VideoType VideoType
        {
            get { return _VideoType; }
            set
            {
                _VideoType = value;
                OnPropertyChanged();
            }
        }
        public string Director { get; set; }
        public string Studio { get; set; }
        public string Publisher { get; set; }
        public string Plot { get; set; }
        public string Outline { get; set; }
        public int Duration { get; set; }

        [TableField(exist: false)]
        public List<string> SubSectionList { get; set; }

        private string _SubSection;
        public string SubSection
        {
            get { return _SubSection; }
            set
            {
                _SubSection = value;
                SubSectionList = value.Split(new char[] { GlobalVariable.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (SubSectionList.Count >= 2) HasSubSection = true;
                OnPropertyChanged();
            }
        }
        [TableField(exist: false)]
        public bool HasSubSection { get; set; }


        [TableField(exist: false)]
        public ObservableCollection<string> PreviewImagePathList { get; set; }

        [TableField(exist: false)]
        public ObservableCollection<BitmapSource> PreviewImageList { get; set; }
        public string ImageUrls { get; set; }

        public string WebType { get; set; }
        public string WebUrl { get; set; }
        public string ExtraInfo { get; set; }


        private BitmapSource _smallimage;

        [TableField(exist: false)]
        public BitmapSource SmallImage { get { return _smallimage; } set { _smallimage = value; OnPropertyChanged(); } }

        private BitmapSource _bigimage;
        [TableField(exist: false)]
        public BitmapSource BigImage { get { return _bigimage; } set { _bigimage = value; OnPropertyChanged(); } }
        private Uri _GifUri;

        [TableField(exist: false)]
        public Uri GifUri { get { return _GifUri; } set { _GifUri = value; OnPropertyChanged(); } }


        [TableField(exist: false)]
        public ObservableCollection<TagStamp> TagStamp { get; set; }
        [TableField(exist: false)]
        public string TagIDs { get; set; }



        private string _ActorNames;

        [TableField(exist: false)]
        public string ActorNames
        {
            get { return _ActorNames; }
            set
            {
                _ActorNames = value;
                if (!string.IsNullOrEmpty(value))
                {
                    ActorNameList = value.Split(new char[] { GlobalVariable.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                OnPropertyChanged();
            }
        }

        [TableField(exist: false)]
        public List<string> ActorNameList { get; set; }

        /// <summary>
        /// 旧数据库的 actorID 列表
        /// </summary>
        [TableField(exist: false)]
        public string OldActorIDs { get; set; }


        private List<ActorInfo> _ActorInfos;

        [TableField(exist: false)]
        public List<ActorInfo> ActorInfos
        {
            get { return _ActorInfos; }
            set
            {
                _ActorInfos = value;
                if (value != null)
                {

                    ActorNames = string.Join(GlobalVariable.Separator.ToString(),
                        value.Select(arg => arg.ActorName).ToList());
                }
                OnPropertyChanged();
            }
        }
        [TableField(exist: false)]
        public List<Magnet> Magnets { get; set; }



        public bool toDownload()
        {
            return string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(WebUrl) || string.IsNullOrEmpty(ImageUrls);
        }

        public string getServerInfoType()
        {
            if (!string.IsNullOrEmpty(VID))
            {
                if (VID.IndexOf("-") > 0)
                {
                    string s = VID.Split('-')[0];
                    if (s.Equals("FC2"))
                    {
                        return "FC";
                    }
                    else
                    {
                        return VideoType.ToString();
                    }
                }
            }
            return null;
        }


        public string getImagePath(ImageType imageType, string ext = null)
        {
            string result = "";
            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            string basePicPath = GlobalConfig.Settings.PicPaths[pathType.ToString()].ToString();
            if (pathType != PathType.RelativeToData)
            {
                if (pathType == PathType.RelativeToApp)
                    basePicPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePicPath);
                string saveDir = "";
                if (imageType == ImageType.Big)
                    saveDir = System.IO.Path.Combine(basePicPath, "BigPic");
                else if (imageType == ImageType.Small)
                    saveDir = System.IO.Path.Combine(basePicPath, "SmallPic");
                else if (imageType == ImageType.Preview)
                    saveDir = System.IO.Path.Combine(basePicPath, "ExtraPic");
                else if (imageType == ImageType.ScreenShot)
                    saveDir = System.IO.Path.Combine(basePicPath, "ScreenShot");
                else if (imageType == ImageType.Gif)
                    saveDir = System.IO.Path.Combine(basePicPath, "Gif");
                if (!Directory.Exists(saveDir)) FileHelper.TryCreateDir(saveDir);
                if (!string.IsNullOrEmpty(VID))
                    result = System.IO.Path.Combine(saveDir, $"{VID}{(string.IsNullOrEmpty(ext) ? "" : ext)}");
                else
                    result = System.IO.Path.Combine(saveDir, $"{Hash}{(string.IsNullOrEmpty(ext) ? "" : ext)}");
            }
            return result;
        }






        public override string ToString()
        {
            return ClassUtils.toString(this);
        }

        public MetaData toMetaData()
        {
            MetaData metaData = (MetaData)this;
            metaData.DataID = this.DataID;
            return metaData;
        }


        public Dictionary<string, string> toDictionary()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("VideoType", VideoType.ToString());
            dict.Add("DataID", DataID.ToString());
            dict.Add("VID", VID);
            return dict;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Video video = obj as Video;
            return video != null && (video.DataID == this.DataID || video.MVID == this.MVID);
        }

        private static string parseRelativeImageFileName(string path)
        {

            string dirName = System.IO.Path.GetDirectoryName(path);
            string regex = System.IO.Path.GetFileName(path);
            List<string> list = FileHelper.TryGetAllFiles(dirName, "*.*").ToList();
            list = list.Where(arg => ScanTask.PICTURE_EXTENSIONS_LIST.Contains(System.IO.Path.GetExtension(arg).ToLower())).ToList();
            if (list.Count == 0) return null;
            foreach (string item in list)
            {
                Match match = Regex.Match(System.IO.Path.GetFileNameWithoutExtension(item), regex);
                if (match.Success) return item;
            }
            return null;
        }

        private static string parseRelativePath(string path)
        {
            string dirName = System.IO.Path.GetDirectoryName(path);
            List<string> list = DirHelper.GetDirList(dirName).ToList();
            string regex = System.IO.Path.GetFileName(path);
            if (list.Count == 0) return null;
            foreach (string item in list)
            {
                Match match = Regex.Match(System.IO.Path.GetFileNameWithoutExtension(item), regex);
                if (match.Success) return item;
            }
            return null;
        }

        public string getSmallImage()
        {
            string smallImagePath = getImagePath(ImageType.Small, ".jpg");
            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path))
            {

                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)GlobalConfig.Settings.PicPaths[pathType.ToString()];
                string smallPath = System.IO.Path.Combine(basePicPath, dict["SmallImagePath"]);
                smallImagePath = parseRelativeImageFileName(smallPath);

            }
            return smallImagePath;
        }

        public string getBigImage()
        {
            string bigImagePath = getImagePath(ImageType.Big, ".jpg");

            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path))
            {

                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)GlobalConfig.Settings.PicPaths[pathType.ToString()];
                string bigPath = System.IO.Path.Combine(basePicPath, dict["BigImagePath"]);

                bigImagePath = parseRelativeImageFileName(bigPath);

            }
            return bigImagePath;
        }

        public string getExtraImage()
        {
            string imagePath = getImagePath(ImageType.Preview);

            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path))
            {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)GlobalConfig.Settings.PicPaths[pathType.ToString()];
                string path = System.IO.Path.Combine(basePicPath, dict["PreviewImagePath"]);
                imagePath = parseRelativePath(path);
            }
            return imagePath;
        }

        public string getScreenShot()
        {
            string imagePath = getImagePath(ImageType.ScreenShot);

            PathType pathType = (PathType)GlobalConfig.Settings.PicPathMode;
            if (pathType == PathType.RelativeToData && !string.IsNullOrEmpty(Path) && File.Exists(Path))
            {
                string basePicPath = System.IO.Path.GetDirectoryName(Path);
                Dictionary<string, string> dict = (Dictionary<string, string>)GlobalConfig.Settings.PicPaths[pathType.ToString()];
                string path = System.IO.Path.Combine(basePicPath, dict["ScreenShotPath"]);
                imagePath = parseRelativePath(path);
            }
            return imagePath;
        }

        public bool parseDictInfo(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0) return false;
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            foreach (PropertyInfo info in propertyInfos)
            {
                if (dict.ContainsKey(info.Name))
                {
                    object value = dict[info.Name];
                    if (value == null) continue;
                    if (value is List<string> list)
                    {
                        info.SetValue(this, string.Join(GlobalVariable.Separator.ToString(), list));
                    }
                    else if (value is string str)
                    {
                        if (info.PropertyType == typeof(string))
                        {
                            info.SetValue(this, str);
                        }
                        else if (info.PropertyType == typeof(int))
                        {
                            int.TryParse(str, out int val);
                            info.SetValue(this, val);
                        }
                    }

                }
            }
            // 图片地址
            ImageUrls = parseImageUrlFromDict(dict);
            return true;
        }


        private string parseImageUrlFromDict(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0) return "";
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(ImageUrls))
            {
                try { result = JsonConvert.DeserializeObject<Dictionary<string, object>>(ImageUrls); }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }
            if (dict.ContainsKey("SmallImageUrl")) result["SmallImageUrl"] = dict["SmallImageUrl"];
            if (dict.ContainsKey("BigImageUrl")) result["BigImageUrl"] = dict["BigImageUrl"];
            if (dict.ContainsKey("ExtraImageUrl")) result["ExtraImageUrl"] = dict["ExtraImageUrl"];
            if (dict.ContainsKey("ActressImageUrl")) result["ActressImageUrl"] = dict["ActressImageUrl"];
            if (dict.ContainsKey("ActorNames")) result["ActorNames"] = dict["ActorNames"];
            return JsonConvert.SerializeObject(result);
        }


        public void SaveNfo()
        {
            if (!GlobalConfig.Settings.SaveInfoToNFO) return;
            string dir = GlobalConfig.Settings.NFOSavePath;
            bool overrideInfo = GlobalConfig.Settings.OverrideInfo; ;

            string saveName = $"{VID.ToProperFileName()}.nfo";
            if (string.IsNullOrEmpty(VID)) saveName = $"{System.IO.Path.GetFileNameWithoutExtension(Path)}.nfo";

            string saveFileName = "";

            if (Directory.Exists(dir))
                saveFileName = System.IO.Path.Combine(dir, saveName);

            //与视频同路径，视频存在才行
            if (!Directory.Exists(dir) && File.Exists(Path))
                saveFileName = System.IO.Path.Combine(new FileInfo(Path).DirectoryName, saveName);

            if (string.IsNullOrEmpty(saveFileName)) return;
            if (overrideInfo || !File.Exists(saveFileName))
                NFOHelper.SaveToNFO(this, saveFileName);

        }
    }
}
