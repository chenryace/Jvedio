using DynamicData.Annotations;
using Jvedio.Utils;
using Jvedio.Utils.Common;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jvedio.Core.SimpleORM;
using Jvedio.Mapper;
using static Jvedio.GlobalMapper;
using Jvedio.Core.Scan;
using Jvedio.Core.Enums;
using Jvedio.Core.Exceptions;
using Jvedio.CommonNet.Crawler;
using Newtonsoft.Json;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.CustomTask;

namespace Jvedio.Core.Net
{
    public class DownLoadTask : ITask, INotifyPropertyChanged
    {

        public Stopwatch stopwatch { get; set; }
        public bool Success { get; set; }
        public bool Canceld { get; set; }
        public bool DownloadPreview { get; set; }

        protected CancellationTokenSource tokenCTS;
        protected CancellationToken token;

        public event EventHandler onError;
        public event EventHandler onCanceled;
        public event EventHandler onDownloadSuccess;
        public event EventHandler onDownloadPreview;


        private static class Delay
        {
            public static int INFO = 1000;
            public static int EXTRA_IMAGE = 500;
            public static int BIG_IMAGE = 50;
            public static int SMALL_IMAGE = 50;
        }

        protected virtual void OnError(EventArgs e)
        {
            EventHandler error = onError;
            error?.Invoke(this, e);

        }

        public List<string> Logs = new List<string>();
        private TaskLogger logger { get; set; }





        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DownLoadTask(Video video, bool downloadPreview = false, bool overrideInfo = false) : this(video.toMetaData())
        {
            Title = string.IsNullOrEmpty(video.VID) ? video.Title : video.VID;
            DownloadPreview = downloadPreview;
            OverrideInfo = overrideInfo;
        }



        public DownLoadTask(MetaData data)
        {
            DataID = data.DataID;
            DataType = data.DataType;
            Status = System.Threading.Tasks.TaskStatus.WaitingToRun;
            CreateTime = DateHelper.Now();

            tokenCTS = new CancellationTokenSource();
            tokenCTS.Token.Register(() =>
            {
                Console.WriteLine("取消任务");
                onCanceled?.Invoke(this, null);
            });
            token = tokenCTS.Token;

            stopwatch = new Stopwatch();
            logger = new TaskLogger(Logs);

        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is DownLoadTask other)
                return other.DataID.Equals(DataID);
            return false;

        }

        public override int GetHashCode()
        {
            return DataID.GetHashCode();
        }


        public long DataID { get; set; }
        public DataType DataType { get; set; }
        public string Title { get; set; }
        public bool OverrideInfo { get; set; }//强制下载覆盖信息

        #region "property"


