using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Net
{
    public class DownloadDispatcher
    {

        // 优先级
        private static int MAX_PRIORITY = 5;
        private static int NORMAL_PRIORITY = 3;
        private static int MIN_PRIORITY = 1;

        private static int MAX_TASK_COUNT = 3;// 每次同时下载的任务数量
        private static int TASK_DELAY = 3000;// 每一批次下载后暂停的时间
        private static int CHECK_PERIOD = 1000;  // 调度器运行周期


        public bool Working = false;// 调度器是否在工作中
        public bool Cancel = false;// 调度器是否被取消了

        public double Progress { get; set; }// 总的工作进度

        public event EventHandler onWorking;

        // 具有优先级的队列
        public static SimplePriorityQueue<DownLoadTask> WaitingQueue = new SimplePriorityQueue<DownLoadTask>();
        public static List<DownLoadTask> WorkingList = new List<DownLoadTask>();
        public static List<DownLoadTask> DoneList = new List<DownLoadTask>();

        private static DownloadDispatcher instance = null;

        private DownloadDispatcher() { }


        public static DownloadDispatcher createInstance()
        {
            if (instance == null) instance = new DownloadDispatcher();
            return instance;
        }


        public void Enqueue(DownLoadTask task)
        {
            if (!WaitingQueue.Contains(task))
                WaitingQueue.Enqueue(task, NORMAL_PRIORITY);
        }

        public void CancelWork()
        {
            Cancel = true;
            Working = false;
            foreach (DownLoadTask task in WorkingList)
            {
                task.Cancel();
            }
        }


        public void ClearDoneList()
        {
            DoneList.Clear();
        }


        public void BeginWork()
        {
            Working = true;
            Task.Run(async () =>
            {
                while (true && !Cancel)
                {
                    Console.WriteLine("调度器工作中...");
                    // 检查工作队列中的任务是否完成
                    for (int i = WorkingList.Count - 1; i >= 0; i--)
                    {
                        if (Cancel) return;
                        DownLoadTask task = WorkingList[i];
                        if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Canceled)
                        {
                            DoneList.Add(task);
                            WorkingList.RemoveAt(i);
                        }
                    }
                    if (WorkingList.Count != 0) await Task.Delay(TASK_DELAY);
                    // 将等待队列中的下载任务添加到工作队列
                    while (WorkingList.Count < MAX_TASK_COUNT && WaitingQueue.Count > 0)
                    {
                        DownLoadTask task = WaitingQueue.Dequeue();
                        if (task.Status == TaskStatus.WaitingToRun)
                            WorkingList.Add(task);
                        else
                            DoneList.Add(task);
                    }

                    foreach (DownLoadTask task in WorkingList)
                    {
                        if (!task.Running && task.Status != TaskStatus.Canceled)
                        {
                            Console.WriteLine($" DataID ={ task.DataID} 将开始运行");
                            task.Start();
                        }
                    }

                    float totalcount = DoneList.Count + WaitingQueue.Count + WorkingList.Count;
                    this.Progress = Math.Round((float)DoneList.Count / totalcount * 100, 2);
                    onWorking?.Invoke(this, null);
                    await Task.Delay(CHECK_PERIOD);

                    if (WorkingList.Count == 0 && WaitingQueue.Count == 0)
                    {
                        Working = false;
                        break;
                    }
                }
            });
        }











    }
}
