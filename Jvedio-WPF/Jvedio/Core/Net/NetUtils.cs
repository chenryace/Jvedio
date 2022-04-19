using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jvedio.Core.Net
{
    public static class NetUtils
    {
        public static string ToProperUrl(this string url)
        {
            url = url.ToLower();
            if (string.IsNullOrEmpty(url)) return "";
            if (url.IndexOf("http") < 0) url = "https://" + url;
            if (!url.EndsWith("/")) url += "/";
            return url;
        }

        public static bool IsProperUrl(this string source) => Uri.TryCreate(source, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

    }
}
