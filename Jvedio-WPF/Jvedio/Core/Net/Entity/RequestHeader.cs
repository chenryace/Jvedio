using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Net
{
    public class RequestHeader
    {
        public RequestHeader()
        {
            Method = "GET";
            Host = "";
            Connection = "keep-alive";
            CacheControl = "max-age=0";
            UpgradeInsecureRequests = "1";
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36";
            Accept = "*/*";
            SecFetchSite = "same-origin";
            SecFetchMode = "navigate";
            SecFetchUser = "?1";
            SecFetchDest = "document";
            AcceptEncoding = "";
            AcceptLanguage = "zh-CN,zh;q=0.9";
            Cookies = "";
            Referer = "";
            Origin = "";
            ContentLength = 0;
            ContentType = "";
        }

        public string Method { get; set; }
        public string Host { get; set; }
        public string Connection { get; set; }
        public string CacheControl { get; set; }
        public string UpgradeInsecureRequests { get; set; }
        public string UserAgent { get; set; }
        public string Accept { get; set; }
        public string SecFetchSite { get; set; }
        public string SecFetchMode { get; set; }
        public string SecFetchUser { get; set; }
        public string SecFetchDest { get; set; }
        public string AcceptEncoding { get; set; }
        public string AcceptLanguage { get; set; }
        public string Cookies { get; set; }
        public string Referer { get; set; }
        public string Origin { get; set; }
        public long ContentLength { get; set; }
        public string ContentType { get; set; }
    }
}
