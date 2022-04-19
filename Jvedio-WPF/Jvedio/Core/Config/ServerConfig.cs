using Jvedio.Core.Crawler;
using Jvedio.Core.SimpleORM;
using Jvedio.Core.WindowConfig;
using Jvedio.Entity;
using Jvedio.Entity.CommonSQL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Config
{
    public class ServerConfig : AbstractConfig
    {
        private ServerConfig() : base("Servers")
        {
        }

        private static ServerConfig instance = null;

        public static ServerConfig createInstance()
        {
            if (instance == null) instance = new ServerConfig();
            return instance;
        }

        public List<CrawlerServer> CrawlerServers { get; set; }


        public override void Read()
        {
            SelectWrapper<AppConfig> wrapper = new SelectWrapper<AppConfig>();
            wrapper.Eq("ConfigName", ConfigName);
            AppConfig appConfig = GlobalMapper.appConfigMapper.selectOne(wrapper);
            if (appConfig == null || appConfig.ConfigId == 0) return;
            List<Dictionary<object, object>> dicts = JsonConvert.DeserializeObject<List<Dictionary<object, object>>>(appConfig.ConfigValue);

            CrawlerServers = new List<CrawlerServer>();
            foreach (var dict in dicts)
            {
                string ServerType = dict["ServerType"].ToString();
                string Datas = dict["Datas"].ToString();

                List<Dictionary<object, object>> ds = JsonConvert.DeserializeObject<List<Dictionary<object, object>>>(Datas);
                foreach (var d in ds)
                {
                    CrawlerServer server = new CrawlerServer();
                    server.ServerType = ServerType;
                    server.Url = d["Url"].ToString();
                    server.Cookies = d["Cookies"].ToString();
                    server.Enabled = d["Enabled"].ToString().ToLower() == "true" ? true : false;
                    server.LastRefreshDate = d["LastRefreshDate"].ToString();
                    server.Headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(d["Headers"].ToString());
                    CrawlerServers.Add(server);
                    int.TryParse(d["Available"].ToString(), out int available);
                    server.Available = available;
                }
            }

        }


        private class _CrawlerServer
        {
            public string ServerType { get; set; }
            public List<string> Datas { get; set; }
        }

        public override void Save()
        {

            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            if (CrawlerServers != null)
            {
                foreach (CrawlerServer server in CrawlerServers)
                {
                    string type = server.ServerType;
                    string json = JsonConvert.SerializeObject(server);
                    if (dict.ContainsKey(type))
                    {
                        dict[type].Add(json);
                    }
                    else
                    {
                        List<string> list = new List<string>();
                        list.Add(json);
                        dict[type] = list;
                    }
                }
            }

            List<_CrawlerServer> result = new List<_CrawlerServer>();
            foreach (string key in dict.Keys)
            {
                List<string> list = dict[key];
                _CrawlerServer _server = new _CrawlerServer();
                _server.ServerType = key;
                _server.Datas = list;
                result.Add(_server);
            }

            AppConfig appConfig = new AppConfig();
            appConfig.ConfigName = ConfigName;
            appConfig.ConfigValue = JsonConvert.SerializeObject(result); ;
            Console.WriteLine();
            GlobalMapper.appConfigMapper.insert(appConfig, Enums.InsertMode.Replace);
        }



    }
}
