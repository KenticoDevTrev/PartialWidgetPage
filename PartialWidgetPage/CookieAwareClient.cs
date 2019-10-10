using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public string RequestUserAgent { get; set; }
        public bool StripSession { get; set; }

        public CookieAwareWebClient()
            : this(new CookieContainer())
        {
        }

        /// <summary>
        /// Creates web client with cookies
        /// </summary>
        /// <param name="cookies"></param>
        public CookieAwareWebClient(CookieContainer cookies, bool stripSession = true)
        {
            CookieContainer = cookies;
            StripSession = stripSession;
        }

        /// <summary>
        /// Creates web client with the cookies of the given Request
        /// </summary>
        /// <param name="CurrentRequest">The current request to copy cookie from</param>
        public CookieAwareWebClient(HttpRequest CurrentRequest, bool stripSession = true)
        {
            StripSession = stripSession;
            // Copy Cookies
            CookieContainer cookieJar = new CookieContainer();
            foreach (string CookieKey in CurrentRequest.Cookies?.AllKeys)
            {
                if (CookieKey != "ASP.NET_SessionId" || !stripSession)
                {
                    var Cookie = CurrentRequest.Cookies[CookieKey];
                    // CookieContainer requires a domain. so if it's empty, then use the Current Request's Host
                    cookieJar.Add(new Cookie(CookieKey, Cookie.Value, Cookie.Path, string.IsNullOrWhiteSpace(Cookie.Domain) ? CurrentRequest.Url.Host : Cookie.Domain));
                }
            }
            CookieContainer = cookieJar;
            RequestUserAgent = CurrentRequest.UserAgent;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            HttpWebRequest httpRequest = (HttpWebRequest)request;
            if (httpRequest != null)
            {
                httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                httpRequest.CookieContainer = this.CookieContainer;
                httpRequest.UserAgent = RequestUserAgent;
            }
            return httpRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            String setCookieHeader = response.Headers[HttpResponseHeader.SetCookie];

            //do something if needed to parse out the cookie.
            if (setCookieHeader != null)
            {
                // get the cookies from the response 
                this.CookieContainer.SetCookies(response.ResponseUri, setCookieHeader);
            }

            return response;
        }
    }
}
