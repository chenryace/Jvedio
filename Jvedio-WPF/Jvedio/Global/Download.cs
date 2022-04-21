using Jvedio.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Global
{
    public static class Download
    {
        public static DownloadDispatcher Dispatcher { get; set; }

        static Download()
        {
            Dispatcher = DownloadDispatcher.createInstance();
        }


    }
}
