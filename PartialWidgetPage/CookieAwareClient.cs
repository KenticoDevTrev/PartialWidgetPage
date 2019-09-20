using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static System.Collections.Specialized.NameObjectCollectionBase;

namespace PartialWidgetPage
{
    public class CookieAwareWebClient : WebClient
    {
        public CookieContainer CookieContainer { get; set; }
        public Uri Uri { get; set; }

        public CookieAwareWebClient()
            : this(new CookieContainer())
        {
        }

        /// <summary>
        /// Creates web client with cookies
        /// </summary>
        /// <param name="cookies"></param>
        public CookieAwareWebClient(CookieContainer cookies)
        {
            CookieContainer = cookies;
        }

        /// <summary>
        /// Creates web client with the cookies of the given Request
        /// </summary>
        /// <param name="CurrentRequest">The current request to copy cookie from</param>
        public CookieAwareWebClient(HttpRequest CurrentRequest)
        {
            // Copy Cookies
            CookieContainer cookieJar = new CookieContainer();
            foreach (string CookieKey in CurrentRequest.Cookies)
            {
                var Cookie = CurrentRequest.Cookies[CookieKey];
                // CookieContainer requires a domain. so if it's empty, then use the Current Request's Host
                cookieJar.Add(new Cookie(CookieKey, Cookie.Value, Cookie.Path, !string.IsNullOrWhiteSpace(Cookie.Domain) ? Cookie.Domain : CurrentRequest.Url.Host));
            }
            CookieContainer = cookieJar;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = this.CookieContainer;
            }
            HttpWebRequest httpRequest = (HttpWebRequest)request;
            httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return httpRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            String setCookieHeader = response.Headers[HttpResponseHeader.SetCookie];

            //do something if needed to parse out the cookie.
            if (setCookieHeader != null)
            {
                Cookie cookie = new Cookie(); //create cookie
                this.CookieContainer.Add(cookie);
            }

            return response;
        }
    }
}
