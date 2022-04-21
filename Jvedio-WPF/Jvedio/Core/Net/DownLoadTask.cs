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

namespace Jvedio.Core.Net
{
    public class DownLoadTask : ITask, INotifyPropertyChanged
    {

        public Stopwatch stopwatch { get; set; }

        protected CancellationTokenSource tokenCTS;
        protected CancellationToken token;

        public event EventHandler onError;
        public event EventHandler onCanceled;

        protected virtual void OnError(EventArgs e)
        {
            EventHandler error = onError;
            error?.Invoke(this, e);

        }




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
        public string Title { get; set; }

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

                await Task.Delay(1000);
                try { CheckStatus(); }
                catch (TaskCanceledException ex) { Console.WriteLine(ex.Message); return; }
                Progress = 33.33f;

                await Task.Delay(1000);
                try { CheckStatus(); }
                catch (TaskCanceledException ex) { Console.WriteLine(ex.Message); return; }
                Progress = 66.66f;

                await Task.Delay(1000);

                Console.WriteLine("下载完成！");
                Progress = 100.00f;
                stopwatch.Stop();
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                Status = TaskStatus.RanToCompletion;
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
            if (Status == TaskStatus.Running)
            {
                Status = TaskStatus.Canceled;
                tokenCTS.Cancel();
            }
        }



    }
}