        public TaskStatus _Status;
        public TaskStatus Status
        {

            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                if (STATUS_TO_TEXT_DICT.ContainsKey(value))
                    StatusText = STATUS_TO_TEXT_DICT[value];
                OnPropertyChanged();
            }
        }
        public string _Message;
        public string Message
        {

            get
            {
                return _Message;
            }
            set
            {
                _Message = value;
                OnPropertyChanged();
            }
        }



        public static Dictionary<TaskStatus, string> STATUS_TO_TEXT_DICT = new Dictionary<TaskStatus, string>()
        {

            {TaskStatus.WaitingToRun,"等待中..."},
            {TaskStatus.Running,"下载中..."},
            {TaskStatus.Canceled,"已取消"},
            {TaskStatus.RanToCompletion,"已完成"},
        };

        public string _StatusText;
        public string StatusText
        {

            get
            {
                return _StatusText;
            }
            set
            {
                _StatusText = value;
                logger?.Info(value);
                OnPropertyChanged();
            }
        }


        public long _ElapsedMilliseconds;
        public long ElapsedMilliseconds
        {

            get
            {
                return _ElapsedMilliseconds;
            }
            set
            {
                _ElapsedMilliseconds = value;
                OnPropertyChanged();
            }
        }
        public float _Progress;
        public float Progress
        {

            get
            {
                return _Progress;
            }
            set
            {
                _Progress = value;
                OnPropertyChanged();
            }
        }




        /// <summary>
        /// 用于指示下载任务是否进行中
        /// </summary>
        public bool _Running;
        public bool Running
        {

            get
            {
                return _Running;
            }
            set
            {
                _Running = value;
                OnPropertyChanged();
            }
        }


        public string _CreateTime;
        public string CreateTime
        {

            get
            {
                return _CreateTime;
            }
            set
            {
                _CreateTime = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public void Start()
        {
            Status = TaskStatus.Running;
            CreateTime = DateHelper.Now();
            Running = true;
            doWrok();
        }

        public virtual void doWrok()
        {
            Task.Run(async () =>
            {
                Progress = 0;
                stopwatch.Start();
                Dictionary<string, object> dict = null;
                if (DataType == DataType.Video)
                {
                    Video video = videoMapper.SelectVideoByID(DataID);
                    VideoDownLoader downLoader = new VideoDownLoader(video, token);
                    // 判断是否需要下载，自动跳过已下载的信息
                    if (video.toDownload() || OverrideInfo)
                    {
                        StatusText = "下载信息";
                        if (!string.IsNullOrEmpty(video.VID))
                        {
                            // 有 VID 的
                            try
                            {
                                dict = await downLoader.GetInfo();
                            }
                            catch (CrawlerNotFoundException ex)
                            {
                                // todo 显示到界面上
                                Message = ex.Message;
                            }

                        }
                        else
                        {
                            // 无 VID 的




                        }
                        // 等待了很久都没成功

                        await Task.Delay(Delay.INFO);
                    }
                    else
                    {
                        logger.Info("跳过信息刮削，准备下载图片");
                    }


                    bool success = true;// 是否刮削到信息（包括db的部分信息）
                    Progress = 33f;
                    if ((dict != null && dict.ContainsKey("Error")))
                    {
                        string error = dict["Error"].ToString();
                        if (!string.IsNullOrEmpty(error))
                        {
                            Message = error;
                            logger.Error(error);
                        }
                        success = dict.ContainsKey("Title") && !string.IsNullOrEmpty(dict["Title"].ToString());
                    }
                    if (!success)
                    {
                        dict = null;
                        // 发生了错误，停止下载
                        finalizeWithCancel();
                        return;
                    }

                    bool downloadInfo = video.parseDictInfo(dict);// 是否从网络上刮削了信息
                    if (downloadInfo)
                    {
                        logger.Info($"保存入库");
                        videoMapper.updateById(video);
                        metaDataMapper.updateById(video.toMetaData());
                        // 保存 dataCode
                        if (dict.ContainsKey("DataCode") && dict.ContainsKey("WebType"))
                        {
                            UrlCode urlCode = new UrlCode();
                            urlCode.LocalValue = video.VID;
                            urlCode.RemoteValue = dict["DataCode"].ToString();
                            urlCode.ValueType = "video";
                            urlCode.WebType = dict["WebType"].ToString();
                            urlCodeMapper.insert(urlCode, InsertMode.Replace);
                        }
                        onDownloadSuccess?.Invoke(this, null);
                    }
                    else
                    {
                        dict = null;
                    }

                    if (Canceld)
                    {
                        finalizeWithCancel();
                        return;
                    }




                    // 可能刮削了信息，但是没刮削图片
                    object o = getInfoFromExist("BigImageUrl", video, dict);
                    string imageUrl = o != null ? o.ToString() : "";
                    StatusText = "下载大图";
                    // 1. 大图
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        // todo 原来的 domain 可能没法用，得替换 domain
                        string saveFileName = video.getImagePath(ImageType.Big, Path.GetExtension(imageUrl));
                        if (!File.Exists(saveFileName))
                        {
                            byte[] fileByte = await downLoader.DownloadImage(imageUrl, (error) =>
                            {
                                if (!string.IsNullOrEmpty(error))
                                    logger.Error($"{imageUrl} => {error}");
                            });
                            if (fileByte != null && fileByte.Length > 0)
                                FileProcess.ByteArrayToFile(fileByte, saveFileName);
                            await Task.Delay(Delay.BIG_IMAGE);
                        }
                        else
                        {
                            logger.Info($"跳过已下载的图片：{saveFileName}");
                        }
                    }

                    Progress = 66f;


                    if (Canceld)
                    {
                        finalizeWithCancel();
                        return;
                    }


                    StatusText = "下载小图";
                    o = getInfoFromExist("SmallImageUrl", video, dict);
                    imageUrl = o != null ? o.ToString() : "";
                    // 2. 小图
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        string saveFileName = video.getImagePath(ImageType.Small, Path.GetExtension(imageUrl));
                        if (!File.Exists(saveFileName))
                        {
                            byte[] fileByte = await downLoader.DownloadImage(imageUrl, (error) =>
                            {
                                if (!string.IsNullOrEmpty(error))
                                    logger.Error($"{imageUrl} => {error}");
                            });
                            if (fileByte != null && fileByte.Length > 0)
                                FileProcess.ByteArrayToFile(fileByte, saveFileName);
                            await Task.Delay(Delay.SMALL_IMAGE);
                        }
                        else
                        {
                            logger.Info($"跳过已下载的图片：{saveFileName}");
                        }
                    }

                    Progress = 77f;
                    if (Canceld)
                    {
                        finalizeWithCancel();
                        return;
                    }

                    onDownloadSuccess?.Invoke(this, null);
                    StatusText = "下载演员信息和头像";

                    object names = getInfoFromExist("ActorNames", video, dict);
                    object urls = getInfoFromExist("ActressImageUrl", video, dict);
                    // 3. 演员信息和头像
                    if (names != null && urls != null && names is List<string> ActorNames && urls is List<string> ActressImageUrl)
                    {
                        if (ActorNames != null && ActressImageUrl != null && ActorNames.Count == ActressImageUrl.Count)
                        {
                            for (int i = 0; i < ActorNames.Count; i++)
                            {
                                string actorName = ActorNames[i];
                                string url = ActressImageUrl[i];
                                ActorInfo actorInfo = actorMapper.selectOne(new SelectWrapper<ActorInfo>().Eq("ActorName", actorName));
                                if (actorInfo == null || actorInfo.ActorID <= 0)
                                {
                                    actorInfo = new ActorInfo();
                                    actorInfo.ActorName = actorName;
                                    actorMapper.insert(actorInfo);
                                }
                                // 保存信息
                                string sql = $"insert or ignore into metadata_to_actor (ActorID,DataID) values ({actorInfo.ActorID},{video.DataID})";
                                metaDataMapper.executeNonQuery(sql);
                                // 下载图片
                                string saveFileName = actorInfo.getImagePath();
                                if (!File.Exists(saveFileName))
                                {
                                    byte[] fileByte = await downLoader.DownloadImage(url, (error) =>
                                    {
                                        if (!string.IsNullOrEmpty(error))
                                            logger.Error($"{url} => {error}");

                                    });
                                    if (fileByte != null && fileByte.Length > 0)
                                        FileProcess.ByteArrayToFile(fileByte, saveFileName);

                                }
                                else
                                {
                                    logger.Info($"跳过已下载的图片：{saveFileName}");
                                }
                            }
                        }


                    }
                    Progress = 88f;
                    if (Canceld)
                    {
                        finalizeWithCancel();
                        return;
                    }



                    // 4. 下载预览图

                    urls = getInfoFromExist("ExtraImageUrl", video, dict);
                    if (DownloadPreview && urls != null && urls is List<string> imageUrls)
                    {
                        StatusText = "下载预览图";
                        if (imageUrls != null && imageUrls.Count > 0)
                        {
                            for (int i = 0; i < imageUrls.Count; i++)
                            {
                                if (Canceld)
                                {
                                    finalizeWithCancel();
                                    return;
                                }

                                string url = imageUrls[i];

                                // 下载图片
                                string saveFiledir = video.getExtraImage();
                                if (!Directory.Exists(saveFiledir)) Directory.CreateDirectory(saveFiledir);
                                string saveFileName = Path.Combine(saveFiledir, Path.GetFileName(url));
                                if (!File.Exists(saveFileName))
                                {
                                    StatusText = $"下载预览图 {(i + 1)}/{imageUrls.Count}";
                                    byte[] fileByte = await downLoader.DownloadImage(url, (error) =>
                                    {
                                        if (!string.IsNullOrEmpty(error))
                                            logger.Error($"{url} => {error}");

                                    });
                                    if (fileByte != null && fileByte.Length > 0)
                                    {
                                        FileProcess.ByteArrayToFile(fileByte, saveFileName);
                                        PreviewImageEventArgs arg = new PreviewImageEventArgs(saveFileName, fileByte);
                                        onDownloadPreview?.Invoke(this, arg);
                                    }

                                    await Task.Delay(Delay.EXTRA_IMAGE);
                                }
                                else
                                {
                                    logger.Info($"跳过已下载的图片：{saveFileName}");
                                }
                            }
                        }
                    }
                    else
                    if (!DownloadPreview)
                        logger.Info($"未开启预览图下载");
                    Success = true;
                    Status = TaskStatus.RanToCompletion;
                }
                Console.WriteLine("下载完成！");
                Progress = 100.00f;
                stopwatch.Stop();
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                logger.Info($"总计耗时：{ElapsedMilliseconds} ms");
            });
        }



        private void finalizeWithCancel()
        {
            Status = TaskStatus.Canceled;// 抛出异常的任务都自动取消
            stopwatch.Stop();
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            Success = false;
            logger.Info($"总计耗时：{ElapsedMilliseconds} ms");
        }


        private object getInfoFromExist(string type, Video video, Dictionary<string, object> dict)
        {
            if (dict != null && dict.Count > 0)
            {
                if (dict.ContainsKey(type))
                {
                    if (dict[type].GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                    {
                        Newtonsoft.Json.Linq.JArray jArray = Newtonsoft.Json.Linq.JArray.Parse(dict[type].ToString());
                        return jArray.Select(x => x.ToString()).ToList();
                    }
                    return dict[type];
                }
                return null;
            }
            else if (video != null)
            {
                string imageUrls = video.ImageUrls;
                if (!string.IsNullOrEmpty(imageUrls))
                {
                    Dictionary<string, object> dic = null;
                    try { dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(imageUrls); }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                    if (dic == null) return null;
                    return getInfoFromExist(type, null, dic);// 递归调用
                }
            }
            return null;
        }


        public void CheckStatus()
        {
            if (Status == TaskStatus.Canceled)
            {
                stopwatch.Stop();
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                throw new TaskCanceledException();
            }

        }


        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Cancel()
        {
            if (Status == TaskStatus.Running || Status == TaskStatus.WaitingToRun)
            {
                Status = TaskStatus.Canceled;
                Running = false;
                tokenCTS.Cancel();
                Canceld = true;
                logger.Info("已取消");
            }
        }

        public void Finished()
        {
            throw new NotImplementedException();
        }
    }
}
