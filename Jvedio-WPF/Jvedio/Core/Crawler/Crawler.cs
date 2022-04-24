
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Jvedio.GlobalVariable;
using static Jvedio.GlobalMapper;
using System.IO;
using System.Net;
using Jvedio.Utils;
using Jvedio.Utils.Net;

using Jvedio.Entity;
using Jvedio.Mapper;
using Jvedio.Core.Enums;
using Jvedio.Core.Crawler;

namespace Jvedio
{







    //public class LibraryCrawler : AbstractCrawler
    //{
    //    protected string MovieCode;
    //    public LibraryCrawler(string Id) : base(Id)
    //    {
    //        Url = JvedioServers.Library.Url + $"vl_searchbyid.php?keyword={ID}";
    //        if (Url.IsProperUrl()) InitHeaders();
    //    }

    //    protected async Task<string> GetMovieCode()
    //    {
    //        string movieCode = DataBase.SelectInfoByID("code", "library", ID);
    //        //先从数据库获取
    //        if (string.IsNullOrEmpty(movieCode) || movieCode.IndexOf("zh-cn") >= 0)
    //        {
    //            HttpResult result = await new BaseHttp().Send(Url, headers, Mode: HttpMode.RedirectGet);
    //            if (result != null && result.StatusCode == HttpStatusCode.Redirect) movieCode = result.Headers.Location;
    //            else if (result != null) movieCode = GetMovieCodeFromSearchResult(result.SourceCode);

    //            if (movieCode.IndexOf("=") >= 0) movieCode = movieCode.Split('=').Last();
    //        }

    //        //存入数据库
    //        if (movieCode != "" && movieCode.IndexOf("zh-cn") < 0)
    //        {
    //            DataBase.SaveMovieCodeByID(ID, "library", movieCode);
    //        }

    //        return movieCode;
    //    }

    //    private string GetMovieCodeFromSearchResult(string html)
    //    {
    //        string result = "";
    //        if (string.IsNullOrEmpty(html)) return result;
    //        HtmlDocument doc = new HtmlDocument();
    //        doc.LoadHtml(html);
    //        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='id']");
    //        if (nodes != null)
    //        {
    //            HtmlNode linknode = null;
    //            string id = "";
    //            foreach (HtmlNode node in nodes)
    //            {
    //                if (node == null) continue;
    //                id = node.InnerText;
    //                if (!string.IsNullOrEmpty(id) && id.ToUpper() == ID.ToUpper())
    //                {
    //                    linknode = node.ParentNode;
    //                    if (linknode != null)
    //                    {
    //                        string link = linknode.Attributes["href"]?.Value;
    //                        if (link.IndexOf("=") > 0) result = link.Split('=').Last();
    //                    }
    //                    break;

    //                }
    //            }
    //        }


    //        return result;

    //    }

    //    protected override void InitHeaders()
    //    {
    //        headers = new RequestHeader() { Cookies = JvedioServers.Library.Cookie };
    //    }


    //    public override async Task<HttpResult> Crawl()
    //    {
    //        MovieCode = await GetMovieCode();
    //        if (MovieCode != "")
    //        {
    //            //解析
    //            Url = JvedioServers.Library.Url + $"?v={MovieCode}";
    //            httpResult = await new BaseHttp().Send(Url, headers);
    //            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
    //            {
    //                FileProcess.SaveInfo(GetInfo(), ID);
    //                httpResult.Success = true;
    //                ParseCookies(httpResult.Headers.SetCookie);
    //            }
    //        }
    //        return httpResult;
    //    }

    //    protected override void ParseCookies(string SetCookie)
    //    {
    //        if (string.IsNullOrEmpty(SetCookie)) return;
    //        List<string> Cookies = new List<string>();
    //        var values = SetCookie.Split(new char[] { ',', ';' }).ToList();
    //        foreach (var item in values)
    //        {
    //            if (item.IndexOf('=') < 0) continue;
    //            string key = item.Split('=')[0];
    //            string value = item.Split('=')[1];
    //            if (key == "__cfduid" || key == "__qca") Cookies.Add(key + "=" + value);
    //        }
    //        Cookies.Add("over18=18");
    //        string cookie = string.Join(";", Cookies);
    //        JvedioServers.Library.Cookie = cookie;
    //        JvedioServers.Save();
    //    }

