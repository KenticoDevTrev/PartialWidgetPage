using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
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

        private const string _SessionIDCookie = "ASP.NET_SessionId";

        public CookieAwareWebClient()
            : this(new CookieContainer())
        {
        }

        /// <summary>
        /// Creates web client with cookies
        /// </summary>
        /// <param name="cookies"></param>
        /// <param name="stripSession">Strips the ASP.Net_SessionId because by default Sessions have a lock on them, so additional requests during the initial request will cause a timeout. Can override, however must make sure initial request has a read-only session attribute</param>
        public CookieAwareWebClient(CookieContainer cookies)
        {
            CookieContainer = cookies;
        }



        /// <summary>
        /// Creates web client with the cookies of the given Request
        /// </summary>
        /// <param name="CurrentRequest">The current request to copy cookie from</param>
        /// <param name="stripSession">Strips the ASP.Net_SessionId because by default Sessions have a lock on them, so additional requests during the initial request will cause a timeout. Can override, however must make sure initial request has a read-only session attribute</param>

        public CookieAwareWebClient(HttpRequest CurrentRequest, bool StripSession = true)
        {
            // Copy Cookies
            CookieContainer cookieJar = new CookieContainer();

            // Get list of cookies
            List<string> CookieKeys = CurrentRequest.Cookies?.AllKeys.ToList();
            if (StripSession)
            {
                CookieKeys.RemoveAll(x => x.Equals(_SessionIDCookie, StringComparison.InvariantCultureIgnoreCase));
            }

            foreach (string CookieKey in CookieKeys)
            {
                if (!StripSession || !CookieKey.Equals(_SessionIDCookie, StringComparison.InvariantCultureIgnoreCase))
                {
                    var Cookie = CurrentRequest.Cookies[CookieKey];

                    // CookieContainer requires a domain. so if it's empty, then use the Current Request's Host.
                    string Domain = string.IsNullOrWhiteSpace(Cookie.Domain) ? CurrentRequest.Url.Host : Cookie.Domain;

                    if (!string.IsNullOrWhiteSpace(Domain))
                    {
	                    cookieJar.Add(new Cookie(CookieKey, Cookie.Value, Cookie.Path, Domain) { Port = CurrentRequest.IsLocal ? $@"""{CurrentRequest.Url.Port}""" : "" });
                    }
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
