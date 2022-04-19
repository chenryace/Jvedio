using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Net
{
    public class HttpResult
    {

        public HttpResult()
        {
            Success = false;//是否成功获取请求
            Error = "";//如果发生错误，则保存错误信息
            SourceCode = "";// html 源码
            FileByte = null;//如果返回的是文件，则保存文件
            MovieCode = "";//网址中影片对应的地址
            StatusCode = HttpStatusCode.Forbidden;
            Headers = null;//返回体
        }




        public bool Success { get; set; }
        public string Error { get; set; }
        public string SourceCode { get; set; }
        public byte[] FileByte { get; set; }
        public string MovieCode { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public ResponseHeader Headers { get; set; }
    }
}