    //    protected override Dictionary<string, string> GetInfo()
    //    {
    //        Dictionary<string, string> Info = new LibraryParse(ID, httpResult.SourceCode).Parse();
    //        if (Info.Count > 0)
    //        {
    //            Info.Add("id", ID);
    //            Info.Add("sourceurl", Url);
    //            Info.Add("source", "javlibrary");
    //            Task.Delay(Delay.MEDIUM).Wait();
    //        }
    //        return Info;
    //    }


    //}

    //public class FANZACrawler : AbstractCrawler
    //{

    //    protected string MovieCode = "";
    //    protected bool OnlyPlot;//是否仅爬取摘要
    //    public FANZACrawler(string Id, bool onlyPlot = false) : base(Id)
    //    {
    //        Url = $"{JvedioServers.DMM.Url}search/?redirect=1&enc=UTF-8&category=mono_dvd&searchstr={ID}&commit.x=5&commit.y=18";
    //        if (Url.IsProperUrl()) InitHeaders();
    //        OnlyPlot = onlyPlot;
    //    }

    //    protected async Task<string> GetMovieCode()
    //    {
    //        //从网络获取
    //        string movieCode = "";
    //        string link = "";
    //        HttpResult result = await new BaseHttp().Send(Url, headers, Mode: HttpMode.RedirectGet);
    //        if (result != null && result.StatusCode == HttpStatusCode.Redirect)
    //            link = result.Headers.Location;//https://www.dmm.co.jp/mono/dvd/-/search/=/searchstr=ABP-123/

    //        if (!string.IsNullOrEmpty(link))
    //        {
    //            HttpResult newResult = await new BaseHttp().Send(link, headers);
    //            if (newResult != null && newResult.StatusCode == HttpStatusCode.OK)
    //            {
    //                if (newResult.SourceCode.IndexOf("に一致する商品は見つかりませんでした") > 0)
    //                {
    //                    //不存在
    //                }
    //                else
    //                {
    //                    //存在并解析
    //                    movieCode = GetLinkFromSearchResult(newResult.SourceCode);
    //                }
    //            }

    //        }
    //        return movieCode;
    //    }

    //    private string GetLinkFromSearchResult(string html)
    //    {
    //        string result = "";
    //        if (string.IsNullOrEmpty(html)) return result;
    //        HtmlDocument doc = new HtmlDocument();
    //        doc.LoadHtml(html);
    //        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//p[@class='tmb']/a");

    //        if (nodes != null)
    //        {
    //            foreach (HtmlNode node in nodes)
    //            {
    //                if (node == null) continue;
    //                string link = node.Attributes["href"]?.Value;
    //                if (link.IsProperUrl())
    //                {
    //                    string fanhao = Identify.GetVIDFromDMMUrl(link);
    //                    if (Identify.GetEng(fanhao).ToUpper() == Identify.GetEng(ID).ToUpper())
    //                    {
    //                        string str1 = Identify.GetNum(fanhao);
    //                        string str2 = Identify.GetNum(ID);
    //                        int num1 = 0;
    //                        int num2 = 1;
    //                        int.TryParse(str1, out num1);
    //                        int.TryParse(str2, out num2);
    //                        if (num1 == num2)
    //                        {
    //                            result = link;
    //                            break;
    //                        }
    //                    }

    //                }
    //            }
    //        }


    //        return result;

    //    }




