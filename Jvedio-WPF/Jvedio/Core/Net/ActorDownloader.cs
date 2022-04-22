using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Net
{
    //public class ActorDownloader
    //{
    //    public event EventHandler MessageCallBack;
    //    public event EventHandler InfoUpdate;
    //    public DownLoadProgress downLoadProgress;
    //    private Semaphore Semaphore;
    //    private ProgressBarUpdate ProgressBarUpdate;
    //    private bool Cancel { get; set; }
    //    public DownLoadState State;

    //    public List<Actress> ActorList { get; set; }

    //    public ActorDownloader(List<Actress> actresses)
    //    {
    //        ActorList = actresses;
    //        Cancel = false;
    //        Semaphore = new Semaphore(3, 3);
    //        ProgressBarUpdate = new ProgressBarUpdate() { value = 0, maximum = 1 };
    //    }

    //    public void CancelDownload()
    //    {
    //        Cancel = true;
    //        State = DownLoadState.Fail;
    //    }

    //    public void BeginDownLoad()
    //    {
    //        if (ActorList.Count == 0) { this.State = DownLoadState.Completed; return; }


    //        // todo 先根据 BusActress.sqlite 获得 id
    //        List<Actress> actresslist = new List<Actress>();
    //        foreach (Actress item in ActorList)
    //        {
    //            if (item != null && (item.smallimage == null || string.IsNullOrEmpty(item.birthday)))
    //            {
    //                Actress actress = item;
    //                MySqlite db = new MySqlite("BusActress");
    //                if (string.IsNullOrEmpty(item.id))
    //                    actress.id = db.GetInfoBySql($"select id from censored where name='{item.name}'");
    //                db.CloseDB();
    //                if (string.IsNullOrEmpty(item.imageurl))
    //                {
    //                    //TODO 没法解决演员头像骑兵和步兵的问题
    //                    //默认骑兵吧，反正拍过步兵的肯定也拍过骑兵
    //                    //拼接网址
    //                    actress.imageurl = $"{JvedioServers.Bus.Url}pics/actress/{item.id}_a.jpg";
    //                }

    //                actresslist.Add(actress);
    //            }
    //        }

    //        ProgressBarUpdate.maximum = actresslist.Count;
    //        for (int i = 0; i < actresslist.Count; i++)
    //        {
    //            Console.WriteLine("开始进程 " + i);
    //            Thread threadObject = new Thread(DownLoad);
    //            threadObject.Start(actresslist[i]);
    //        }
    //    }

    //    private async void DownLoad(object o)
    //    {

    //        //    Semaphore.WaitOne();
    //        //    Actress actress = o as Actress;
    //        //    if (Cancel | actress.id == "")
    //        //    {
    //        //        Semaphore.Release();
    //        //        return;
    //        //    }
    //        //    try
    //        //    {
    //        //        this.State = DownLoadState.DownLoading;

    //        //        //下载头像
    //        //        if (!string.IsNullOrEmpty(actress.imageurl))
    //        //        {
    //        //            string url = actress.imageurl;
    //        //            HttpResult streamResult = await HTTP.DownLoadFile(url);
    //        //            if (streamResult != null)
    //        //            {
    //        //                ImageProcess.SaveImage(actress.name, streamResult.FileByte, ImageType.ActorImage, url);
    //        //                actress.smallimage = ImageProcess.GetBitmapImage(actress.name, "Actresses");
    //        //            }

    //        //        }
    //        //        //下载信息
    //        //        bool success = false;
    //        //        success = await Task.Run(() =>
    //        //        {
    //        //            Task.Delay(300).Wait();
    //        //            return HTTP.DownLoadActress(actress.id, actress.name, callback: (message) => { MessageCallBack?.Invoke(this, new MessageCallBackEventArgs(message)); });
    //        //        });

    //        //        if (success) actress = DataBase.SelectInfoByActress(actress);
    //        //        ProgressBarUpdate.value += 1;
    //        //        InfoUpdate?.Invoke(this, new ActressUpdateEventArgs() { Actress = actress, progressBarUpdate = ProgressBarUpdate, state = State });
    //        //    }
    //        //    catch (Exception ex)
    //        //    {
    //        //        Console.WriteLine(ex.Message);
    //        //    }
    //        //    finally
    //        //    {
    //        //        Semaphore.Release();
    //        //    }
    //    }
    //}


}
