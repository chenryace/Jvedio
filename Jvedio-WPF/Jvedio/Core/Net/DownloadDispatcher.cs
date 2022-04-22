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

        public static int MAX_PRIORITY = 5;
        public static int NORMAL_PRIORITY = 3;
        public static int MIN_PRIORITY = 1;

        public static int MAX_TASK_COUNT = 3;
        public static int CHECK_PERIOD = 1000;


        public bool Working = false;
        public bool Cancel = false;

        public double Progress { get; set; }

        public event EventHandler onWorking;


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
                    for (int i = WorkingList.Count - 1; i >= 0; i--)
                    {
                        if (Cancel) return;
                        DownLoadTask task = WorkingList[i];
                        if (task.Status == TaskStatus.RanToCompletion || task.Status == TaskStatus.Canceled)
                        {
                            DoneList.Add(task);
                            WorkingList.RemoveAt(i);
                        }
                        else if (task.Status == TaskStatus.WaitingForActivation)
                        {
                            WaitingQueue.Enqueue(task, NORMAL_PRIORITY);
                            WorkingList.RemoveAt(i);
                        }
                    }

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
                        if (!task.Running) task.Start();
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