    //    public override async Task<HttpResult> Crawl()
    //    {
    //        MovieCode = await GetMovieCode();
    //        if (MovieCode != "")
    //        {
    //            //解析
    //            Url = MovieCode;
    //            httpResult = await new BaseHttp().Send(Url, headers);
    //            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
    //            {
    //                if (!this.OnlyPlot)
    //                    FileProcess.SaveInfo(GetInfo(), ID);
    //                else
    //                    FileProcess.SavePartialInfo(GetInfo(), "plot", ID);
    //                httpResult.Success = true;
    //            }
    //        }
    //        return httpResult;
    //    }

    //    protected override Dictionary<string, string> GetInfo()
    //    {
    //        Dictionary<string, string> Info = new Dictionary<string, string>();
    //        Info = new FanzaParse(ID, httpResult.SourceCode).Parse();
    //        if (Info.Count > 0)
    //        {
    //            Info.Add("id", ID);
    //            Info.Add("sourceurl", Url);
    //            Info.Add("source", "FANZA");
    //            Task.Delay(Delay.MEDIUM).Wait();
    //        }
    //        return Info;
    //    }

    //    protected override void InitHeaders()
    //    {
    //        headers = new RequestHeader() { Cookies = JvedioServers.DMM.Cookie };
    //    }

    //    protected override void ParseCookies(string SetCookie)
    //    {
    //        return;
    //    }
    //}


    //public class MOOCrawler : AbstractCrawler
    //{

    //    protected string MovieCode;
    //    public MOOCrawler(string Id) : base(Id)
    //    {
    //        Url = JvedioServers.MOO.Url + $"search/{ID}";
    //        if (Url.IsProperUrl()) InitHeaders();
    //    }
    //    protected override void InitHeaders()
    //    {
    //        headers = new RequestHeader() { Cookies = JvedioServers.MOO.Cookie };
    //    }

    //    public async Task<string> GetMovieCode(Action<string> callback = null)
    //    {

    //        //从网络获取
    //        HttpResult result = await new BaseHttp().Send(Url, headers, allowRedirect: false);
    //        //if (result != null && result.StatusCode == HttpStatusCode.Redirect) callback?.Invoke(Jvedio.Language.Resources.SearchTooFrequent);
    //        if (result != null && result.SourceCode != "")
    //            return GetMovieCodeFromSearchResult(result.SourceCode);

    //        //未找到

    //        //搜索太频繁

    //        return "";
    //    }

    //    protected string GetMovieCodeFromSearchResult(string content)
    //    {

    //        HtmlDocument doc = new HtmlDocument();
    //        doc.LoadHtml(content);

    //        HtmlNodeCollection gridNodes = doc.DocumentNode.SelectNodes("//a[@class='movie-box']");
    //        if (gridNodes != null)
    //        {
    //            foreach (HtmlNode gridNode in gridNodes)
    //            {
    //                HtmlNode htmlNode = gridNode.SelectSingleNode("div/span/date");
    //                if (htmlNode != null && htmlNode.InnerText.ToUpper() == ID.ToUpper())
    //                {
    //                    string link = gridNode.Attributes["href"]?.Value;
    //                    if (!string.IsNullOrEmpty(link) && link.IndexOf("/") > 0) return link.Split('/').Last();

    //                }
    //            }
    //        }
    //        return "";
    //    }



    //    public override async Task<HttpResult> Crawl()
    //    {
    //        MovieCode = await GetMovieCode((error) =>
    //        {
    //            httpResult = new HttpResult() { Error = error, Success = false };
    //        });
    //        if (MovieCode != "")
    //        {
    //            Url = JvedioServers.MOO.Url + $"movie/{MovieCode}";
    //            httpResult = await new BaseHttp().Send(Url, headers);
    //            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
    //            {
    //                FileProcess.SaveInfo(GetInfo(), ID);
    //                httpResult.Success = true;
    //            }
    //        }
    //        return httpResult;

    //    }

    //    protected override void ParseCookies(string SetCookie)
    //    {
    //        return;
    //    }


