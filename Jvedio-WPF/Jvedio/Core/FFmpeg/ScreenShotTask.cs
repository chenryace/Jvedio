using DynamicData.Annotations;
using Jvedio.Utils;
using Jvedio.Utils.Common;
using Jvedio.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jvedio.Core.SimpleORM;
using Jvedio.Mapper;
using static Jvedio.GlobalMapper;
using Jvedio.Core.Scan;
using Jvedio.Core.Enums;
using Jvedio.Core.Exceptions;
using Jvedio.CommonNet.Crawler;
using Newtonsoft.Json;
using Jvedio.Core.CustomEventArgs;
using Jvedio.Core.CustomTask;
using Jvedio.CommonNet.Entity;

namespace Jvedio.Core.FFmpeg
{
    public class ScreenShotTask : AbstractTask
    {


        public bool DownloadPreview { get; set; }

        public event EventHandler onDownloadSuccess;
        public event EventHandler onDownloadPreview;









        public ScreenShotTask(Video video, bool downloadPreview = false, bool overrideInfo = false) : this(video.toMetaData())
        {
            Title = string.IsNullOrEmpty(video.VID) ? video.Title : video.VID;
            DownloadPreview = downloadPreview;
            OverrideInfo = overrideInfo;
        }



        public ScreenShotTask(MetaData data) : base()
        {
            DataID = data.DataID;
            DataType = data.DataType;


        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is ScreenShotTask other)
                return other.DataID.Equals(DataID);
            return false;

        }

        public override int GetHashCode()
        {
            return DataID.GetHashCode();
        }


        public long DataID { get; set; }
        public DataType DataType { get; set; }
        public string Title { get; set; }
        public bool OverrideInfo { get; set; }//强制下载覆盖信息




        public override void doWrok()
        {

        }
    }
}
