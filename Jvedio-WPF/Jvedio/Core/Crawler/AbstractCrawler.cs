using Jvedio.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Crawler
{
    public abstract class AbstractCrawler
    {

        protected string Url { get; set; }
        protected string Cookies { get; set; }
        protected string HtmlText { get; set; }
        protected RequestHeader Header = new RequestHeader();
        protected HttpResult httpResult = null;


        public AbstractCrawler(string url, string cookies, RequestHeader header)
        {
            Url = url;
            Cookies = cookies;
            Header = header;
        }


        /// <summary>
        /// 初始化请求头
        /// </summary>
        protected abstract void InitHeaders();

        /// <summary>
        /// 表示如何解析 html 文件或者从网络传输中获取信息
        /// </summary>
        /// <returns>键值对</returns>
        protected abstract Dictionary<string, string> ParseInfo();

        /// <summary>
        /// 持久化保存 Cookies
        /// </summary>
        /// <param name="SetCookie"></param>
        protected abstract string GetCookies(string SetCookie);

        /// <summary>
        /// 爬取逻辑
        /// </summary>
        /// <returns>Http 结果</returns>
        public abstract Task<HttpResult> Crawl();

    }
}