    //    protected override Dictionary<string, string> GetInfo()
    //    {
    //        Dictionary<string, string> Info = new MOOParse(ID, httpResult.SourceCode).Parse();
    //        if (Info.Count > 0)
    //        {
    //            Info.Add("id", ID);
    //            Info.Add("sourceurl", Url);
    //            Info.Add("source", "AVMOO");
    //            Task.Delay(Delay.MEDIUM).Wait();
    //        }
    //        return Info;
    //    }


    //}

    //public class Jav321Crawler : AbstractCrawler
    //{
    //    protected string MovieCode = "";
    //    public Jav321Crawler(string Id) : base(Id)
    //    {
    //        Url = JvedioServers.Jav321.Url + $"search";
    //    }

    //    protected void InitHeaders(string postdata)
    //    {
    //        //sn=pppd-093
    //        if (!Url.IsProperUrl()) return;
    //        Uri uri = new Uri(Url);
    //        headers = new RequestHeader()
    //        {

    //            ContentLength = postdata.Length + 3,
    //            Origin = uri.Scheme + "://" + uri.Host,
    //            ContentType = "application/x-www-form-urlencoded",
    //            Referer = uri.Scheme + "://" + uri.Host,
    //            Method = "POST",
    //            Cookies = JvedioServers.Jav321.Cookie
    //        };
    //    }

    //    protected override void InitHeaders()
    //    {
    //        headers = new RequestHeader() { Cookies = JvedioServers.Jav321.Cookie };
    //    }


    //    //TODO
    //    public async Task<string> GetMovieCode(Action<string> callback = null)
    //    {
    //        //从网络获取
    //        InitHeaders(ID);
    //        HttpResult result = await new BaseHttp().Send(Url, headers, allowRedirect: false, postString: $"sn={ID}");
    //        if (result != null && result.StatusCode == HttpStatusCode.MovedPermanently && !string.IsNullOrEmpty(result.Headers.Location))
    //        {
    //            return result.Headers.Location;
    //        }
    //        //未找到

    //        //搜索太频繁

    //        return "";
    //    }

    //    public override async Task<HttpResult> Crawl()
    //    {
    //        //从网络获取

    //        MovieCode = await GetMovieCode((error) =>
    //        {
    //            httpResult = new HttpResult() { Error = error, Success = false };
    //        });
    //        if (MovieCode.Length > 1)
    //        {
    //            InitHeaders();
    //            Url = JvedioServers.Jav321.Url + MovieCode.Substring(1);
    //            httpResult = await new BaseHttp().Send(Url, headers);
    //            if (httpResult != null && httpResult.StatusCode == HttpStatusCode.OK && httpResult.SourceCode != null)
    //            {
    //                FileProcess.SaveInfo(GetInfo(), ID);
    //                httpResult.Success = true;
    //                ParseCookies(httpResult.Headers.SetCookie);
    //            }
    //        }
    //        return httpResult;
    //    }




    //    protected override Dictionary<string, string> GetInfo()
    //    {
    //        Dictionary<string, string> Info = new Jav321Parse(ID, httpResult.SourceCode).Parse();
    //        if (Info.Count > 0)
    //        {
    //            Info.Add("id", ID);
    //            Info.Add("sourceurl", Url);
    //            Info.Add("source", "JAV321");
    //            Task.Delay(Delay.MEDIUM).Wait();
    //        }
    //        return Info;
    //    }


    //    protected override void ParseCookies(string SetCookie)
    //    {
    //        if (string.IsNullOrEmpty(SetCookie)) return;
    //        List<string> Cookies = new List<string>();
    //        var values = SetCookie.Split(new char[] { ',', ';' }).ToList();
    //        foreach (var item in values)
    //        {
    //            if (item.IndexOf('=') < 0) continue;
    //            string key = item.Split('=')[0];
    //            string value = item.Split('=')[1];
    //            if (key == "__cfduid" || key == "is_loyal") Cookies.Add(key + "=" + value);
    //        }
    //        string cookie = string.Join(";", Cookies);
    //        JvedioServers.Jav321.Cookie = cookie;
    //        JvedioServers.Save();
    //    }
    //}



}
