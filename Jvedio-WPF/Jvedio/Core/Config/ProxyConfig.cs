﻿using Jvedio.Core.WindowConfig;
using MihaZupan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Config
{
    public class ProxyConfig : AbstractConfig
    {

        static int DEFAULT_TIMEOUT = 10;

        private ProxyConfig() : base("ProxyConfig")
        {
            ProxyMode = 1;
            ProxyType = 1;
            HttpTimeout = DEFAULT_TIMEOUT;
        }

        private static ProxyConfig _instance = null;

        public static ProxyConfig createInstance()
        {
            if (_instance == null) _instance = new ProxyConfig();

            return _instance;
        }

        /// <summary>
        /// 0-无代理 1-系统代理 2-自定义代理
        /// </summary>
        public long ProxyMode { get; set; }


        // 自定义代理配置
        public long ProxyType { get; set; }  // 0-HTTP 1-SOCKS
        public string Server { get; set; }
        public long Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        private long _HttpTimeout = 10;
        public long HttpTimeout
        {
            get
            {
                return _HttpTimeout;
            }
            set
            {
                if (value <= 0)
                    _HttpTimeout = DEFAULT_TIMEOUT;
                else
                    _HttpTimeout = value;
            }
        }






        public IWebProxy GetWebProxy()
        {
            WebProxy proxy = null;
            if (ProxyMode == 0)
            {
                return proxy;
            }
            else if (ProxyMode == 1)
            {
                proxy = WebProxy.GetDefaultProxy();
                if (proxy.Address != null)
                {
                    proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    return new System.Net.WebProxy(proxy.Address, proxy.BypassProxyOnLocal, proxy.BypassList, proxy.Credentials);
                }
            }
            else if (ProxyMode == 2)
            {
                if (string.IsNullOrEmpty(Server) || Port <= 0)
                {
                    return null;
                }
                else
                {
                    if (ProxyType == 0)
                    {
                        // http
                        proxy = new WebProxy(Server, (int)Port);
                        if (!string.IsNullOrEmpty(UserName) || !string.IsNullOrEmpty(Password))
                        {
                            NetworkCredential credential = new NetworkCredential(UserName, Password);
                            if (credential != null) proxy.Credentials = credential;
                        }
                    }
                    else
                    {
                        // socks

                        if (!string.IsNullOrEmpty(UserName) || !string.IsNullOrEmpty(Password))
                        {
                            return new HttpToSocks5Proxy(Server, (int)Port, "username", "password");
                        }
                        else
                        {
                            return new HttpToSocks5Proxy(Server, (int)Port);
                        }
                    }
                }
            }
            return proxy;
        }
    }
}
