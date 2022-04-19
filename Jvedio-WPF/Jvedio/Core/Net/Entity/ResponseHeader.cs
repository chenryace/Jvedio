using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Net
{
    public class ResponseHeader
    {

        public ResponseHeader()
        {
            Date = "";
            ContentType = "";
            Connection = "";
            CacheControl = "";
            SetCookie = "";
            Location = "";
            ContentLength = 0;
        }

        public string Date { get; set; }
        public string ContentType { get; set; }
        public string Connection { get; set; }
        public string CacheControl { get; set; }
        public string SetCookie { get; set; }
        public string Location { get; set; }
        public double ContentLength { get; set; }
    }
}
