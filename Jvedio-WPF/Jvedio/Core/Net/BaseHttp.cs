using Jvedio.Utils;
using Jvedio.Utils.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Net
{
    public class BaseHttp : ILog
    {
        public enum HttpMode
        {
            Normal = 0,
            RedirectGet = 1,//重定向到 Location
            Stream = 2//下载文件
        }

        public int AttemptCount { get; set; }
        public HttpTimeOut httpTimeOut { get; set; }
        public class HttpTimeOut
        {
            public HttpTimeOut()
            {
                TCP = 30;   // TCP 超时
                HTTP = 30; // HTTP 超时
                Request = 30000;//网站 HTML 获取超时
                FileRequest = 30000;//图片下载超时
                ReadWrite = 30000;
            }
            public int TCP { get; set; }
            public int HTTP { get; set; }
            public int Request { get; set; }
            public int FileRequest { get; set; }
            public int ReadWrite { get; set; }
        }



        public BaseHttp()
        {
            AttemptCount = 2;// 最大尝试次数
            httpTimeOut = new HttpTimeOut();
        }


        /// <summary>
        /// Http 请求
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="headers"></param>
        /// <param name="Mode"></param>
        /// <param name="Proxy"></param>
        /// <param name="allowRedirect"></param>
        /// <param name="poststring"></param>
        /// <returns></returns>
        public async Task<HttpResult> Send(string Url, RequestHeader header = null, HttpMode Mode = HttpMode.Normal, WebProxy Proxy = null, bool allowRedirect = true, string postString = "")
        {
            if (!Url.IsProperUrl()) return null;
            if (header == null) header = new RequestHeader();
            int trynum = 0;
            HttpResult httpResult = null;

            try
            {
                while (trynum < AttemptCount && httpResult == null)
                {
                    httpResult = await Task.Run(() =>
                    {
                        HttpWebRequest Request;
                        HttpWebResponse Response = default;
                        try
                        {
                            Request = (HttpWebRequest)HttpWebRequest.Create(Url);
                        }
                        catch (Exception e)
                        {
                            Log($" {Jvedio.Language.Resources.Url}：{Url}， {Jvedio.Language.Resources.Reason}：{e.Message}");
                            return null;
                        }
                        Uri uri = new Uri(Url);
                        Request.Host = string.IsNullOrEmpty(header.Host) ? uri.Host : header.Host;
                        Request.Accept = header.Accept;
                        Request.Timeout = httpTimeOut.HTTP * 1000;
                        Request.Method = header.Method;
                        Request.KeepAlive = true;
                        Request.AllowAutoRedirect = allowRedirect;
                        Request.Referer = uri.Scheme + "://" + uri.Host + "/";
                        Request.UserAgent = header.UserAgent;
                        Request.Headers.Add("Accept-Language", header.AcceptLanguage);
                        Request.Headers.Add("Upgrade-Insecure-Requests", header.UpgradeInsecureRequests);
                        Request.Headers.Add("Sec-Fetch-Site", header.SecFetchSite);
                        Request.Headers.Add("Sec-Fetch-Mode", header.SecFetchMode);
                        Request.Headers.Add("Sec-Fetch-User", header.SecFetchUser);
                        Request.Headers.Add("Sec-Fetch-Dest", header.SecFetchDest);
                        Request.ReadWriteTimeout = httpTimeOut.ReadWrite;
                        if (header.Cookies != "") Request.Headers.Add("Cookie", header.Cookies);
                        if (Mode == HttpMode.RedirectGet) Request.AllowAutoRedirect = false;
                        if (Proxy != null) Request.Proxy = Proxy;

                        try
                        {
                            if (header.Method == "POST")
                            {
                                Request.Method = "POST";
                                Request.ContentType = header.ContentType;
                                Request.ContentLength = header.ContentLength;
                                Request.Headers.Add("Origin", header.Origin);
                                byte[] bs = Encoding.UTF8.GetBytes(postString);
                                using (Stream reqStream = Request.GetRequestStream())
                                {
                                    reqStream.Write(bs, 0, bs.Length);
                                }
                            }
                            Response = (HttpWebResponse)Request.GetResponse();
                            httpResult = GetHttpResult(Response, Mode);
                            Console.WriteLine($" {Jvedio.Language.Resources.Url}：{Url} => {httpResult.StatusCode}");
                        }
                        catch (WebException e)
                        {
                            Log($" {Jvedio.Language.Resources.Url}：{Url}， {Jvedio.Language.Resources.Reason}：{e.Message}");

                            httpResult = new HttpResult()
                            {
                                Error = e.Message,
                                Success = false,
                                SourceCode = ""
                            };

                            if (e.Status == WebExceptionStatus.Timeout)
                                trynum++;
                            else
                                trynum = 2;
                        }
                        catch (Exception e)
                        {
                            Log($" {Jvedio.Language.Resources.Url}：{Url}， {Jvedio.Language.Resources.Reason}：{e.Message}");
                            trynum = 2;
                        }
                        finally
                        {
                            if (Response != null) Response.Close();
                        }
                        return httpResult;
                    }).TimeoutAfter(TimeSpan.FromSeconds(httpTimeOut.HTTP));
                }
            }
            catch (TimeoutException ex)
            {
                //任务超时了
                Console.WriteLine(ex.Message);
            }
            return httpResult;
        }

        private HttpResult GetHttpResult(HttpWebResponse response, HttpMode mode)
        {
            HttpResult httpResult = new HttpResult();
            WebHeaderCollection webHeaderCollection = null;
            ResponseHeader responseHeaders = null;
            try
            {
                httpResult.StatusCode = response.StatusCode;
                webHeaderCollection = response.Headers;
            }
            catch (ObjectDisposedException ex)
            {
                Log(ex.Message);
            }
            if (webHeaderCollection != null)
            {
                //获得响应头
                responseHeaders = new ResponseHeader()
                {
                    Location = webHeaderCollection.Get("Location"),
                    Date = webHeaderCollection.Get("Date"),
                    ContentType = webHeaderCollection.Get("Content-Type"),
                    Connection = webHeaderCollection.Get("Connection"),
                    CacheControl = webHeaderCollection.Get("Cache-Control"),
                    SetCookie = webHeaderCollection.Get("Set-Cookie"),
                };
                double.TryParse(webHeaderCollection.Get("Content-Length"), out double contentLength);
                responseHeaders.ContentLength = contentLength;
                httpResult.Headers = responseHeaders;
            }
            if (response.StatusCode == HttpStatusCode.OK)
            {
                if (mode != HttpMode.Stream)
                {
                    try
                    {
                        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        {
                            httpResult.SourceCode = sr.ReadToEnd();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(ex);
                    }
                    if (responseHeaders?.ContentLength == 0) responseHeaders.ContentLength = httpResult.SourceCode.Length;
                }
                else
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            response.GetResponseStream().CopyTo(ms);
                            httpResult.FileByte = ms.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(ex);
                    }
                    if (responseHeaders?.ContentLength == 0) responseHeaders.ContentLength = httpResult.FileByte.Length;
                }
            }
            return httpResult;
        }

        public async Task<HttpResult> Send(string Url)
        {
            return await Send(Url, null);
        }

        public void Log(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void Log(string str)
        {
            throw new NotImplementedException();
        }
    }
}
