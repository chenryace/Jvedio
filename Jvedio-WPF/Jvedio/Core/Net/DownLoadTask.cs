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

namespace Jvedio.Core.Net
{
    public class DownLoadTask : ITask, INotifyPropertyChanged
    {

        public Stopwatch stopwatch { get; set; }
        public bool Success { get; set; }

        protected CancellationTokenSource tokenCTS;
        protected CancellationToken token;

        public event EventHandler onError;
        public event EventHandler onCanceled;
        public event EventHandler onDownloadSuccess;

        protected virtual void OnError(EventArgs e)
        {
            EventHandler error = onError;
            error?.Invoke(this, e);

        }

        public Dictionary<string, string> Error = new Dictionary<string, string>();



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DownLoadTask(Video video) : this(video.toMetaData())
        {
            Title = string.IsNullOrEmpty(video.VID) ? video.Title : video.VID;
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
                Running = value == TaskStatus.Running;
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
                Console.WriteLine("开始下载！");
                stopwatch.Start();
                Dictionary<string, object> dict = null;
                if (DataType == DataType.Video)
                {
                    Video video = videoMapper.SelectVideoByID(DataID);

                    // 判断是否需要下载，自动跳过已下载的信息
                    if (video.toDownload() || OverrideInfo)
                    {
                        VideoDownLoader downLoader = new VideoDownLoader(video, token);
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
                        if (dict != null && dict.Count > 0)
                        {
                            Progress = 33f;
                            if (dict.ContainsKey("Error"))
                            {
                                string error = dict["Error"].ToString();
                                if (!string.IsNullOrEmpty(error))
                                {
                                    Message = error;
                                    dict = null;
                                }
                            }
                            else
                            {
                                // 1. 大图
                                if (dict.ContainsKey("BigImageUrl"))
                                {
                                    string imageUrl = dict["BigImageUrl"].ToString();
                                    string saveFileName = video.getImagePath(ImageType.Big, Path.GetExtension(imageUrl));
                                    if (!File.Exists(saveFileName))
                                    {
                                        byte[] fileByte = downLoader.DownloadImage(imageUrl, (error) =>
                                        {
                                            if (!string.IsNullOrEmpty(error))
                                                Error.Add("BigImageUrl",
                                                    $"{DateHelper.Now()} [Error] {imageUrl} => {error}");
                                        });
                                        if (fileByte != null && fileByte.Length > 0)
                                            FileProcess.ByteArrayToFile(fileByte, saveFileName);
                                    }
                                }
                                Progress = 66f;
                                // 2. 小图
                                if (dict.ContainsKey("SmallImageUrl"))
                                {
                                    string imageUrl = dict["SmallImageUrl"].ToString();
                                    string saveFileName = video.getImagePath(ImageType.Small, Path.GetExtension(imageUrl));
                                    if (!File.Exists(saveFileName))
                                    {
                                        byte[] fileByte = downLoader.DownloadImage(imageUrl, (error) =>
                                        {
                                            if (!string.IsNullOrEmpty(error))
                                                Error.Add("SmallImageUrl",
                                                    $"{DateHelper.Now()} [Error] {imageUrl} => {error}");
                                        });
                                        if (fileByte != null && fileByte.Length > 0)
                                            FileProcess.ByteArrayToFile(fileByte, saveFileName);
                                    }
                                }
                                Progress = 77f;
                                // 3. 演员信息和头像
                                if (dict.ContainsKey("ActorNames") && dict.ContainsKey("ActressImageUrl"))
                                {
                                    if (dict["ActorNames"] is List<string> ActorNames && dict["ActressImageUrl"] is List<string> ActressImageUrl)
                                    {
                                        if (ActorNames != null && ActressImageUrl != null && ActorNames.Count == ActressImageUrl.Count)
                                        {
                                            for (int i = 0; i < ActorNames.Count; i++)
                                            {
                                                string actorName = ActorNames[i];
                                                string imageUrl = ActressImageUrl[i];
                                                bool insert = false;
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
                                                    byte[] fileByte = downLoader.DownloadImage(imageUrl, (error) =>
                                                    {
                                                        if (!string.IsNullOrEmpty(error))
                                                            Error.Add("ActressImageUrl",
                                                                $"{DateHelper.Now()} [Error] {imageUrl} => {error}");
                                                    });
                                                    if (fileByte != null && fileByte.Length > 0)
                                                        FileProcess.ByteArrayToFile(fileByte, saveFileName);
                                                }
                                            }
                                        }

                                    }
                                }
                                Progress = 88f;
                                // 2. 更新
                                video.parseDictInfo(dict);
                                videoMapper.updateById(video);
                                metaDataMapper.updateById(video.toMetaData());
                                Success = true;
                            }
                        }
                    }
                    else
                    {
                        Message = "跳过";
                        dict = new Dictionary<string, object>();
                    }


                }

                //try { CheckStatus(); }
                //catch (TaskCanceledException ex) { Console.WriteLine(ex.Message); return; }
                if (dict != null)
                {
                    Status = TaskStatus.RanToCompletion;
                }
                else
                {
                    Status = TaskStatus.Canceled;// 抛出异常的任务都自动取消
                }






                await Task.Delay(1000);

                Console.WriteLine("下载完成！");
                Progress = 100.00f;
                stopwatch.Stop();
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                if (Success) onDownloadSuccess?.Invoke(this, null);
            });
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
                tokenCTS.Cancel();
            }
        }



    }
}
