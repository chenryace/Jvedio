using DynamicData.Annotations;
using Jvedio.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jvedio.Core.Crawler
{

    /// <summary>
    /// 服务器源
    /// </summary>
    public class CrawlerServer : INotifyPropertyChanged
    {
        public CrawlerServer()
        {

        }


        public string ServerType { get; set; }
        public string Url { get; set; }
        public bool Enabled { get; set; }
        public int Available { get; set; }
        public string LastRefreshDate { get; set; }
        public string Cookies { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }

}
