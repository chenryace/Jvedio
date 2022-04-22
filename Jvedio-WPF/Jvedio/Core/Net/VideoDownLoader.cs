using DynamicData.Annotations;
using Jvedio.Utils.Encrypt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;
using Jvedio.Utils;
using Jvedio.Utils.Net;
using Jvedio.Entity;
using Jvedio.Core.Enums;
using Jvedio.Core.Scan;
using Jvedio.Core.Crawler;
using Jvedio.Core.Exceptions;
using Jvedio.Core.Plugins;
using Jvedio.Common.Crawler.Entity;
using Jvedio.Common.Crawler;

namespace Jvedio.Core.Net
{
    public class VideoDownLoader
    {
        public DownLoadState State = DownLoadState.DownLoading;
        public event EventHandler InfoUpdate;
        public event EventHandler MessageCallBack;
        public DownLoadProgress downLoadProgress;
        private bool Canceld { get; set; }
        private CancellationToken cancellationToken { get; set; }

        public Video CurrentVideo { get; set; }
        public string InfoType { get; set; }
        public RequestHeader Header { get; set; }

        public List<CrawlerServer> CrawlerServers { get; set; } //该资源支持的爬虫刮削器
        public VideoDownLoader(Video video, CancellationToken token)
        {
            CurrentVideo = video;
            cancellationToken = token;
            InfoType = CurrentVideo.getServerInfoType().ToLower();
        }


        /// <summary>
        /// 取消下载
        /// </summary>
        public void Cancel()
        {
            Canceld = true;
            State = DownLoadState.Fail;
        }



        public string generateUrl(CrawlerServer server)
        {
            string baseUrl = server.Url;
            string serverName = server.ServerName.ToUpper();
            if (server.ServerName.Equals("BUS"))
            {
                return $"{baseUrl}{CurrentVideo.VID}";
            }
            else if (server.ServerName.Equals("DB"))
            {

            }
            else if (server.ServerName.Equals("FC"))
            {

            }
            else if (server.ServerName.Equals("BUS"))
            {

            }
            else if (server.ServerName.Equals("BUS"))
            {

            }
            return baseUrl;
        }



        public async Task<Dictionary<string, object>> GetInfo()
        {
            //下载信息
            State = DownLoadState.DownLoading;
            Dictionary<string, object> result = new Dictionary<string, object>();
            // 获取信息类型，并设置爬虫类型

            if (string.IsNullOrEmpty(InfoType) || GlobalConfig.ServerConfig.CrawlerServers.Count == 0
                || Global.Plugins.Crawlers.Count == 0
                )
                throw new CrawlerNotFoundException();

            List<PluginInfo> pluginInfos = Global.Plugins.Crawlers.Where(arg => arg.InfoType.Split(',')
                                           .Select(item => item.ToLower()).Contains(InfoType)).ToList();
            if (pluginInfos.Count == 0)
                throw new CrawlerNotFoundException();
            PluginInfo pluginInfo = pluginInfos[0];


            // 一组支持刮削的网址列表
            List<CrawlerServer> crawlers = GlobalConfig.ServerConfig.CrawlerServers
                    .Where(arg => arg.ServerName.ToLower().Equals(pluginInfo.ServerName.ToLower())).ToList();

            if (crawlers == null || crawlers.Count == 0) throw new CrawlerNotFoundException();
            crawlers = crawlers.Where(arg => arg.Enabled && arg.Available == 1 && !string.IsNullOrEmpty(arg.Url)).ToList();
            if (crawlers == null || crawlers.Count == 0) throw new CrawlerNotFoundException();
            crawlers = crawlers.OrderBy(arg => arg.ServerName).ToList();
            CrawlerServer crawler = crawlers[0];
            string url = generateUrl(crawler);
            Header = CrawlerServer.parseHeader(crawler);

            Dictionary<string, string> dataInfo = CurrentVideo.toDictionary();

            Plugin plugin = new Plugin(pluginInfo.Path, "GetInfo", new object[] { url, Header, dataInfo });
            object o = await plugin.asyncInvokeMethod();
            if (o is Dictionary<string, object> d)
            {
                return d;
            }
            return result;
            //HttpResult httpResult = await HTTP.DownLoadFromNet(movie);
            //if (httpResult != null)
            //{
            //    if (httpResult.Success)
            //    {
            //        InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = movie, progress = downLoadProgress.value, Success = httpResult.Success });//委托到主界面显示
            //    }
            //    else
            //    {
            //        string error = httpResult.Error != "" ? httpResult.Error : httpResult.StatusCode.ToStatusMessage();
            //        MessageCallBack?.Invoke(this, new MessageCallBackEventArgs($" {movie.id} {Jvedio.Language.Resources.DownloadMessageFailFor}：{error}"));
            //    }
            //}

            //DetailMovie dm = DataBase.SelectDetailMovieById(movie.id);

            //if (dm == null)
            //{
            //    if (movie.id.ToUpper().StartsWith("FC2"))
            //        SemaphoreFC2.Release();
            //    else
            //        Semaphore.Release();
            //    return;
            //}

            //if (!File.Exists(BasePicPath + $"BigPic\\{dm.id}.jpg") || enforce)
            //{
            //    await HTTP.DownLoadImage(dm.bigimageurl, ImageType.BigImage, dm.id);//下载大图
            //}



            ////fc2 没有缩略图
            //if (dm.id.IndexOf("FC2") >= 0)
            //{
            //    //复制海报图作为缩略图
            //    if (File.Exists(BasePicPath + $"BigPic\\{dm.id}.jpg") && !File.Exists(BasePicPath + $"SmallPic\\{dm.id}.jpg"))
            //    {
            //        FileHelper.TryCopyFile(BasePicPath + $"BigPic\\{dm.id}.jpg", BasePicPath + $"SmallPic\\{dm.id}.jpg");
            //    }

            //}
            //else
            //{
            //    if (!File.Exists(BasePicPath + $"SmallPic\\{dm.id}.jpg") || enforce)
            //    {
            //        await HTTP.DownLoadImage(dm.smallimageurl, ImageType.SmallImage, dm.id); //下载小图
            //    }
            //}
            //dm.smallimage = ImageProcess.GetBitmapImage(dm.id, "SmallPic");
            //InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = dm, progress = downLoadProgress.value, state = State });//委托到主界面显示
            //dm.bigimage = ImageProcess.GetBitmapImage(dm.id, "BigPic");
            //lock (downLoadProgress.lockobject) downLoadProgress.value += 1;//完全下载完一个影片
            //InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = dm, progress = downLoadProgress.value, state = State, Success = true });//委托到主界面显示
            //Task.Delay(Delay.MEDIUM).Wait();//每个线程之间暂停
            //                                //取消阻塞
            //if (movie.id.ToUpper().IndexOf("FC2") >= 0)
            //    SemaphoreFC2.Release();
            //else
            //    Semaphore.Release();

        }


