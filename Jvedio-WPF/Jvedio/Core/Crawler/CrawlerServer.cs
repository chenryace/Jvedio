using DynamicData.Annotations;
using Jvedio.Utils;
using Newtonsoft.Json;
using System;
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
        private string _Url;
        public string Url
        {
            get
            {
                return _Url;
            }

            set
            {
                _Url = value;
                OnPropertyChanged();
            }
        }


        private bool _Enabled;
        public bool Enabled
        {
            get
            {
                return _Enabled;
            }

            set
            {
                _Enabled = value;
                OnPropertyChanged();
            }
        }

        private int _Available;
        public int Available
        {
            get
            {
                return _Available;
            }

            set
            {
                _Available = value;
                OnPropertyChanged();
            }
        }
        public string LastRefreshDate { get; set; }

        private string _Cookies;
        public string Cookies
        {
            get
            {
                return _Cookies;
            }

            set
            {
                _Cookies = value;
                OnPropertyChanged();
            }
        }
        public string _Headers { get; set; }
        public string Headers
        {
            get
            {
                return _Headers;
            }

            set
            {
                _Headers = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public bool isHeaderProper()
        {
            if (string.IsNullOrEmpty(Headers)) return true;
            try
            {
                Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Headers);
                return true;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return false;
            }

        }

    }

}
