using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Plugins.Crawler
{
    /// <summary>
    /// 加载爬虫插件
    /// </summary>
    public class CrawlerLoader
    {
        /**
         * 文件类型：DLL 文件，或者 cs 文件
         * 执行方式：反射加载
         * 
         */

        private static string BaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "crawlers");

        private static List<string> BaseDll = new List<string>() { "HtmlAgilityPack.dll", "Common.dll" };


        public static void loadAllCrawlers()
        {
            // 扫描
            List<string> list = FileHelper.TryGetAllFiles(BaseDir, "*.dll").ToList();
            list.RemoveAll(arg => BaseDll.Contains(Path.GetFileName(arg)));
            foreach (string dllPath in list)
            {

                Assembly dll = Assembly.LoadFrom(dllPath);
                Type classType = getPublicType(dll.GetTypes());
                if (classType == null) continue;
                Dictionary<string, string> info = getInfo(classType);
                if (info == null || !info.ContainsKey("ServerType")) continue;
                PluginInfo pluginInfo = PluginInfo.ParseDict(info);
                if (pluginInfo != null)
                    Global.Plugins.Crawlers.Add(pluginInfo);
            }
            Console.WriteLine(Global.Plugins.Crawlers);
        }



        async void testWithAssembly()
        {
            //string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "crawlers", "BusCrawler.dll");
            //string methodName = "GetInfo";
            //object[] param = new object[] { url, cookies };
            //if (File.Exists(dllPath))
            //{
            //    Assembly dll = Assembly.LoadFrom(dllPath);
            //    Type classType = getPublicType(dll.GetTypes());
            //    if (classType != null)
            //    {
            //        Dictionary<string, string> infos = getInfo(classType);
            //        var instance = Activator.CreateInstance(classType, url, cookies, header);
            //        MethodInfo methodInfo = classType.GetMethod(methodName);
            //        if (methodInfo != null)
            //        {
            //            Console.WriteLine("开始爬取");
            //            object result = await Task.Run(() =>
            //            {
            //                return methodInfo.Invoke(instance, null);
            //            });
            //            Console.WriteLine(result);
            //            Console.WriteLine("爬取结束");
            //        }
            //    }
            //}
        }

        private static Type getPublicType(Type[] types)
        {
            if (types == null || types.Length == 0) return null;
            foreach (Type type in types)
            {
                if (type.IsPublic) return type;
            }
            return null;
        }

        public static Dictionary<string, string> getInfo(Type type)
        {
            FieldInfo fieldInfo = type.GetField("Infos");
            if (fieldInfo != null)
            {
                object value = fieldInfo.GetValue(null);
                if (value != null)
                {
                    try
                    {
                        return (Dictionary<string, string>)value;
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }

                }
            }
            return null;
        }




    }
}