        public byte[] DownloadImage(string url, Action<string> onError = null)
        {
            return HTTP.DownloadSmallFile(url, Header, null, (error) =>
            {
                onError?.Invoke(error);
            });
        }



        private async void DownLoad(object o)
        {

            //下载信息
            //    Movie movie = o as Movie;
            //    if (movie.id.ToUpper().StartsWith("FC2"))
            //        SemaphoreFC2.WaitOne();
            //    else
            //        Semaphore.WaitOne();//阻塞
            //    if (Cancel || string.IsNullOrEmpty(movie.id))
            //    {
            //        if (movie.id.ToUpper().StartsWith("FC2"))
            //            SemaphoreFC2.Release();
            //        else
            //            Semaphore.Release();
            //        return;
            //    }

            //    //下载信息
            //    State = DownLoadState.DownLoading;
            //    if (movie.IsToDownLoadInfo() || enforce)
            //    {
            //        //满足一定条件才下载信息
            //        HttpResult httpResult = await HTTP.DownLoadFromNet(movie);
            //        if (httpResult != null)
            //        {
            //            if (httpResult.Success)
            //            {
            //                InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = movie, progress = downLoadProgress.value, Success = httpResult.Success });//委托到主界面显示
            //            }
            //            else
            //            {
            //                string error = httpResult.Error != "" ? httpResult.Error : httpResult.StatusCode.ToStatusMessage();
            //                MessageCallBack?.Invoke(this, new MessageCallBackEventArgs($" {movie.id} {Jvedio.Language.Resources.DownloadMessageFailFor}：{error}"));
            //            }
            //        }
            //    }
            //    DetailMovie dm = DataBase.SelectDetailMovieById(movie.id);

            //    if (dm == null)
            //    {
            //        if (movie.id.ToUpper().StartsWith("FC2"))
            //            SemaphoreFC2.Release();
            //        else
            //            Semaphore.Release();
            //        return;
            //    }

            //    if (!File.Exists(BasePicPath + $"BigPic\\{dm.id}.jpg") || enforce)
            //    {
            //        await HTTP.DownLoadImage(dm.bigimageurl, ImageType.BigImage, dm.id);//下载大图
            //    }



            //    //fc2 没有缩略图
            //    if (dm.id.IndexOf("FC2") >= 0)
            //    {
            //        //复制海报图作为缩略图
            //        if (File.Exists(BasePicPath + $"BigPic\\{dm.id}.jpg") && !File.Exists(BasePicPath + $"SmallPic\\{dm.id}.jpg"))
            //        {
            //            FileHelper.TryCopyFile(BasePicPath + $"BigPic\\{dm.id}.jpg", BasePicPath + $"SmallPic\\{dm.id}.jpg");
            //        }

            //    }
            //    else
            //    {
            //        if (!File.Exists(BasePicPath + $"SmallPic\\{dm.id}.jpg") || enforce)
            //        {
            //            await HTTP.DownLoadImage(dm.smallimageurl, ImageType.SmallImage, dm.id); //下载小图
            //        }
            //    }
            //    dm.smallimage = ImageProcess.GetBitmapImage(dm.id, "SmallPic");
            //    InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = dm, progress = downLoadProgress.value, state = State });//委托到主界面显示
            //    dm.bigimage = ImageProcess.GetBitmapImage(dm.id, "BigPic");
            //    lock (downLoadProgress.lockobject) downLoadProgress.value += 1;//完全下载完一个影片
            //    InfoUpdate?.Invoke(this, new InfoUpdateEventArgs() { Movie = dm, progress = downLoadProgress.value, state = State, Success = true });//委托到主界面显示
            //    Task.Delay(Delay.MEDIUM).Wait();//每个线程之间暂停
            //    //取消阻塞
            //    if (movie.id.ToUpper().IndexOf("FC2") >= 0)
            //        SemaphoreFC2.Release();
            //    else
            //        Semaphore.Release();
        }


        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }
    }













}
